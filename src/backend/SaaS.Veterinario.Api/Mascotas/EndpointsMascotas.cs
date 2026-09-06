using SaaS.Veterinario.Api.Autorizacion;
using SaaS.Veterinario.Api.Consultas;
using SaaS.Veterinario.Application.Abstracciones;
using SaaS.Veterinario.Application.Consultas;
using SaaS.Veterinario.Application.Mascotas;

namespace SaaS.Veterinario.Api.Mascotas;

public static class EndpointsMascotas
{
    public static void MapearEndpointsMascotas(this IEndpointRouteBuilder app)
    {
        var grupo = app.MapGroup("/api/v1/mascotas").RequireAuthorization();

        grupo.MapGet("/", async (
                IContextoVeterinaria contexto,
                IRepositorioMascotasVeterinarias repositorioMascotasVeterinarias,
                CancellationToken cancellationToken) =>
            {
                var fichas = await repositorioMascotasVeterinarias.ListarPorVeterinariaAsync(contexto.VeterinariaId!.Value, cancellationToken);
                return Results.Ok(fichas.Select(MascotaFichaResponse.DeInfo));
            })
            .RequierePermiso("mascotas.ver");

        // La busqueda es el primer paso del flujo de vincular una mascota existente (R06):
        // solo admite CodigoPublico o NumeroMicrochip, nunca nombre/raza/dueño/correo.
        grupo.MapGet("/buscar", async (
                string? codigoPublico,
                string? microchip,
                BuscarMascota casoDeUso,
                CancellationToken cancellationToken) =>
            {
                if (string.IsNullOrWhiteSpace(codigoPublico) == string.IsNullOrWhiteSpace(microchip))
                {
                    return Results.BadRequest(new { error = "Debe indicar exactamente uno de: codigoPublico o microchip." });
                }

                var encontrada = string.IsNullOrWhiteSpace(codigoPublico)
                    ? await casoDeUso.PorMicrochipAsync(microchip!, cancellationToken)
                    : await casoDeUso.PorCodigoPublicoAsync(codigoPublico, cancellationToken);

                return encontrada is null ? Results.NotFound() : Results.Ok(MascotaEncontradaResponse.DeAplicacion(encontrada));
            })
            .RequierePermiso("mascotas.crear");

        grupo.MapGet("/{id:guid}", async (
                Guid id,
                IContextoVeterinaria contexto,
                IRepositorioMascotasVeterinarias repositorioMascotasVeterinarias,
                CancellationToken cancellationToken) =>
            {
                var ficha = await repositorioMascotasVeterinarias.ObtenerFichaPorIdAsync(id, contexto.VeterinariaId!.Value, cancellationToken);
                return ficha is null ? Results.NotFound() : Results.Ok(MascotaFichaResponse.DeInfo(ficha));
            })
            .RequierePermiso("mascotas.ver");

        grupo.MapPost("/", async (
                RegistrarMascotaRequest solicitud,
                RegistrarMascota casoDeUso,
                CancellationToken cancellationToken) =>
            {
                var resultado = await casoDeUso.EjecutarAsync(
                    new RegistrarMascotaComando(
                        solicitud.Nombre,
                        solicitud.EspecieId,
                        solicitud.RazaId,
                        solicitud.Sexo,
                        solicitud.FechaNacimiento,
                        solicitud.FechaNacimientoAproximada,
                        solicitud.Color,
                        solicitud.NumeroMicrochip,
                        solicitud.Esterilizado,
                        solicitud.NumeroHistoria),
                    cancellationToken);

                return Results.Ok(new RegistrarMascotaResponse(resultado.MascotaVeterinariaId, resultado.MascotaId, resultado.CodigoPublico));
            })
            .RequierePermiso("mascotas.crear");

        grupo.MapPost("/vincular", async (
                VincularMascotaRequest solicitud,
                VincularMascotaExistente casoDeUso,
                CancellationToken cancellationToken) =>
            {
                var resultado = await casoDeUso.EjecutarAsync(
                    new VincularMascotaExistenteComando(solicitud.CodigoPublico, solicitud.NumeroHistoria),
                    cancellationToken);

                return Results.Ok(new VincularMascotaResponse(resultado.MascotaVeterinariaId, resultado.CodigoPublico, resultado.Nombre));
            })
            .RequierePermiso("mascotas.crear");

        grupo.MapGet("/{id:guid}/responsables", async (
                Guid id,
                IContextoVeterinaria contexto,
                IRepositorioResponsables repositorioResponsables,
                CancellationToken cancellationToken) =>
            {
                var responsables = await repositorioResponsables.ListarPorMascotaVeterinariaAsync(id, contexto.VeterinariaId!.Value, cancellationToken);
                return Results.Ok(responsables.Select(ResponsableMascotaResponse.DeDominio));
            })
            .RequierePermiso("responsables.ver");

        grupo.MapPost("/{id:guid}/responsables", async (
                Guid id,
                RegistrarResponsableRequest solicitud,
                RegistrarResponsableMascota casoDeUso,
                CancellationToken cancellationToken) =>
            {
                var resultado = await casoDeUso.EjecutarAsync(
                    new RegistrarResponsableComando(
                        id,
                        solicitud.ClienteVeterinariaId,
                        solicitud.Nombre,
                        solicitud.Correo,
                        solicitud.Telefono,
                        solicitud.TipoRelacion,
                        solicitud.EsPrincipal,
                        solicitud.PuedeVerHistorial,
                        solicitud.PuedeGestionarTratamientos),
                    cancellationToken);

                return Results.Ok(new RegistrarResponsableResponse(resultado.ResponsableId));
            })
            .RequierePermiso("responsables.crear");

        grupo.MapGet("/{id:guid}/historial", async (Guid id, ObtenerHistorialMascota casoDeUso, CancellationToken cancellationToken) =>
            {
                var historial = await casoDeUso.EjecutarAsync(id, cancellationToken);
                return Results.Ok(HistorialMascotaResponse.DeAplicacion(historial));
            })
            .RequierePermiso("historial.ver");
    }
}
