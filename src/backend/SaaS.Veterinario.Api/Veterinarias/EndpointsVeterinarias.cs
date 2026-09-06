using SaaS.Veterinario.Api.Autorizacion;
using SaaS.Veterinario.Application.Abstracciones;
using SaaS.Veterinario.Application.Veterinarias;

namespace SaaS.Veterinario.Api.Veterinarias;

public static class EndpointsVeterinarias
{
    public static void MapearEndpointsVeterinarias(this IEndpointRouteBuilder app)
    {
        var grupo = app.MapGroup("/api/v1/veterinarias").RequireAuthorization();

        // La veterinaria SIEMPRE sale de IContextoVeterinaria; nunca se acepta un
        // {veterinariaId} en la ruta (evita que un usuario consulte otra veterinaria
        // manipulando la URL).
        grupo.MapGet("/actual", async (
                IContextoVeterinaria contexto,
                IRepositorioVeterinarias repositorioVeterinarias,
                CancellationToken cancellationToken) =>
            {
                var veterinaria = await repositorioVeterinarias.ObtenerPorIdAsync(contexto.VeterinariaId!.Value, cancellationToken);
                return veterinaria is null
                    ? Results.NotFound()
                    : Results.Ok(VeterinariaActualResponse.DeDominio(veterinaria));
            })
            .RequierePermiso("veterinaria.ver");

        grupo.MapPut("/actual", async (
                ActualizarVeterinariaRequest solicitud,
                ActualizarVeterinariaActual casoDeUso,
                CancellationToken cancellationToken) =>
            {
                await casoDeUso.EjecutarAsync(
                    new ActualizarVeterinariaActualComando(
                        solicitud.NombreComercial,
                        solicitud.RazonSocial,
                        solicitud.Ruc,
                        solicitud.Telefono,
                        solicitud.Direccion,
                        solicitud.Ciudad,
                        solicitud.Pais,
                        solicitud.ZonaHoraria),
                    cancellationToken);

                return Results.NoContent();
            })
            .RequierePermiso("veterinaria.editar");
    }
}
