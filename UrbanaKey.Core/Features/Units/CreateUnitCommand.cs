using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;
using UrbanaKey.Core.Domain;
using UrbanaKey.Core.Interfaces;

namespace UrbanaKey.Core.Features.Units;

public record CreateUnitCommand(CreateUnitRequest Request) : IRequest<Guid>;

public class CreateUnitHandler(IApplicationDbContext db, ITenantProvider tenantProvider) 
    : IRequestHandler<CreateUnitCommand, Guid>
{
    public async Task<Guid> Handle(CreateUnitCommand command, CancellationToken ct)
    {
        var unit = new Domain.Unit
        {
            Id = Guid.NewGuid(),
            TenantId = tenantProvider.GetTenantId(), // Asignación automática de tenant
            Identifier = command.Request.Identifier,
            Coefficient = command.Request.Coefficient,
            UnitType = command.Request.UnitType,
            ParentUnitId = command.Request.ParentUnitId,
            HasSanctions = false
        };

        db.Units.Add(unit);
        await db.SaveChangesAsync(ct);
        return unit.Id;
    }
}
