using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using UrbanaKey.Core.Features.Sanctions;
using UrbanaKey.Core.Domain;
using UrbanaKey.Core.Common;

namespace UrbanaKey.Api.Endpoints;

public static class SanctionEndpoints
{
    public static void MapSanctionEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/admin/sanctions")
            .WithTags("Sanctions");

        group.MapPost("/", async (CreateSanctionCommand command, IMediator mediator) =>
            Results.Ok(await mediator.Send(command)))
            .RequireAuthorization("AdminOnly")
            .WithOpenApi(operation => new(operation)
            {
                Summary = "Crear sanción",
                Description = "Registra una nueva sanción disciplinaria o económica para una unidad."
            });

    }
}
