using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using UrbanaKey.Core.Features.Units;
using UrbanaKey.Core.Common;
using System;
using Microsoft.AspNetCore.Builder;

namespace UrbanaKey.Api.Endpoints;

public static class UnitEndpoints
{
    public static void MapUnitEndpoints(this IEndpointRouteBuilder app)
    {
        var unitsGroup = app.MapGroup("/api/units")
            .RequireAuthorization(policy => policy.RequireRole(UserRoles.Admin))
            .WithTags("Units");

        unitsGroup.MapGet("/", async (IMediator mediator) => 
            Results.Ok(await mediator.Send(new GetUnitsQuery())))
            .WithOpenApi(operation => new(operation)
            {
                Summary = "Obtener todas las unidades",
                Description = "Retorna una lista de todas las unidades habitacionales registradas para el inquilino actual."
            });

        unitsGroup.MapPost("/", async (IMediator mediator, CreateUnitRequest request) => 
        {
            var id = await mediator.Send(new CreateUnitCommand(request));
            return Results.Created($"/api/units/{id}", new { Id = id });
        })
        .WithOpenApi(operation => new(operation)
        {
            Summary = "Crear una nueva unidad",
            Description = "Registra una nueva unidad (apartamento/casa) en el sistema."
        });

        unitsGroup.MapPut("/{id}", async (Guid id, UpdateUnitRequest request, IMediator mediator) => 
        {
            var result = await mediator.Send(new UpdateUnitCommand(id, request));
            return result ? Results.NoContent() : Results.NotFound();
        })
        .WithOpenApi(operation => new(operation)
        {
            Summary = "Actualizar unidad",
            Description = "Modifica la información de una unidad existente."
        });

        unitsGroup.MapDelete("/{id}", async (Guid id, IMediator mediator) => 
        {
            var result = await mediator.Send(new DeleteUnitCommand(id));
            return result ? Results.NoContent() : Results.BadRequest("No se puede eliminar la unidad (verifique si hay residentes vinculados).");
        })
        .WithOpenApi(operation => new(operation)
        {
            Summary = "Eliminar unidad",
            Description = "Elimina permanentemente una unidad si no tiene residentes asociados."
        });

        unitsGroup.MapPost("/import", async (IMediator mediator, IFormFile file) => 
        {
            if (file == null || file.Length == 0)
                return Results.BadRequest("El archivo está vacío.");

            using var stream = file.OpenReadStream();
            var count = await mediator.Send(new ImportUnitsCommand(stream));
            return Results.Ok(new { ImportedCount = count });
        })
        .DisableAntiforgery()
        .WithOpenApi(operation => new(operation)
        {
            Summary = "Importar unidades masivamente",
            Description = "Permite cargar múltiples unidades desde un archivo (Excel/CSV)."
        });
    }
}
