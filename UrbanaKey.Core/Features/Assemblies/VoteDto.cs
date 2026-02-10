using System;

namespace UrbanaKey.Core.Features.Assemblies;

public class VoteDto
{
    public Guid AssemblyId { get; set; }
    public Guid TenantId { get; set; }
    public Guid AgendaItemId { get; set; }
    public Guid UnitId { get; set; }
    public string Option { get; set; } = string.Empty; // SSoT: Option
    public decimal CoefficientAtTime { get; set; } // SSoT: Coeficiente
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
