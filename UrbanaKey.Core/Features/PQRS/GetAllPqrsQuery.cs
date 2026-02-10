using MediatR;
using Microsoft.EntityFrameworkCore;
using UrbanaKey.Core.Interfaces;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace UrbanaKey.Core.Features.PQRS;

public record GetAllPqrsQuery() : IRequest<List<PqrResponse>>;

public class GetAllPqrsHandler(IApplicationDbContext db) : IRequestHandler<GetAllPqrsQuery, List<PqrResponse>>
{
    public async Task<List<PqrResponse>> Handle(GetAllPqrsQuery request, CancellationToken ct)
    {
        // Global Query Filter handles filtering by current Tenant automatically
        return await db.PQRS
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new PqrResponse(x.Id, x.Title, x.Description, x.Status, x.CreatedAt, x.IsPublic))
            .ToListAsync(ct);
    }
}
