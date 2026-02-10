using System;
using UrbanaKey.Core.Common;

namespace UrbanaKey.Core.Domain;

public class CommonArea : ITenantEntity
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string Name { get; set; } = string.Empty; // Ejemplo: "Salón Social"
    public string Description { get; set; } = string.Empty;
    public int Capacity { get; set; }
    public decimal HourlyRate { get; set; } // Costo por hora si aplica
    public bool IsActive { get; set; } = true;
}
