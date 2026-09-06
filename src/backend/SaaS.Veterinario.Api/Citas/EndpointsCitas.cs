using SaaS.Veterinario.Api.Autorizacion;
using SaaS.Veterinario.Application.Citas;
using SaaS.Veterinario.Domain.Citas;

namespace SaaS.Veterinario.Api.Citas;

public static class EndpointsCitas
{
    public static void MapearEndpointsCitas(this IEndpointRouteBuilder app)
    {
        var grupo = app.MapGroup("/api/v1/citas").RequireAuthorization();

        grupo.MapGet("/", async (
                DateTimeOffset? fechaDesde,
                DateTimeOffset? fechaHasta,
                Guid? veterinarioId,
                string? estado,
                ListarAgenda casoDeUso,
                CancellationToken cancellationToken) =>
            {
                EstadoCita? estadoFiltro = null;
                if (!string.IsNullOrWhiteSpace(estado))
                {
                    if (!Enum.TryParse<EstadoCita>(estado, ignoreCase: true, out var estadoParseado))
                    {
                        return Results.BadRequest(new { error = "El estado indicado no es válido." });
                    }

                    estadoFiltro = estadoParseado;
                }

                var citas = await casoDeUso.EjecutarAsync(new ListarAgendaConsulta(fechaDesde, fechaHasta, veterinarioId, estadoFiltro), cancellationToken);
                return Results.Ok(citas.Select(CitaResponse.DeDominio));
            })
            .RequierePermiso("citas.ver");

        grupo.MapGet("/{id:guid}", async (Guid id, ObtenerCita casoDeUso, CancellationToken cancellationToken) =>
            {
                var cita = await casoDeUso.EjecutarAsync(id, cancellationToken);
                return cita is null ? Results.NotFound() : Results.Ok(CitaResponse.DeDominio(cita));
            })
            .RequierePermiso("citas.ver");

        grupo.MapPost("/", async (CrearCitaRequest solicitud, CrearCita casoDeUso, CancellationToken cancellationToken) =>
            {
                var resultado = await casoDeUso.EjecutarAsync(
                    new CrearCitaComando(
                        solicitud.MascotaVeterinariaId,
                        solicitud.ClienteVeterinariaId,
                        solicitud.VeterinarioUsuarioId,
                        solicitud.FechaHoraInicio,
                        solicitud.FechaHoraFin,
                        solicitud.Motivo,
                        solicitud.Observaciones),
                    cancellationToken);

                return Results.Ok(new CrearCitaResponse(resultado.CitaId, resultado.Estado.ToString()));
            })
            .RequierePermiso("citas.crear");

        grupo.MapPut("/{id:guid}", async (Guid id, ActualizarCitaRequest solicitud, ActualizarCita casoDeUso, CancellationToken cancellationToken) =>
            {
                await casoDeUso.EjecutarAsync(
                    new ActualizarCitaComando(
                        id, solicitud.FechaHoraInicio, solicitud.FechaHoraFin, solicitud.Motivo, solicitud.Observaciones, solicitud.VeterinarioUsuarioId),
                    cancellationToken);

                return Results.NoContent();
            })
            .RequierePermiso("citas.editar");

        grupo.MapPost("/{id:guid}/confirmar", async (Guid id, GestionarEstadoCita casoDeUso, CancellationToken cancellationToken) =>
            {
                await casoDeUso.ConfirmarAsync(id, cancellationToken);
                return Results.NoContent();
            })
            .RequierePermiso("citas.cambiar_estado");

        grupo.MapPost("/{id:guid}/en-espera", async (Guid id, GestionarEstadoCita casoDeUso, CancellationToken cancellationToken) =>
            {
                await casoDeUso.MarcarEnEsperaAsync(id, cancellationToken);
                return Results.NoContent();
            })
            .RequierePermiso("citas.cambiar_estado");

        grupo.MapPost("/{id:guid}/atender", async (Guid id, GestionarEstadoCita casoDeUso, CancellationToken cancellationToken) =>
            {
                await casoDeUso.MarcarAtendidaAsync(id, cancellationToken);
                return Results.NoContent();
            })
            .RequierePermiso("citas.cambiar_estado");

        grupo.MapPost("/{id:guid}/cancelar", async (Guid id, GestionarEstadoCita casoDeUso, CancellationToken cancellationToken) =>
            {
                await casoDeUso.CancelarAsync(id, cancellationToken);
                return Results.NoContent();
            })
            .RequierePermiso("citas.cancelar");

        grupo.MapPost("/{id:guid}/no-asistio", async (Guid id, GestionarEstadoCita casoDeUso, CancellationToken cancellationToken) =>
            {
                await casoDeUso.MarcarNoAsistioAsync(id, cancellationToken);
                return Results.NoContent();
            })
            .RequierePermiso("citas.cambiar_estado");
    }
}
