using MediatR;
using UrbanaKey.Core.Domain;
using UrbanaKey.Core.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace UrbanaKey.Core.Features.Amenities;

public record CreateCommonAreaRequest(string Name, string Description, int Capacity, decimal HourlyRate);
public record CreateCommonAreaCommand(CreateCommonAreaRequest Request) : IRequest<Guid>;

public class CreateCommonAreaHandler(IApplicationDbContext db, ITenantProvider tenantProvider) 
    : IRequestHandler<CreateCommonAreaCommand, Guid>
{
    public async Task<Guid> Handle(CreateCommonAreaCommand command, CancellationToken ct)
    {
        var area = new CommonArea
        {
            Id = Guid.NewGuid(),
            TenantId = tenantProvider.GetTenantId(),
            Name = command.Request.Name,
            Description = command.Request.Description,
            Capacity = command.Request.Capacity,
            HourlyRate = command.Request.HourlyRate,
            IsActive = true
        };

        db.CommonAreas.Add(area);
        await db.SaveChangesAsync(ct);
        return area.Id;
    }
}
