using SaaS.Veterinario.Api.Autenticacion;
using SaaS.Veterinario.Application.ContextoVeterinario;
using SaaS.Veterinario.Infrastructure.Identidad;

namespace SaaS.Veterinario.Api.ContextoVeterinario;

/// <summary>
/// Se ejecuta despues de UseAuthentication() y antes de UseAuthorization(): si la request
/// esta autenticada, resuelve el contexto veterinario UNA sola vez (ver ResolverContextoVeterinaria)
/// y lo carga en la instancia Scoped de ContextoVeterinaria, que PermisoAuthorizationHandler y
/// cualquier caso de uso van a leer despues sin volver a tocar la base de datos.
/// </summary>
public sealed class ContextoVeterinarioMiddleware(RequestDelegate siguiente)
{
    public async Task InvokeAsync(
        HttpContext httpContext,
        ResolverContextoVeterinaria resolver,
        Infrastructure.Identidad.ContextoVeterinaria contexto,
        ILogger<ContextoVeterinarioMiddleware> logger)
    {
        if (httpContext.User.Identity?.IsAuthenticated != true)
        {
            await siguiente(httpContext);
            return;
        }

        var usuarioId = httpContext.ObtenerUsuarioIdAutenticado();
        var resultado = await resolver.EjecutarAsync(usuarioId, httpContext.RequestAborted);
        contexto.Cargar(usuarioId, resultado);

        using (logger.BeginScope(new Dictionary<string, object?>
        {
            ["UsuarioId"] = usuarioId,
            ["VeterinariaId"] = resultado.VeterinariaId,
            ["MembresiaId"] = resultado.MembresiaId,
        }))
        {
            await siguiente(httpContext);
        }
    }
}
