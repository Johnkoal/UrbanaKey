using MediatR;
using Microsoft.EntityFrameworkCore;
using UrbanaKey.Core.Domain;
using UrbanaKey.Core.Interfaces;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace UrbanaKey.Core.Features.Finance;

public record GenerateMonthlyInvoicesCommand(decimal StandardFee) : IRequest<int>;

public class GenerateMonthlyInvoicesHandler(IApplicationDbContext db, ITenantProvider tenantProvider) 
    : IRequestHandler<GenerateMonthlyInvoicesCommand, int>
{
    public async Task<int> Handle(GenerateMonthlyInvoicesCommand command, CancellationToken ct)
    {
        var tenantId = tenantProvider.GetTenantId();
        var units = await db.Units.ToListAsync(ct);
        
        var invoices = units.Select(u => new Invoice
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            UnitId = u.Id,
            Amount = command.StandardFee * u.Coefficient, // Calculado por coeficiente
            Description = $"Cuota de Administración - {DateTime.Now:MMMM yyyy}",
            DueDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 10), // Vence el 10
            IsPaid = false,
            CreatedAt = DateTime.UtcNow
        }).ToList();

        db.Invoices.AddRange(invoices);
        return await db.SaveChangesAsync(ct);
    }
}
