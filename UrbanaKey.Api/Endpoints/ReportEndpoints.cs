using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using UrbanaKey.Core.Features.Admin;
using UrbanaKey.Core.Features.Finance;
using UrbanaKey.Core.Domain;
using UrbanaKey.Core.Common;
using System;

namespace UrbanaKey.Api.Endpoints;

public static class ReportEndpoints
{
    public static void MapReportEndpoints(this IEndpointRouteBuilder app)
    {
        // Admin Reports Group
        var adminReportsGroup = app.MapGroup("/api/admin/reports")
            .RequireAuthorization("AdminOnly")
            .WithTags("Admin Reports");

        adminReportsGroup.MapGet("/audit-logs", async (IMediator mediator) =>
                Results.Ok(await mediator.Send(new GetAuditLogsQuery())))
            .WithOpenApi(operation => new(operation)
            {
                Summary = "Registros de auditoría",
                Description = "Obtiene los registros de auditoría del sistema."
            });



        // Finance Group
        app.MapGroup("/api/admin/finance")
            .RequireAuthorization(policy => policy.RequireRole(UserRoles.Admin))
            .WithTags("Finance")
            .MapPost("/generate-invoices", async (decimal standardFee, IMediator mediator) =>
            {
                var count = await mediator.Send(new GenerateMonthlyInvoicesCommand(standardFee));
                return Results.Ok(new { InvoicesGenerated = count });
            })
            .WithOpenApi(operation => new(operation)
            {
                Summary = "Generar facturas mensuales",
                Description = "Genera las facturas mensuales para todas las unidades habitacionales."
            });
    }
}
