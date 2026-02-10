using MediatR;
using Microsoft.AspNetCore.Identity;
using UrbanaKey.Core.Domain;
using UrbanaKey.Core.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace UrbanaKey.Core.Features.Admin;

public record ApproveUserCommand(Guid UserId) : IRequest<bool>;

public class ApproveUserHandler(
    UserManager<User> userManager, 
    ITenantProvider tenantProvider,
    IEmailQueue emailQueue) : IRequestHandler<ApproveUserCommand, bool>
{
    public async Task<bool> Handle(ApproveUserCommand request, CancellationToken ct)
    {
        var user = await userManager.FindByIdAsync(request.UserId.ToString());
        var currentTenantId = tenantProvider.GetTenantId();

        // Security Check: Admin can only approve users from their own tenant
        if (user == null || user.TenantId != currentTenantId)
            return false;

        user.IsActive = true;
        var result = await userManager.UpdateAsync(user);

        if (result.Succeeded && !string.IsNullOrEmpty(user.Email))
        {
            await emailQueue.EnqueueEmailAsync(new EmailMessage(
                user.Email,
                "¡Cuenta Activada en UrbanaKey!",
                $"Hola {user.FullName}, tu cuenta ha sido aprobada por la administración."
            ));
        }
        
        return result.Succeeded;
    }
}
