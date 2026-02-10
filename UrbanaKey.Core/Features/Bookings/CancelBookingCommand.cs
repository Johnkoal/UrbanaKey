using MediatR;
using Microsoft.EntityFrameworkCore;
using UrbanaKey.Core.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace UrbanaKey.Core.Features.Bookings;

public record CancelBookingCommand(Guid BookingId, Guid UserId, bool IsAdmin) : IRequest<bool>;

public class CancelBookingHandler(IApplicationDbContext db) : IRequestHandler<CancelBookingCommand, bool>
{
    public async Task<bool> Handle(CancelBookingCommand command, CancellationToken ct)
    {
        var booking = await db.AmenityBookings
            .FirstOrDefaultAsync(b => b.Id == command.BookingId, ct);

        if (booking == null) return false;

        // Validación: Solo el dueño de la reserva o un administrador pueden cancelarla
        if (!command.IsAdmin && booking.UserId != command.UserId)
            return false;

        booking.Status = "Cancelled";
        await db.SaveChangesAsync(ct);
        return true;
    }
}
