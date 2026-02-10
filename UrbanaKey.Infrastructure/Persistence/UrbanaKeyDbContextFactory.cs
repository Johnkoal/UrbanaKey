
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using UrbanaKey.Core.Interfaces;
using UrbanaKey.Infrastructure.Persistence.Interceptors;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace UrbanaKey.Infrastructure.Persistence;

public class UrbanaKeyDbContextFactory : IDesignTimeDbContextFactory<UrbanaKeyDbContext>
{
    public UrbanaKeyDbContext CreateDbContext(string[] args)
    {
        // Build configuration
        IConfigurationRoot configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true)
            // Fallback for when running from Infrastructure folder but appsettings is in Api
            .AddJsonFile(Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory())?.FullName ?? "", "UrbanaKey.Api", "appsettings.json"), optional: true)
            .Build();

        var builder = new DbContextOptionsBuilder<UrbanaKeyDbContext>();
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        
        // Fallback if config is missing (e.g. CI or weird path)
        if (string.IsNullOrEmpty(connectionString))
        {
            connectionString = "Server=localhost;Port=3306;Database=urbanakey;User=root;Password=example;Charset=utf8mb4;";
        }

        builder.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));

        return new UrbanaKeyDbContext(
            builder.Options, 
            new DesignTimeTenantProvider(), 
            new AuditableEntityInterceptor(new DesignTimeTenantProvider(), new DesignTimeHttpContextAccessor())
        );
    }

    private class DesignTimeTenantProvider : ITenantProvider
    {
        public Guid GetTenantId() => Guid.Empty;
        public void SetTenantId(Guid tenantId) { }
    }

    private class DesignTimeHttpContextAccessor : IHttpContextAccessor
    {
        public HttpContext? HttpContext { get; set; }
    }
}
