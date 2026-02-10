using System;
using UrbanaKey.Core.Common;

namespace UrbanaKey.Core.Domain;
public class ResidentProfile : ITenantEntity {
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid UnitId { get; set; }
    public Guid UserId { get; set; }
    public string LinkType { get; set; } = "Residente"; // Propietario, Arrendatario, Familiar
    public bool IsResponsible { get; set; } = false;
}
