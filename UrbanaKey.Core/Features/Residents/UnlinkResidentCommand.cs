using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;
using UrbanaKey.Core.Interfaces;

namespace UrbanaKey.Core.Features.Residents;

public record UnlinkResidentCommand(Guid ProfileId) : IRequest<bool>;

public class UnlinkResidentHandler(IApplicationDbContext db) : IRequestHandler<UnlinkResidentCommand, bool>
{
    public async Task<bool> Handle(UnlinkResidentCommand command, CancellationToken ct)
    {
        var profile = await db.ResidentProfiles.FirstOrDefaultAsync(p => p.Id == command.ProfileId, ct);
        if (profile == null) return false;

        db.ResidentProfiles.Remove(profile);
        await db.SaveChangesAsync(ct);
        return true;
    }
}
