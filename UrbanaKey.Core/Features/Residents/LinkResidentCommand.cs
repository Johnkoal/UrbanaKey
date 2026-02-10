using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;
using UrbanaKey.Core.Domain;
using UrbanaKey.Core.Interfaces;

namespace UrbanaKey.Core.Features.Residents;

public record LinkResidentCommand(LinkResidentRequest Request) : IRequest<Guid>;

public class LinkResidentHandler(IApplicationDbContext db, ITenantProvider tenantProvider) 
    : IRequestHandler<LinkResidentCommand, Guid>
{
    public async Task<Guid> Handle(LinkResidentCommand command, CancellationToken ct)
    {
        var profile = new ResidentProfile
        {
            Id = Guid.NewGuid(),
            TenantId = tenantProvider.GetTenantId(),
            UserId = command.Request.UserId,
            UnitId = command.Request.UnitId,
            LinkType = command.Request.LinkType,
            IsResponsible = command.Request.IsResponsible
        };

        db.ResidentProfiles.Add(profile);
        await db.SaveChangesAsync(ct);
        return profile.Id;
    }
}
