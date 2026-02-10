using System;
using UrbanaKey.Core.Common;

namespace UrbanaKey.Core.Domain;

public class Vote : ITenantEntity
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; } // ITenantEntity
    public Guid AssemblyId { get; set; }
    public Guid AgendaItemId { get; set; }
    public Guid UnitId { get; set; }
    public string Option { get; set; } = string.Empty; // SSoT: Option (Yes/No/Abs)
    public decimal CoefficientAtTime { get; set; } // SSoT: Coeficiente
    public DateTime Timestamp { get; set; }
}
