using System;
using UrbanaKey.Core.Common;

namespace UrbanaKey.Core.Domain;

public class Payment : ITenantEntity
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid UnitId { get; set; }
    public decimal Amount { get; set; }
    public string Reference { get; set; } = string.Empty; // Ej. número de consignación o pasarela
    public DateTime PaymentDate { get; set; } = DateTime.UtcNow;
}
