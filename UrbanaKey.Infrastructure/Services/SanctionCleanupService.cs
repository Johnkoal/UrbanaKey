using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using UrbanaKey.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;

namespace UrbanaKey.Infrastructure.Services;

public class SanctionCleanupService(IServiceProvider serviceProvider) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            using (var scope = serviceProvider.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<UrbanaKeyDbContext>();
                
                // Buscar sanciones expiradas y desactivarlas
                // Usamos IgnoreQueryFilters() para asegurar que procesamos todas las sanciones de todos los inquilinos
                var expiredSanctions = await db.Sanctions
                    .IgnoreQueryFilters()
                    .Where(s => s.IsActive && s.ExpiryDate <= DateTime.UtcNow)
                    .ToListAsync(ct);

                foreach (var sanction in expiredSanctions)
                {
                    sanction.IsActive = false;
                    
                    // Verificar si la unidad ya no tiene otras sanciones activas para limpiar el flag en Unit.cs
                    var otherActive = await db.Sanctions
                        .IgnoreQueryFilters()
                        .AnyAsync(s => s.UnitId == sanction.UnitId && s.IsActive && s.Id != sanction.Id, ct);
                        
                    if (!otherActive)
                    {
                        var unit = await db.Units
                            .IgnoreQueryFilters()
                            .FirstOrDefaultAsync(u => u.Id == sanction.UnitId, ct);
                        if (unit != null) unit.HasSanctions = false;
                    }
                }

                await db.SaveChangesAsync(ct);
            }
            await Task.Delay(TimeSpan.FromHours(1), ct); // Ejecutar cada hora
        }
    }
}
