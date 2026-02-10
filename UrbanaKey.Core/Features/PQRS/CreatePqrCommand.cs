using MediatR;
using UrbanaKey.Core.Domain;
using UrbanaKey.Core.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace UrbanaKey.Core.Features.PQRS;

public record CreatePqrCommand(CreatePqrRequest Request, Guid UserId) : IRequest<Guid>;

public class CreatePqrHandler(IApplicationDbContext db, ITenantProvider tenantProvider, IEmailService emailService) 
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

        // Notify Admin (or designated contact)
        await emailService.SendEmailAsync(new EmailMessage(
            "admin@urbanakey.com", 
            $"New PQR: {pqr.Title}", 
            $"A new PQR has been created by user {command.UserId}."
        ));

        return pqr.Id;
    }
}
