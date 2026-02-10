using System;
using UrbanaKey.Core.Common;

namespace UrbanaKey.Core.Domain;

public class AmenityBooking : ITenantEntity
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid CommonAreaId { get; set; }
    public Guid UserId { get; set; }
    public Guid UnitId { get; set; } // Unidad que realiza la reserva
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public string Status { get; set; } = "Reserved"; // Reserved, Cancelled, Completed
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
