using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using UrbanaKey.Core.Features.Assemblies;
using UrbanaKey.Core.Domain;
using UrbanaKey.Core.Common;
using System;

namespace UrbanaKey.Api.Endpoints;

public static class AssemblyEndpoints
{
    public static void MapAssemblyEndpoints(this IEndpointRouteBuilder app)
    {
        var assembliesGroup = app.MapGroup("/api/assemblies")
            .RequireAuthorization()
            .WithTags("Assemblies");



        assembliesGroup.MapPost("/{id}/vote", async (Guid id, VoteDto voteDto, IMediator mediator) =>
        {
            voteDto.AssemblyId = id;
            await mediator.Send(new CastVoteCommand(voteDto));
            return Results.Accepted();
        })
        .RequireAuthorization("ResidentOnly")
        .WithOpenApi(operation => new(operation)
        {
            Summary = "Emitir voto",
            Description = "Permite a un residente registrar su voto en un punto de la agenda de la asamblea."
        });

        assembliesGroup.MapGet("/{id}/quorum", async (Guid id, IMediator mediator) =>
            Results.Ok(await mediator.Send(new GetQuorumQuery(id))))
            .RequireAuthorization("StaffOnly") // Changed from default to "StaffOnly"
            .WithOpenApi(operation => new(operation)
            {
                Summary = "Consultar quórum",
                Description = "Calcula el quórum actual de la asamblea en tiempo real basado en la asistencia/votos."
            });

        assembliesGroup.MapPost("/{id}/close", async (Guid id, IMediator mediator) =>
        {
            var result = await mediator.Send(new CloseAssemblyCommand(id));
            return result ? Results.Ok() : Results.NotFound();
        })
        .RequireAuthorization("AdminOnly") // Changed from policy.RequireRole(UserRoles.Admin) to "AdminOnly"
        .WithOpenApi(operation => new(operation)
        {
            Summary = "Cerrar asamblea",
            Description = "Finaliza formalmente la asamblea y bloquea nuevas votaciones."
        });

        // Assembly Report Endpoint
        assembliesGroup.MapGet("/{id}/report", async (Guid id, IMediator mediator) =>
        {
            var pdf = await mediator.Send(new GetAssemblyPdfQuery(id));
            return Results.File(pdf, "application/pdf", $"Acta_Asamblea_{id}.pdf");
        })
        .RequireAuthorization(policy => policy.RequireRole(UserRoles.Admin))
        .WithOpenApi(operation => new(operation)
        {
            Summary = "Generar acta de asamblea",
            Description = "Genera y descarga un archivo PDF con el acta oficial y resultados de la asamblea."
        });
    }
}
