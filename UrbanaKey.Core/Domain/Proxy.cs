using System;
using UrbanaKey.Core.Common;

namespace UrbanaKey.Core.Domain;
public class Proxy : ITenantEntity {
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid AssemblyId { get; set; }
    public Guid GrantorUnitId { get; set; } // Unidad que delega
    public Guid RepresentativeUserId { get; set; } // Usuario que recibe el poder
    public string DocumentPath { get; set; } = string.Empty; // URL en Cloudflare R2
}
