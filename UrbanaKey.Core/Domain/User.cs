using System;
using Microsoft.AspNetCore.Identity;
using UrbanaKey.Core.Common;

namespace UrbanaKey.Core.Domain;

public class User : IdentityUser<Guid>, ITenantEntity
{
    public Guid TenantId { get; set; } // ITenantEntity implementation
    public string FullName { get; set; } = string.Empty;
    public string? SecurityPin { get; set; } // Hash
    public bool IsActive { get; set; }
}
