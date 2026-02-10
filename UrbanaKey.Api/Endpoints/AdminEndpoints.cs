using MediatR;
using UrbanaKey.Core.Features.Admin;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using UrbanaKey.Core.Domain;
using UrbanaKey.Core.Common;
using UrbanaKey.Infrastructure.Persistence;
using UrbanaKey.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System;

namespace UrbanaKey.Api.Endpoints;

public static class AdminEndpoints
{
    public static void MapAdminEndpoints(this IEndpointRouteBuilder app)
    {
        var adminGroup = app.MapGroup("/api/admin")
            .RequireAuthorization(policy => policy.RequireRole(UserRoles.Admin))
            .WithTags("Admin");

        adminGroup.MapPost("/users/{id}/approve", async (Guid id, IMediator mediator) => 
        {
            var success = await mediator.Send(new ApproveUserCommand(id));
            return success ? Results.Ok() : Results.BadRequest("No se pudo aprobar el usuario.");
        })
        .WithOpenApi(operation => new(operation)
        {
            Summary = "Aprobar usuario",
            Description = "Aprueba un usuario pendiente de activación por su ID."
        });

        adminGroup.MapGet("/logs", async (IMediator mediator) => 
            Results.Ok(await mediator.Send(new GetAuditLogsQuery())))
            .RequireAuthorization("AdminOnly")
            .WithOpenApi(operation => new(operation)
            {
                Summary = "Consultar logs de auditoría",
                Description = "Recupera el historial de cambios y acciones críticas realizadas en el sistema."
            });

        adminGroup.MapPost("/approve-user", async (ApproveUserCommand command, IMediator mediator) => 
            Results.Ok(await mediator.Send(command)))
            .RequireAuthorization("AdminOnly")
            .WithOpenApi(operation => new(operation)
            {
                Summary = "Aprobar nuevo usuario",
                Description = "Habilita el acceso a un usuario previamente registrado tras verificación administrativa."
            });

        adminGroup.MapGet("/tenants", async (UrbanaKeyDbContext db) => 
            Results.Ok(await db.Tenants.ToListAsync()))
            .RequireAuthorization("AdminOnly")
            .WithOpenApi(operation => new(operation)
            {
                Summary = "Listar inquilinos (Copropiedades)",
                Description = "Lista todas las administraciones/copropiedades registradas en el sistema global."
            });

        // Listado de usuarios pendientes
        var usersGroup = app.MapGroup("/api/admin/users")
            .RequireAuthorization(policy => policy.RequireRole(UserRoles.Admin))
            .WithTags("Admin Users");

        usersGroup.MapGet("/pending", async (UrbanaKeyDbContext db, ITenantProvider tenantProvider) => 
        {
            var tenantId = tenantProvider.GetTenantId();
            return await db.Users
                .Where(u => u.TenantId == tenantId && !u.IsActive)
                .ToListAsync();
        })
        .WithOpenApi(operation => new(operation)
        {
            Summary = "Listar usuarios pendientes",
            Description = "Obtiene una lista de usuarios que aún no han sido aprobados para el tenant actual."
        });
    }
}
