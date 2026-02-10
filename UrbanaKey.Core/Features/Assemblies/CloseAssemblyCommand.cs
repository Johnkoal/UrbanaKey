using MediatR;
using Microsoft.EntityFrameworkCore;
using UrbanaKey.Core.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace UrbanaKey.Core.Features.Assemblies;

public record CloseAssemblyCommand(Guid AssemblyId) : IRequest<bool>;

public class CloseAssemblyHandler(IApplicationDbContext db) : IRequestHandler<CloseAssemblyCommand, bool>
{
    public async Task<bool> Handle(CloseAssemblyCommand request, CancellationToken ct)
    {
        var assembly = await db.Assemblies.FirstOrDefaultAsync(a => a.Id == request.AssemblyId, ct);
        if (assembly == null) return false;

        assembly.IsActive = false; // Bloquea futuros votos
        // Note: EndTime property was not in Domain.Assembly, let's stick to IsActive for now unless we add it.
        // Looking back at local check: Domain/Assembly.cs only has Id, TenantId, Name, Date, MinQuorumPercentage, IsActive.

        await db.SaveChangesAsync(ct);
        return true;
    }
}
