using System;
using UrbanaKey.Core.Common;

namespace UrbanaKey.Core.Domain;
public class PQRS : ITenantEntity {
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid UnitId { get; set; }
    public Guid CreatedBy { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Status { get; set; } = "Open"; // Open, InProgress, Closed
    public bool IsPublic { get; set; } = false;
    public string? AttachmentUrl { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
