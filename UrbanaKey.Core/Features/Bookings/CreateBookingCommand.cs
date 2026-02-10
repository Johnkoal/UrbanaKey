using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;
using UrbanaKey.Core.Domain;
using UrbanaKey.Core.Interfaces;

namespace UrbanaKey.Core.Features.Bookings;

public record CreateBookingRequest(Guid CommonAreaId, Guid UnitId, DateTime StartTime, DateTime EndTime);
public record CreateBookingCommand(CreateBookingRequest Request, Guid UserId) : IRequest<Guid?>;

public class CreateBookingHandler(IApplicationDbContext db, ITenantProvider tenantProvider) 
    : IRequestHandler<CreateBookingCommand, Guid?>
{
    public async Task<Guid?> Handle(CreateBookingCommand command, CancellationToken ct)
    {
        // 1. Validar estado de la unidad (Sanciones/Deudas)
        var unit = await db.Units
            .FirstOrDefaultAsync(u => u.Id == command.Request.UnitId, ct);

        // Bloqueo si la unidad tiene el flag de sanciones activo
        if (unit == null || unit.HasSanctions)
        {
            return null; // O lanzar una excepción personalizada de negocio
        }

        // 2. Verificar específicamente si hay sanciones vigentes en la tabla de sanciones
        var activeSanction = await db.Sanctions
            .AnyAsync(s => s.UnitId == command.Request.UnitId && s.IsActive && s.ExpiryDate > DateTime.UtcNow, ct);

        if (activeSanction)
        {
            return null;
        }
        // 3. Verificar disponibilidad (no traslapes)
        var hasConflict = await db.AmenityBookings.AnyAsync(b => 
            b.CommonAreaId == command.Request.CommonAreaId &&
            b.Status == "Reserved" &&
            ((command.Request.StartTime >= b.StartTime && command.Request.StartTime < b.EndTime) ||
             (command.Request.EndTime > b.StartTime && command.Request.EndTime <= b.EndTime) ||
             (command.Request.StartTime <= b.StartTime && command.Request.EndTime >= b.EndTime)), // Covers case where new booking encloses existing one
            ct);

        if (hasConflict) return null;

        // 4. Crear reserva
        var booking = new AmenityBooking
        {
            Id = Guid.NewGuid(),
            TenantId = tenantProvider.GetTenantId(),
            CommonAreaId = command.Request.CommonAreaId,
            UserId = command.UserId,
            UnitId = command.Request.UnitId,
            StartTime = command.Request.StartTime,
            EndTime = command.Request.EndTime
        };

        db.AmenityBookings.Add(booking);
        await db.SaveChangesAsync(ct);
        return booking.Id;
    }
}
