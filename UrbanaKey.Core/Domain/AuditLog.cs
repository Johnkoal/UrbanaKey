using System;
using UrbanaKey.Core.Common;

namespace UrbanaKey.Core.Domain;

public class AuditLog : ITenantEntity
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string EntityName { get; set; } = string.Empty;
    public string OldValues { get; set; } = string.Empty; // Json
    public string NewValues { get; set; } = string.Empty; // Json
    public DateTime Timestamp { get; set; }
}
