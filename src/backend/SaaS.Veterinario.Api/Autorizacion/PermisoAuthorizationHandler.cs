using Microsoft.AspNetCore.Authorization;
using SaaS.Veterinario.Application.Abstracciones;
using SaaS.Veterinario.Application.Errores;

namespace SaaS.Veterinario.Api.Autorizacion;

/// <summary>
/// Consume IContextoVeterinaria ya resuelto (por ContextoVeterinarioMiddleware, una sola vez
/// por request) -- nunca vuelve a consultar la base de datos. Distingue el motivo exacto del
/// rechazo (sin membresia / veterinaria no disponible / falta el permiso) via
/// AuthorizationFailureReason, que ManejadorResultadoAutorizacion traduce al codigo
/// ProblemDetails correspondiente.
/// </summary>
public sealed class PermisoAuthorizationHandler(IContextoVeterinaria contexto, ILogger<PermisoAuthorizationHandler> logger)
    : AuthorizationHandler<PermisoRequirement>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, PermisoRequirement requirement)
    {
        if (!contexto.TieneMembresiaActiva)
        {
            logger.LogWarning(
                "Acceso denegado: usuario {UsuarioId} sin membresía veterinaria activa (permiso requerido: {Permiso}).",
                contexto.UsuarioId, requirement.Permiso);
            context.Fail(new AuthorizationFailureReason(this, CodigosError.ContextoVeterinarioRequerido));
            return Task.CompletedTask;
        }

        if (!contexto.TieneContextoOperativo)
        {
            logger.LogWarning(
                "Acceso denegado: veterinaria {VeterinariaId} no disponible para el usuario {UsuarioId} (permiso requerido: {Permiso}).",
                contexto.VeterinariaId, contexto.UsuarioId, requirement.Permiso);
            context.Fail(new AuthorizationFailureReason(this, CodigosError.VeterinariaNoDisponible));
            return Task.CompletedTask;
        }

        if (contexto.TienePermiso(requirement.Permiso))
        {
            context.Succeed(requirement);
        }
        else
        {
            logger.LogWarning(
                "Acceso denegado: usuario {UsuarioId} sin el permiso {Permiso} en veterinaria {VeterinariaId}.",
                contexto.UsuarioId, requirement.Permiso, contexto.VeterinariaId);
            context.Fail(new AuthorizationFailureReason(this, CodigosError.AutorizacionPermisoRequerido));
        }

        return Task.CompletedTask;
    }
}
