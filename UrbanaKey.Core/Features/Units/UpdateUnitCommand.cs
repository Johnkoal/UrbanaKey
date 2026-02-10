using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;
using UrbanaKey.Core.Interfaces;

namespace UrbanaKey.Core.Features.Units;

public record UpdateUnitCommand(Guid Id, UpdateUnitRequest Request) : IRequest<bool>;

public class UpdateUnitHandler(IApplicationDbContext db) : IRequestHandler<UpdateUnitCommand, bool>
{
    public async Task<bool> Handle(UpdateUnitCommand command, CancellationToken ct)
    {
        var unit = await db.Units.FirstOrDefaultAsync(u => u.Id == command.Id, ct);
        if (unit == null) return false;

        unit.Identifier = command.Request.Identifier;
        unit.Coefficient = command.Request.Coefficient;
        unit.UnitType = command.Request.UnitType;
        unit.HasSanctions = command.Request.HasSanctions;

        await db.SaveChangesAsync(ct);
        return true;
    }
}
