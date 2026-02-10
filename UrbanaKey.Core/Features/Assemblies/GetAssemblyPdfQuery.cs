using MediatR;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using UrbanaKey.Core.Interfaces;

namespace UrbanaKey.Core.Features.Assemblies;

public record GetAssemblyPdfQuery(Guid AssemblyId) : IRequest<byte[]>;

public class GetAssemblyPdfHandler(
    IApplicationDbContext db, 
    IPdfService pdfService) : IRequestHandler<GetAssemblyPdfQuery, byte[]>
{
    public async Task<byte[]> Handle(GetAssemblyPdfQuery request, CancellationToken ct)
    {
        var assembly = await db.Assemblies.FirstOrDefaultAsync(a => a.Id == request.AssemblyId, ct);
        if (assembly == null) throw new Exception("Asamblea no encontrada");

        var votes = await db.Votes.Where(v => v.AssemblyId == request.AssemblyId).ToListAsync(ct);
        
        return pdfService.GenerateAssemblyReport(assembly, votes);
    }
}
