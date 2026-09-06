using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.AspNetCore.Mvc;
using SaaS.Veterinario.Api.Errores;
using SaaS.Veterinario.Application.Errores;

namespace SaaS.Veterinario.Api.Autorizacion;

/// <summary>
/// Traduce el AuthorizationFailureReason que dejo PermisoAuthorizationHandler a un
/// ProblemDetails 403 con el codigo funcional correspondiente (distinto segun sea "sin
/// contexto veterinario", "veterinaria no disponible" o "falta el permiso"). Para cualquier
/// otro caso (por ejemplo 401 por no estar autenticado) delega en el manejador por defecto.
/// </summary>
public sealed class ManejadorResultadoAutorizacion : IAuthorizationMiddlewareResultHandler
{
    private readonly AuthorizationMiddlewareResultHandler _predeterminado = new();

    public async Task HandleAsync(
        RequestDelegate siguiente,
        HttpContext httpContext,
        AuthorizationPolicy policy,
        PolicyAuthorizationResult authorizeResult)
    {
        var codigo = authorizeResult.Forbidden
            ? authorizeResult.AuthorizationFailure?.FailureReasons
                .Select(r => r.Message)
                .FirstOrDefault(m => !string.IsNullOrWhiteSpace(m))
            : null;

        if (codigo is null)
        {
            await _predeterminado.HandleAsync(siguiente, httpContext, policy, authorizeResult);
            return;
        }

        var correlationId = httpContext.ObtenerCorrelationId();
        var mensaje = MensajePara(codigo);

        httpContext.Response.StatusCode = StatusCodes.Status403Forbidden;

        var problema = new ProblemDetails
        {
            Status = StatusCodes.Status403Forbidden,
            Title = codigo,
            Detail = mensaje,
        };
        problema.Extensions["codigo"] = codigo;
        problema.Extensions["correlationId"] = correlationId;

        await httpContext.Response.WriteAsJsonAsync(problema);
    }

    private static string MensajePara(string codigo) => codigo switch
    {
        CodigosError.ContextoVeterinarioRequerido => "Esta operación requiere pertenecer al personal de una veterinaria.",
        CodigosError.VeterinariaNoDisponible => "La veterinaria asociada no está disponible actualmente.",
        CodigosError.AutorizacionPermisoRequerido => "No tienes el permiso necesario para esta operación.",
        _ => "No tienes autorización para esta operación.",
    };
}
