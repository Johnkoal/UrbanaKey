using MediatR;
using Microsoft.EntityFrameworkCore;
using UrbanaKey.Core.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace UrbanaKey.Core.Features.PQRS;

public record UpdatePqrStatusCommand(Guid PqrId, string Status) : IRequest<bool>;

public class UpdatePqrStatusHandler(IApplicationDbContext db) : IRequestHandler<UpdatePqrStatusCommand, bool>
{
    public async Task<bool> Handle(UpdatePqrStatusCommand command, CancellationToken ct)
    {
        var pqr = await db.PQRS.FirstOrDefaultAsync(x => x.Id == command.PqrId, ct);
        if (pqr == null) return false;

        pqr.Status = command.Status;
        await db.SaveChangesAsync(ct);
        return true;
    }
}
