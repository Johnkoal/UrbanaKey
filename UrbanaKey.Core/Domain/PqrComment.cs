using System;
using UrbanaKey.Core.Common;

namespace UrbanaKey.Core.Domain;

public class PqrComment : ITenantEntity
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid PqrId { get; set; }
    public Guid UserId { get; set; }
    public string Message { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public string? AttachmentUrl { get; set; }
}
