using System;
using UrbanaKey.Core.Common;

namespace UrbanaKey.Core.Domain;

public class Warning : ITenantEntity
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid UnitId { get; set; }
    public string Reason { get; set; } = string.Empty;
    public DateTime Date { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;
}
