using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using UrbanaKey.Core.Features.PQRS;
using UrbanaKey.Core.Domain;
using UrbanaKey.Core.Common;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Linq;
using System;

namespace UrbanaKey.Api.Endpoints;

public static class PqrEndpoints
{
    public static void MapPqrEndpoints(this IEndpointRouteBuilder app)
    {
        var pqrGroup = app.MapGroup("/api/pqr")
            .RequireAuthorization()
            .WithTags("PQR");

        pqrGroup.MapGet("/", async (IMediator mediator, ClaimsPrincipal user) => 
        {
            var userId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var results = await mediator.Send(new GetMyPqrsQuery(userId));
            return Results.Ok(results);
        })
        .WithOpenApi(operation => new(operation)
        {
            Summary = "Listar mis PQRS",
            Description = "Obtiene las solicitudes de PQR registradas por el usuario autenticado."
        });

        pqrGroup.MapGet("/public", async (IMediator mediator) => 
        {
            return Results.Ok(await mediator.Send(new GetPublicPqrsQuery()));
        })
        .WithOpenApi(operation => new(operation)
        {
            Summary = "Listar PQRS públicas",
            Description = "Obtiene todas las solicitudes de PQR marcadas como públicas."
        });

        pqrGroup.MapPost("/", async (
            IMediator mediator, 
            ClaimsPrincipal user, 
            HttpRequest request
            ) => 
        {
            var userId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var form = await request.ReadFormAsync();
            
            var attachments = form.Files.Select(f => f.OpenReadStream()).ToList();
            
            var pqrRequest = new CreatePqrRequest(
                form["title"]!, 
                form["description"]!, 
                Guid.Parse(form["unitId"]!), 
                bool.Parse(form["isPublic"]!),
                null, // attachmentUrl
                attachments);

            var id = await mediator.Send(new CreatePqrCommand(pqrRequest, userId));
            return Results.Created($"/api/pqr/{id}", new { Id = id });
        })
        .DisableAntiforgery()
        .WithOpenApi(operation => new(operation)
        {
            Summary = "Crear PQRS",
            Description = "Permite a un residente registrar una nueva Petición, Queja, Reclamo o Sugerencia con archivos adjuntos."
        }); 

        pqrGroup.MapPost("/{id}/comments", async (Guid id, AddPqrCommentRequest request, IMediator mediator, ClaimsPrincipal user) => 
        {
            var userId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var commentId = await mediator.Send(new AddPqrCommentCommand(request with { PqrId = id }, userId));
            return Results.Ok(new { Id = commentId });
        })
        .WithOpenApi(operation => new(operation)
        {
            Summary = "Agregar comentario a PQRS",
            Description = "Permite la interacción entre el residente y la administración sobre una solicitud existente."
        });

        // Admin PQR Group
        var adminPqrGroup = app.MapGroup("/api/admin/pqr")
            .RequireAuthorization(UserRoles.Admin)
            .WithTags("Admin PQR");

        adminPqrGroup.MapGet("/", async (IMediator mediator) => 
        {
            return Results.Ok(await mediator.Send(new GetAllPqrsQuery()));
        })
        .WithOpenApi(operation => new(operation)
        {
            Summary = "Listar todas las PQRS (Admin)",
            Description = "Obtiene el listado completo de todas las PQRS de la copropiedad para su gestión."
        });

        adminPqrGroup.MapPut("/{id}/status", async (IMediator mediator, Guid id, [FromBody] string status) => 
        {
            var success = await mediator.Send(new UpdatePqrStatusCommand(id, status));
            return success ? Results.NoContent() : Results.NotFound();
        })
        .WithOpenApi(operation => new(operation)
        {
            Summary = "Actualizar estado de PQRS (Admin)",
            Description = "Permite a la administración cambiar el estado de una solicitud (ej. Pendiente -> Resuelta)."
        });
    }
}
