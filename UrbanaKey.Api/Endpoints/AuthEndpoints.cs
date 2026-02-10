using Microsoft.AspNetCore.Identity;
using UrbanaKey.Core.Domain;
using UrbanaKey.Core.Interfaces;
using UrbanaKey.Api.Validators;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace UrbanaKey.Api.Endpoints;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var authGroup = app.MapGroup("/api/auth").WithTags("Auth");

        authGroup.MapIdentityApi<User>();

        authGroup.MapPost("/onboarding", async (UserManager<User> userManager, ITenantProvider tenantProvider, OnboardingRequest request) => 
        {
            var user = new User 
            { 
                UserName = request.Email, 
                Email = request.Email, 
                FullName = request.FullName,
                TenantId = tenantProvider.GetTenantId(),
                IsActive = false 
            };
            
            var result = await userManager.CreateAsync(user, request.Password);
            
            if (result.Succeeded)
                return Results.Ok(new { Message = "Solicitud enviada para aprobación" });
                
            return Results.BadRequest(result.Errors);
        })
        .WithOpenApi(operation => new(operation)
        {
            Summary = "Solicitud de alta (Onboarding)",
            Description = "Permite a un nuevo usuario solicitar acceso al sistema. La cuenta quedará inactiva hasta ser aprobada por un administrador."
        });
    }
}
