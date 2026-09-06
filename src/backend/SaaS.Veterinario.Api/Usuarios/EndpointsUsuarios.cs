using SaaS.Veterinario.Api.Autenticacion;
using SaaS.Veterinario.Application.Abstracciones;

namespace SaaS.Veterinario.Api.Usuarios;

public static class EndpointsUsuarios
{
    public static void MapearEndpointsUsuarios(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/v1/usuarios/me", async (
                HttpContext httpContext,
                IRepositorioUsuarios repositorioUsuarios,
                CancellationToken cancellationToken) =>
            {
                var usuarioId = httpContext.ObtenerUsuarioIdAutenticado();
                var usuario = await repositorioUsuarios.ObtenerPorIdAsync(usuarioId, cancellationToken);

                return usuario is null
                    ? Results.NotFound()
                    : Results.Ok(UsuarioActualResponse.DeDominio(usuario));
            })
            .RequireAuthorization();
    }
}
