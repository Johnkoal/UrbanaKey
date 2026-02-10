using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using UrbanaKey.Infrastructure.Persistence;

namespace UrbanaKey.Api.Middleware;

public class TenantResolverMiddleware
{
    private readonly RequestDelegate _next;

    public TenantResolverMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, UrbanaKeyDbContext dbContext)
    {
        var host = context.Request.Host.Host;
        var subdomain = host.Split('.')[0];

        var tenant = await dbContext.Tenants
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Subdomain == subdomain);

        if (tenant != null)
        {
            context.Items["TenantId"] = tenant.Id;
        }

        await _next(context);
    }
}
