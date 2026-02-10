using System;
using UrbanaKey.Core.Common;

namespace UrbanaKey.Core.Domain;
public class EmergencyAlert : ITenantEntity {
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid UnitId { get; set; }
    public Guid UserId { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string Status { get; set; } = "Active"; // Active, Attended, FalseAlarm
}
