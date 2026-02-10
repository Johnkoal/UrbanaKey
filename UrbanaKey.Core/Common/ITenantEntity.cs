using System;

namespace UrbanaKey.Core.Common;

public interface ITenantEntity
{
    Guid TenantId { get; set; }
}
