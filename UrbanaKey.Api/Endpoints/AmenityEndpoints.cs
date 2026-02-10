using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using UrbanaKey.Core.Features.Amenities;
using UrbanaKey.Core.Features.Bookings;
using UrbanaKey.Core.Domain;
using UrbanaKey.Core.Common;
using System.Security.Claims;
using System;

namespace UrbanaKey.Api.Endpoints;

public static class AmenityEndpoints
{
    public static void MapAmenityEndpoints(this IEndpointRouteBuilder app)
    {
        var amenitiesGroup = app.MapGroup("/api/amenities")
            .RequireAuthorization()
            .WithTags("Amenities");

        amenitiesGroup.MapPost("/", async (CreateCommonAreaRequest request, IMediator mediator) => 
        {
            var id = await mediator.Send(new CreateCommonAreaCommand(request));
            return Results.Created($"/api/amenities/{id}", new { Id = id });
        })
        .WithOpenApi(operation => new(operation)
        {
            Summary = "Crear área común",
            Description = "Crea una nueva área común o servicio que puede ser reservado."
        });

        amenitiesGroup.MapPut("/{id}", async (Guid id, CreateCommonAreaRequest request, bool isActive, IMediator mediator) => 
        {
            var result = await mediator.Send(new UpdateCommonAreaCommand(id, request, isActive));
            return result ? Results.NoContent() : Results.NotFound();
        })
        .RequireAuthorization(policy => policy.RequireRole(UserRoles.Admin))
        .WithOpenApi(operation => new(operation)
        {
            Summary = "Actualizar área común",
            Description = "Actualiza la información de un área común existente."
        });

        amenitiesGroup.MapPost("/book", async (CreateBookingCommand command, IMediator mediator) => 
            Results.Ok(await mediator.Send(command)))
            .RequireAuthorization("ResidentOnly")
            .WithOpenApi(operation => new(operation)
            {
                Summary = "Reservar área común",
                Description = "Realiza una solicitud de reserva para un área o servicio específico."
            });

        // Endpoint de Cancelación
        app.MapDelete("/api/amenities/bookings/{id}", async (Guid id, IMediator mediator, ClaimsPrincipal user) => 
        {
            var userId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var isAdmin = user.IsInRole(UserRoles.Admin);
            
            var result = await mediator.Send(new CancelBookingCommand(id, userId, isAdmin));
            return result ? Results.NoContent() : Results.BadRequest("No se pudo cancelar la reserva.");
        })
        .RequireAuthorization()
        .WithTags("Amenities")
        .WithOpenApi(operation => new(operation)
        {
            Summary = "Cancelar reserva",
            Description = "Permite a un residente cancelar su propia reserva o a un administrador cancelar cualquier reserva."
        });
    }
}
