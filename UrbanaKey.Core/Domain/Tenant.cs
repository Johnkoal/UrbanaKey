using System;
using UrbanaKey.Core.Common;

namespace UrbanaKey.Core.Domain;

public class Tenant
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Subdomain { get; set; } = string.Empty; // Indexed / Unique
    public string Nit { get; set; } = string.Empty;
    public DateTime SubscriptionExpiry { get; set; }
    public bool IsActive { get; set; }
    public string PlanType { get; set; } = string.Empty;
    public int MaxWarnings { get; set; } = 3; // Default limit
}
