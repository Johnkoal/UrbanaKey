using MediatR;
using UrbanaKey.Core.Domain;
using UrbanaKey.Core.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace UrbanaKey.Core.Features.PQRS;

using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using UrbanaKey.Core.Interfaces;

public record CreatePqrCommand(CreatePqrRequest Request, Guid UserId) : IRequest<Guid>;

public class CreatePqrHandler(
    IApplicationDbContext db, 
    ITenantProvider tenantProvider, 
    IEmailQueue emailQueue,
    ITemplateService templateService,
    UserManager<User> userManager) 
    : IRequestHandler<CreatePqrCommand, Guid>
{
    public async Task<Guid> Handle(CreatePqrCommand command, CancellationToken ct)
    {
        var pqr = new Domain.PQRS
        {
            Id = Guid.NewGuid(),
            TenantId = tenantProvider.GetTenantId(),
            CreatedBy = command.UserId,
            UnitId = command.Request.UnitId,
            Title = command.Request.Title,
            Description = command.Request.Description,
            IsPublic = command.Request.IsPublic,
            AttachmentUrl = command.Request.AttachmentUrl,
            Status = "Open",
            CreatedAt = DateTime.UtcNow
        };

        db.PQRS.Add(pqr);
        await db.SaveChangesAsync(ct);

        // 2. Preparar la notificación
        var user = await userManager.FindByIdAsync(command.UserId.ToString());
        if (user != null && !string.IsNullOrEmpty(user.Email))
        {
            var placeholders = new Dictionary<string, string>
            {
                { "UserName", user.FullName ?? "Residente" },
                { "Title", pqr.Title },
                { "Date", pqr.CreatedAt.ToString("f") }
            };

            var body = templateService.GetTemplate("PqrConfirmation", placeholders);

            // 3. Encolar para envío en segundo plano
            await emailQueue.EnqueueEmailAsync(new EmailMessage(
                user.Email,
                $"Radicado Exitoso: {pqr.Title}",
                body
            ));
            
            // También notificar al admin (manteniendo la lógica anterior pero quizás simplificada o combinada)
            // Por ahora mantenemos la notificación al usuario como prioridad del requerimiento.
             await emailQueue.EnqueueEmailAsync(new EmailMessage(
                "admin@urbanakey.com", 
                $"New PQR: {pqr.Title}", 
                $"A new PQR has been created by user {command.UserId}."
            ));
        }

        return pqr.Id;
    }
}
