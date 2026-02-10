using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UrbanaKey.Core.Interfaces;

namespace UrbanaKey.Core.Features.Units;

public record GetUnitsQuery() : IRequest<List<UnitResponse>>;

public class GetUnitsHandler(IApplicationDbContext db) : IRequestHandler<GetUnitsQuery, List<UnitResponse>>
{
    public async Task<List<UnitResponse>> Handle(GetUnitsQuery request, CancellationToken ct)
    {
        return await db.Units
            .Select(u => new UnitResponse(u.Id, u.Identifier, u.Coefficient, u.UnitType, u.HasSanctions, u.ParentUnitId))
            .ToListAsync(ct);
    }
}
