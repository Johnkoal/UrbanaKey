using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;
using UrbanaKey.Core.Interfaces;

namespace UrbanaKey.Core.Features.Units;

public record DeleteUnitCommand(Guid Id) : IRequest<bool>;

public class DeleteUnitHandler(IApplicationDbContext db) : IRequestHandler<DeleteUnitCommand, bool>
{
    public async Task<bool> Handle(DeleteUnitCommand command, CancellationToken ct)
    {
        var unit = await db.Units.FirstOrDefaultAsync(u => u.Id == command.Id, ct);
        if (unit == null) return false;

        // Opcional: Verificar si hay residentes vinculados antes de eliminar
        var hasResidents = await db.ResidentProfiles.AnyAsync(rp => rp.UnitId == command.Id, ct);
        if (hasResidents) return false;

        db.Units.Remove(unit);
        await db.SaveChangesAsync(ct);
        return true;
    }
}
