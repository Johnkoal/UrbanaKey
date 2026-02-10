using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;
using UrbanaKey.Core.Domain;
using UrbanaKey.Core.Interfaces;

namespace UrbanaKey.Core.Features.PQRS;

public record AddPqrCommentRequest(Guid PqrId, string Message);
public record AddPqrCommentCommand(AddPqrCommentRequest Request, Guid UserId) : IRequest<Guid>;

public class AddPqrCommentHandler(IApplicationDbContext db, ITenantProvider tenantProvider) 
    : IRequestHandler<AddPqrCommentCommand, Guid>
{
    public async Task<Guid> Handle(AddPqrCommentCommand command, CancellationToken ct)
    {
        var comment = new PqrComment
        {
            Id = Guid.NewGuid(),
            TenantId = tenantProvider.GetTenantId(),
            PqrId = command.Request.PqrId,
            UserId = command.UserId,
            Message = command.Request.Message,
            CreatedAt = DateTime.UtcNow
        };

        db.PqrComments.Add(comment);
        await db.SaveChangesAsync(ct);
        return comment.Id;
    }
}
