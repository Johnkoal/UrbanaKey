using Microsoft.AspNetCore.SignalR;
using UrbanaKey.Infrastructure.Hubs;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using UrbanaKey.Core.Domain;
using UrbanaKey.Core.Features.Assemblies;
using UrbanaKey.Core.Interfaces;
using UrbanaKey.Infrastructure.Persistence;

namespace UrbanaKey.Infrastructure.Services;

public class VoteBackgroundService : BackgroundService
{
    private readonly IVoteChannel _voteChannel; // Needs to expose Reader 
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<VoteBackgroundService> _logger;
    private const int BatchSize = 100;
    private readonly TimeSpan _batchInterval = TimeSpan.FromSeconds(2);

    public VoteBackgroundService(IVoteChannel voteChannel, IServiceProvider serviceProvider, ILogger<VoteBackgroundService> logger)
    {
        _voteChannel = voteChannel;
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var batch = new List<VoteDto>();
        
        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                 while (_voteChannel.Reader.TryRead(out var vote))
                 {
                     batch.Add(vote);
                     if (batch.Count >= BatchSize) break;
                 }

                 if (batch.Count > 0)
                 {
                     await ProcessBatchAsync(batch, stoppingToken);
                     batch.Clear();
                 }

                 await Task.Delay(100, stoppingToken);
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("VoteBackgroundService is shutting down gracefully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred in VoteBackgroundService.");
        }
    }

    private async Task ProcessBatchAsync(List<VoteDto> voteDtos, CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<UrbanaKeyDbContext>();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        var hubContext = scope.ServiceProvider.GetRequiredService<IHubContext<AssemblyHub>>();
        
        var votes = new List<Vote>();
        foreach (var dto in voteDtos)
        {
             votes.Add(new Vote
             {
                 TenantId = dto.TenantId,
                 AssemblyId = dto.AssemblyId,
                 AgendaItemId = dto.AgendaItemId,
                 UnitId = dto.UnitId,
                 Option = dto.Option, // SSoT Mapped
                 CoefficientAtTime = dto.CoefficientAtTime, // SSoT Mapped
                 Timestamp = dto.Timestamp
             });
        }
        
        dbContext.Votes.AddRange(votes);
        await dbContext.SaveChangesAsync(cancellationToken);
        _logger.LogInformation($"Processed {votes.Count} votes.");

        // Notificar quórum por cada asamblea afectada en el lote
        var assemblyIds = voteDtos.Select(v => v.AssemblyId).Distinct();
        foreach (var assemblyId in assemblyIds)
        {
            var quorum = await mediator.Send(new GetQuorumQuery(assemblyId), cancellationToken);
            await hubContext.Clients.Group(assemblyId.ToString())
                .SendAsync("UpdateQuorum", quorum, cancellationToken);
        }
    }
}
