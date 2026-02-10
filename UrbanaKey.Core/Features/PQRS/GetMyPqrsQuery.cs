using MediatR;
using Microsoft.EntityFrameworkCore;
using UrbanaKey.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace UrbanaKey.Core.Features.PQRS;

public record GetMyPqrsQuery(Guid UserId) : IRequest<List<PqrResponse>>;

public class GetMyPqrsHandler(IApplicationDbContext db) : IRequestHandler<GetMyPqrsQuery, List<PqrResponse>>
{
    public async Task<List<PqrResponse>> Handle(GetMyPqrsQuery request, CancellationToken ct)
    {
        return await db.PQRS
            .Where(x => x.CreatedBy == request.UserId)
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new PqrResponse(x.Id, x.Title, x.Description, x.Status, x.CreatedAt, x.IsPublic, x.AttachmentUrl, x.AttachmentUrls))
            .ToListAsync(ct);
    }
}
