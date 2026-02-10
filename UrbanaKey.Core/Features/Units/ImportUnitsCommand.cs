using MediatR;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using UrbanaKey.Core.Domain;
using UrbanaKey.Core.Interfaces;

namespace UrbanaKey.Core.Features.Units;

public record ImportUnitsCommand(Stream FileStream) : IRequest<int>;

public class ImportUnitsHandler(IApplicationDbContext db, ITenantProvider tenantProvider) 
    : IRequestHandler<ImportUnitsCommand, int>
{
    public async Task<int> Handle(ImportUnitsCommand command, CancellationToken ct)
    {
        using var reader = new StreamReader(command.FileStream);
        var tenantId = tenantProvider.GetTenantId();
        var units = new List<Domain.Unit>();

        // Omitir encabezado (assumes first line is header)
        await reader.ReadLineAsync(ct);

        while (await reader.ReadLineAsync(ct) is { } line)
        {
            var parts = line.Split(',');
            if (parts.Length < 3) continue;

            // Basic parsing without error handling for simplicity
            // Format expected: Identifier,Coefficient,UnitType
            try 
            {
                var identifier = parts[0].Trim();
                if(string.IsNullOrEmpty(identifier)) continue;

                var unit = new Domain.Unit
                {
                    Id = Guid.NewGuid(),
                    TenantId = tenantId,
                    Identifier = identifier, // Apto
                    Coefficient = decimal.Parse(parts[1].Trim()), // Coeficiente
                    UnitType = parts[2].Trim(), // Tipo
                    HasSanctions = false
                };
                units.Add(unit);
            }
            catch
            {
                // Skip malformed lines
                continue;
            }
        }

        if(units.Count > 0)
        {
            db.Units.AddRange(units);
            await db.SaveChangesAsync(ct);
        }
        
        return units.Count;
    }
}
