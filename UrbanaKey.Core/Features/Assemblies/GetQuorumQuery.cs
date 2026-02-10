using MediatR;
using Microsoft.EntityFrameworkCore;
using UrbanaKey.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace UrbanaKey.Core.Features.Assemblies;

public record QuorumResponse(decimal CurrentQuorum, bool IsMinQuorumMet);
public record GetQuorumQuery(Guid AssemblyId) : IRequest<QuorumResponse>;

public class GetQuorumHandler(IApplicationDbContext db) : IRequestHandler<GetQuorumQuery, QuorumResponse>
{
    public async Task<QuorumResponse> Handle(GetQuorumQuery request, CancellationToken ct)
    {
        var assembly = await db.Assemblies.FirstOrDefaultAsync(a => a.Id == request.AssemblyId, ct);
        if (assembly == null) return new QuorumResponse(0, false);

        // Sumar coeficientes únicos de unidades que ya han votado en esta asamblea
        var currentQuorum = await db.Votes
            .Where(v => v.AssemblyId == request.AssemblyId)
            .Select(v => v.UnitId)
            .Distinct()
            .Join(db.Units, unitId => unitId, unit => unit.Id, (unitId, unit) => unit.Coefficient)
            .SumAsync(ct);

        return new QuorumResponse(currentQuorum, currentQuorum >= assembly.MinQuorumPercentage);
    }
}
