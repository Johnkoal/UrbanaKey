using MediatR;
using Microsoft.EntityFrameworkCore;
using UrbanaKey.Core.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace UrbanaKey.Core.Features.PQRS;

public record GetPublicPqrsQuery() : IRequest<List<PqrResponse>>;

public class GetPublicPqrsHandler(IApplicationDbContext db) : IRequestHandler<GetPublicPqrsQuery, List<PqrResponse>>
{
    public async Task<List<PqrResponse>> Handle(GetPublicPqrsQuery request, CancellationToken ct)
    {
        // Retrieves only public PQRS (respecting Tenant filter)
        return await db.PQRS
            .Where(x => x.IsPublic)
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new PqrResponse(x.Id, x.Title, x.Description, x.Status, x.CreatedAt, x.IsPublic, x.AttachmentUrl, x.AttachmentUrls))
            .ToListAsync(ct);
    }
}
