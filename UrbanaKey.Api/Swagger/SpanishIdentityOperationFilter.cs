using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace UrbanaKey.Api.Swagger;

public class SpanishIdentityOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var path = context.ApiDescription.RelativePath;

        if (path == null) return;

        // Map Identity endpoints to Spanish documentation
        if (path.Contains("api/auth/register", StringComparison.OrdinalIgnoreCase))
        {
            operation.Summary = "Registrar un nuevo usuario";
            operation.Description = "Permite registrar un nuevo usuario con email y contraseña.";
        }
        else if (path.Contains("api/auth/login", StringComparison.OrdinalIgnoreCase))
        {
            operation.Summary = "Iniciar sesión";
            operation.Description = "Autentica al usuario y retorna una cookie o token de sesión.";
        }
        else if (path.Contains("api/auth/refresh", StringComparison.OrdinalIgnoreCase))
        {
            operation.Summary = "Refrescar token";
            operation.Description = "Renueva el token de autenticación utilizando un refresh token válido.";
        }
        else if (path.Contains("api/auth/confirmEmail", StringComparison.OrdinalIgnoreCase))
        {
            operation.Summary = "Confirmar correo electrónico";
            operation.Description = "Valida el correo electrónico del usuario mediante un código de confirmación.";
        }
        else if (path.Contains("api/auth/resendConfirmationEmail", StringComparison.OrdinalIgnoreCase))
        {
            operation.Summary = "Reenviar confirmación de correo";
            operation.Description = "Envía nuevamente el correo de confirmación a la dirección registrada.";
        }
        else if (path.Contains("api/auth/forgotPassword", StringComparison.OrdinalIgnoreCase))
        {
            operation.Summary = "Olvidé mi contraseña";
            operation.Description = "Envía un enlace de recuperación de contraseña al correo del usuario.";
        }
        else if (path.Contains("api/auth/resetPassword", StringComparison.OrdinalIgnoreCase))
        {
            operation.Summary = "Restablecer contraseña";
            operation.Description = "Cambia la contraseña del usuario utilizando un token de recuperación.";
        }
        else if (path.Contains("api/auth/manage/2fa", StringComparison.OrdinalIgnoreCase))
        {
            operation.Summary = "Gestionar 2FA";
            operation.Description = "Configura o desactiva la autenticación de dos factores.";
        }
        else if (path.Contains("api/auth/manage/info", StringComparison.OrdinalIgnoreCase))
        {
            operation.Summary = "Gestionar información de perfil";
            operation.Description = "Consulta o actualiza la información básica del perfil del usuario (Email, 2FA).";
        }
    }
}
