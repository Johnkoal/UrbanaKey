using System;

namespace UrbanaKey.Core.Interfaces;

public interface ITenantProvider
{
    Guid GetTenantId();
}
