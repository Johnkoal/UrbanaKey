using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using UrbanaKey.Core.Features.Residents;
using UrbanaKey.Core.Domain;
using UrbanaKey.Core.Common;
using System;

namespace UrbanaKey.Api.Endpoints;

public static class ResidentEndpoints
{
    public static void MapResidentEndpoints(this IEndpointRouteBuilder app)
    {
        var residentsGroup = app.MapGroup("/api/admin/residents")
            .RequireAuthorization(policy => policy.RequireRole(UserRoles.Admin))
            .WithTags("Residents");

        residentsGroup.MapPost("/link", async (IMediator mediator, LinkResidentRequest request) => 
        {
            var id = await mediator.Send(new LinkResidentCommand(request));
            return Results.Ok(new { ProfileId = id });
        })
        .WithOpenApi(operation => new(operation)
        {
            Summary = "Vincular residente",
            Description = "Vincula un residente existente a una unidad habitacional."
        });

        residentsGroup.MapDelete("/{profileId}", async (Guid profileId, IMediator mediator) => 
        {
            var result = await mediator.Send(new UnlinkResidentCommand(profileId));
            return result ? Results.NoContent() : Results.NotFound();
        })
        .WithOpenApi(operation => new(operation)
        {
            Summary = "Desvincular residente",
            Description = "Desvincula un residente de una unidad habitacional."
        });
    }
}
