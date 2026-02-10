using System;
using UrbanaKey.Core.Common;

namespace UrbanaKey.Core.Domain;
public class Assembly : ITenantEntity {
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public decimal MinQuorumPercentage { get; set; }
    public bool IsActive { get; set; }
}
