using SaaS.Veterinario.Application.Autenticacion;

namespace SaaS.Veterinario.Api.Autenticacion;

public static class EndpointsAutenticacion
{
    public const string PoliticaSensible = "autenticacion-sensible";

    public static void MapearEndpointsAutenticacion(this IEndpointRouteBuilder app)
    {
        var grupo = app.MapGroup("/api/v1/autenticacion");

        grupo.MapPost("/registrar", async (
            RegistrarUsuarioRequest solicitud,
            RegistrarUsuario casoDeUso,
            CancellationToken cancellationToken) =>
        {
            var resultado = await casoDeUso.EjecutarAsync(
                new RegistrarUsuarioComando(solicitud.Nombre, solicitud.Correo, solicitud.Telefono, solicitud.Password),
                cancellationToken);

            return Results.Ok(new RegistrarUsuarioResponse(resultado.UsuarioId, resultado.Correo));
        }).RequireRateLimiting(PoliticaSensible);

        grupo.MapPost("/iniciar-sesion", async (
            IniciarSesionRequest solicitud,
            IniciarSesion casoDeUso,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            var tokens = await casoDeUso.EjecutarAsync(
                new IniciarSesionComando(
                    solicitud.Correo,
                    solicitud.Password,
                    solicitud.Dispositivo,
                    ObtenerDireccionIp(httpContext)),
                cancellationToken);

            return Results.Ok(TokenResponse.DeDominio(tokens));
        }).RequireRateLimiting(PoliticaSensible);

        grupo.MapPost("/renovar", async (
            RenovarTokenRequest solicitud,
            RenovarToken casoDeUso,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            var tokens = await casoDeUso.EjecutarAsync(
                new RenovarTokenComando(solicitud.RefreshToken, solicitud.Dispositivo, ObtenerDireccionIp(httpContext)),
                cancellationToken);

            return Results.Ok(TokenResponse.DeDominio(tokens));
        });

        grupo.MapPost("/cerrar-sesion", async (
            CerrarSesionRequest solicitud,
            CerrarSesion casoDeUso,
            CancellationToken cancellationToken) =>
        {
            await casoDeUso.EjecutarAsync(new CerrarSesionComando(solicitud.RefreshToken), cancellationToken);
            return Results.NoContent();
        });

        grupo.MapPost("/cerrar-todas-las-sesiones", async (
            CerrarTodasLasSesiones casoDeUso,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            var usuarioId = httpContext.ObtenerUsuarioIdAutenticado();
            await casoDeUso.EjecutarAsync(new CerrarTodasLasSesionesComando(usuarioId), cancellationToken);
            return Results.NoContent();
        }).RequireAuthorization();

        grupo.MapPost("/confirmar-correo", async (
            ConfirmarCorreoRequest solicitud,
            ConfirmarCorreo casoDeUso,
            CancellationToken cancellationToken) =>
        {
            await casoDeUso.EjecutarAsync(new ConfirmarCorreoComando(solicitud.UsuarioId, solicitud.Token), cancellationToken);
            return Results.NoContent();
        });

        grupo.MapPost("/solicitar-recuperacion", async (
            SolicitarRecuperacionRequest solicitud,
            SolicitarRecuperacionPassword casoDeUso,
            CancellationToken cancellationToken) =>
        {
            await casoDeUso.EjecutarAsync(new SolicitarRecuperacionPasswordComando(solicitud.Correo), cancellationToken);

            // Respuesta identica exista o no la cuenta -- nunca revela existencia del correo.
            return Results.Ok(new MensajeResponse("Si existe una cuenta asociada, recibirás instrucciones."));
        }).RequireRateLimiting(PoliticaSensible);

        grupo.MapPost("/restablecer-password", async (
            RestablecerPasswordRequest solicitud,
            RestablecerPassword casoDeUso,
            CancellationToken cancellationToken) =>
        {
            await casoDeUso.EjecutarAsync(
                new RestablecerPasswordComando(solicitud.Correo, solicitud.Token, solicitud.NuevoPassword),
                cancellationToken);

            return Results.NoContent();
        });
    }

    private static string? ObtenerDireccionIp(HttpContext httpContext)
        => httpContext.Connection.RemoteIpAddress?.ToString();
}
