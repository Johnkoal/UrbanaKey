using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using UrbanaKey.Core.Common;
using UrbanaKey.Core.Domain;
using UrbanaKey.Core.Interfaces;
using UrbanaKey.Infrastructure.Persistence.Interceptors;

namespace UrbanaKey.Infrastructure.Persistence;

public class UrbanaKeyDbContext : IdentityDbContext<User, IdentityRole<Guid>, Guid>
{
    private readonly ITenantProvider _tenantProvider;
    private readonly AuditableEntityInterceptor _auditableEntityInterceptor;

    public UrbanaKeyDbContext(
        DbContextOptions<UrbanaKeyDbContext> options, 
        ITenantProvider tenantProvider,
        AuditableEntityInterceptor auditableEntityInterceptor)
        : base(options)
    {
        _tenantProvider = tenantProvider;
        _auditableEntityInterceptor = auditableEntityInterceptor;
    }

    public DbSet<Tenant> Tenants { get; set; }
    public DbSet<Unit> Units { get; set; }
    public DbSet<AuditLog> AuditLogs { get; set; }
    public DbSet<Warning> Warnings { get; set; }
    public DbSet<Sanction> Sanctions { get; set; }
    public DbSet<Vote> Votes { get; set; }
    public DbSet<Assembly> Assemblies { get; set; }
    public DbSet<PQRS> PqrsEntries { get; set; }
    public DbSet<ResidentProfile> ResidentProfiles { get; set; }
    public DbSet<Proxy> Proxies { get; set; }
    public DbSet<EmergencyAlert> EmergencyAlerts { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.AddInterceptors(_auditableEntityInterceptor);
        base.OnConfiguring(optionsBuilder);
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder); // Important for Identity

        // Apply Global Query Filter for Multi-tenancy
        builder.Entity<User>().HasQueryFilter(e => e.TenantId == _tenantProvider.GetTenantId());
        builder.Entity<Unit>().HasQueryFilter(e => e.TenantId == _tenantProvider.GetTenantId());
        builder.Entity<AuditLog>().HasQueryFilter(e => e.TenantId == _tenantProvider.GetTenantId());
        builder.Entity<Warning>().HasQueryFilter(e => e.TenantId == _tenantProvider.GetTenantId());
        builder.Entity<Sanction>().HasQueryFilter(e => e.TenantId == _tenantProvider.GetTenantId());
        builder.Entity<Vote>().HasQueryFilter(e => e.TenantId == _tenantProvider.GetTenantId());
        builder.Entity<Assembly>().HasQueryFilter(e => e.TenantId == _tenantProvider.GetTenantId());
        builder.Entity<PQRS>().HasQueryFilter(e => e.TenantId == _tenantProvider.GetTenantId());
        builder.Entity<ResidentProfile>().HasQueryFilter(e => e.TenantId == _tenantProvider.GetTenantId());
        builder.Entity<Proxy>().HasQueryFilter(e => e.TenantId == _tenantProvider.GetTenantId());
        builder.Entity<EmergencyAlert>().HasQueryFilter(e => e.TenantId == _tenantProvider.GetTenantId());

        builder.Entity<Tenant>()
            .HasIndex(t => t.Subdomain)
            .IsUnique();
    }
}
