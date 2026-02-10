using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text.Json;
using UrbanaKey.Core.Common;
using UrbanaKey.Core.Domain;
using UrbanaKey.Core.Interfaces;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace UrbanaKey.Infrastructure.Persistence.Interceptors;

public class AuditableEntityInterceptor : SaveChangesInterceptor
{
    private readonly ITenantProvider _tenantProvider;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AuditableEntityInterceptor(ITenantProvider tenantProvider, IHttpContextAccessor httpContextAccessor)
    {
        _tenantProvider = tenantProvider;
        _httpContextAccessor = httpContextAccessor;
    }

    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        UpdateEntities(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        UpdateEntities(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void UpdateEntities(DbContext? context)
    {
        if (context == null) return;

        var tenantId = _tenantProvider.GetTenantId();
        var userId = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier) ?? "System";

        var entries = context.ChangeTracker.Entries<ITenantEntity>().ToList();
        var auditEntries = new List<AuditLog>();

        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Added)
            {
                if (entry.Entity.TenantId == Guid.Empty)
                {
                    entry.Entity.TenantId = tenantId;
                }
            }
            
            // Audit Log Generation
            // Skip AuditLog itself to prevent recursion loop
            if (entry.Entity is AuditLog) continue;

            if (entry.State == EntityState.Added || entry.State == EntityState.Modified || entry.State == EntityState.Deleted)
            {
                var audit = new AuditLog
                {
                    TenantId = tenantId != Guid.Empty ? tenantId : entry.Entity.TenantId,
                    UserId = userId,
                    EntityName = entry.Entity.GetType().Name,
                    Timestamp = DateTime.UtcNow,
                    Action = entry.State.ToString(),
                    // Id won't be available for Added entities until after save usually, but we can capture what we have.
                };
                
                // Quick serialization of values
                // Careful with circular references.
                try 
                {
                    if (entry.State == EntityState.Modified)
                    {
                        var oldValues = new Dictionary<string, object?>();
                        var newValues = new Dictionary<string, object?>();
                        
                        foreach(var property in entry.Properties)
                        {
                            if (property.IsModified)
                            {
                                oldValues[property.Metadata.Name] = property.OriginalValue;
                                newValues[property.Metadata.Name] = property.CurrentValue;
                            }
                        }
                        
                        audit.OldValues = JsonSerializer.Serialize(oldValues);
                        audit.NewValues = JsonSerializer.Serialize(newValues);
                    }
                    else if (entry.State == EntityState.Added)
                    {
                        var values = entry.Properties.ToDictionary(p => p.Metadata.Name, p => p.CurrentValue);
                        audit.NewValues = JsonSerializer.Serialize(values);
                    }
                    else if (entry.State == EntityState.Deleted)
                    {
                         var values = entry.Properties.ToDictionary(p => p.Metadata.Name, p => p.OriginalValue);
                         audit.OldValues = JsonSerializer.Serialize(values);
                    }
                }
                catch 
                {
                    // Fallback or ignore serialization errors
                    audit.NewValues = "Serialization Error";
                }
                
                auditEntries.Add(audit);
            }
        }
        
        // We cannot AddRange to the same context we are saving intercepting?
        // Actually we can, but they will be Added state.
        // It's safe if we don't infinitely recurse. We checked `if (entry.Entity is AuditLog) continue;`.
        
        if (auditEntries.Any())
        {
            context.Set<AuditLog>().AddRange(auditEntries);
        }
    }
}
