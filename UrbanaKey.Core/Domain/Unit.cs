using System;
using UrbanaKey.Core.Common;

namespace UrbanaKey.Core.Domain;

public class Unit : ITenantEntity
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string Identifier { get; set; } = string.Empty; // e.g. "Apto 501"
    public decimal Coefficient { get; set; }
    public string UnitType { get; set; } = string.Empty; // Principal/Parqueadero
    public Guid? ParentUnitId { get; set; }
    public bool HasSanctions { get; set; }
}
