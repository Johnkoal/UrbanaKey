using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;
using UrbanaKey.Core.Domain;
using UrbanaKey.Core.Interfaces;

namespace UrbanaKey.Core.Features.Sanctions;

public record CreateSanctionRequest(Guid UnitId, string Type, string Description, int DurationDays);
public record CreateSanctionCommand(CreateSanctionRequest Request) : IRequest<Guid>;

public class CreateSanctionHandler(IApplicationDbContext db, ITenantProvider tenantProvider) 
    : IRequestHandler<CreateSanctionCommand, Guid>
{
    public async Task<Guid> Handle(CreateSanctionCommand command, CancellationToken ct)
    {
        var sanction = new Sanction
        {
            Id = Guid.NewGuid(),
            TenantId = tenantProvider.GetTenantId(),
            UnitId = command.Request.UnitId,
            Type = command.Request.Type,
            Description = command.Request.Description,
            StartDate = DateTime.UtcNow,
            ExpiryDate = DateTime.UtcNow.AddDays(command.Request.DurationDays),
            IsActive = true
        };

        db.Sanctions.Add(sanction);
        
        // Actualizar estado de la unidad automáticamente
        var unit = await db.Units.FindAsync([command.Request.UnitId], ct);
        if (unit != null)
        {
            unit.HasSanctions = true;
        }

        await db.SaveChangesAsync(ct);
        return sanction.Id;
    }
}
