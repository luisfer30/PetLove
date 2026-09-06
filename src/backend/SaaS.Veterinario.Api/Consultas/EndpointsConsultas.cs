using SaaS.Veterinario.Api.Autorizacion;
using SaaS.Veterinario.Application.Consultas;

namespace SaaS.Veterinario.Api.Consultas;

public static class EndpointsConsultas
{
    public static void MapearEndpointsConsultas(this IEndpointRouteBuilder app)
    {
        var grupo = app.MapGroup("/api/v1/consultas").RequireAuthorization();

        grupo.MapGet("/{id:guid}", async (Guid id, ObtenerConsulta casoDeUso, CancellationToken cancellationToken) =>
            {
                var consulta = await casoDeUso.EjecutarAsync(id, cancellationToken);
                return consulta is null ? Results.NotFound() : Results.Ok(ConsultaResponse.DeDominio(consulta));
            })
            .RequierePermiso("consultas.ver");

        grupo.MapPost("/", async (CrearConsultaRequest solicitud, CrearConsulta casoDeUso, CancellationToken cancellationToken) =>
            {
                var resultado = await casoDeUso.EjecutarAsync(
                    new CrearConsultaComando(
                        solicitud.MascotaVeterinariaId,
                        solicitud.CitaId,
                        solicitud.VeterinarioUsuarioId,
                        solicitud.FechaHora,
                        solicitud.MotivoConsulta,
                        solicitud.Peso,
                        solicitud.Temperatura,
                        solicitud.FrecuenciaCardiaca,
                        solicitud.ObservacionesClinicas,
                        solicitud.IndicacionesPropietario,
                        solicitud.ProximaFechaControl,
                        solicitud.MotivoProximoControl),
                    cancellationToken);

                return Results.Ok(new CrearConsultaResponse(resultado.ConsultaId, resultado.Estado.ToString()));
            })
            .RequierePermiso("consultas.crear");

        grupo.MapPut("/{id:guid}", async (Guid id, ActualizarConsultaRequest solicitud, ActualizarConsultaBorrador casoDeUso, CancellationToken cancellationToken) =>
            {
                await casoDeUso.EjecutarAsync(
                    new ActualizarConsultaComando(
                        id,
                        solicitud.VeterinarioUsuarioId,
                        solicitud.FechaHora,
                        solicitud.MotivoConsulta,
                        solicitud.Peso,
                        solicitud.Temperatura,
                        solicitud.FrecuenciaCardiaca,
                        solicitud.ObservacionesClinicas,
                        solicitud.IndicacionesPropietario,
                        solicitud.ProximaFechaControl,
                        solicitud.MotivoProximoControl),
                    cancellationToken);

                return Results.NoContent();
            })
            .RequierePermiso("consultas.editar_borrador");

        grupo.MapPost("/{id:guid}/finalizar", async (Guid id, FinalizarConsulta casoDeUso, CancellationToken cancellationToken) =>
            {
                await casoDeUso.EjecutarAsync(id, cancellationToken);
                return Results.NoContent();
            })
            .RequierePermiso("consultas.finalizar");

        grupo.MapPost("/{id:guid}/anular", async (Guid id, AnularConsultaRequest solicitud, AnularConsulta casoDeUso, CancellationToken cancellationToken) =>
            {
                await casoDeUso.EjecutarAsync(new AnularConsultaComando(id, solicitud.Motivo), cancellationToken);
                return Results.NoContent();
            })
            .RequierePermiso("consultas.anular");

        grupo.MapGet("/{id:guid}/diagnosticos", async (Guid id, ListarDiagnosticos casoDeUso, CancellationToken cancellationToken) =>
            {
                var diagnosticos = await casoDeUso.EjecutarAsync(id, cancellationToken);
                return Results.Ok(diagnosticos.Select(DiagnosticoResponse.DeDominio));
            })
            .RequierePermiso("diagnosticos.ver");

        grupo.MapPost("/{id:guid}/diagnosticos", async (Guid id, AgregarDiagnosticoRequest solicitud, AgregarDiagnostico casoDeUso, CancellationToken cancellationToken) =>
            {
                var resultado = await casoDeUso.EjecutarAsync(
                    new AgregarDiagnosticoComando(id, solicitud.Descripcion, solicitud.Tipo, solicitud.EsPrincipal), cancellationToken);

                return Results.Ok(new AgregarDiagnosticoResponse(resultado.DiagnosticoId));
            })
            .RequierePermiso("diagnosticos.crear");

        grupo.MapPut("/{id:guid}/diagnosticos/{diagnosticoId:guid}", async (
                Guid id, Guid diagnosticoId, ActualizarDiagnosticoRequest solicitud, ActualizarDiagnostico casoDeUso, CancellationToken cancellationToken) =>
            {
                await casoDeUso.EjecutarAsync(
                    new ActualizarDiagnosticoComando(id, diagnosticoId, solicitud.Descripcion, solicitud.Tipo, solicitud.EsPrincipal), cancellationToken);

                return Results.NoContent();
            })
            .RequierePermiso("diagnosticos.editar");

        grupo.MapDelete("/{id:guid}/diagnosticos/{diagnosticoId:guid}", async (
                Guid id, Guid diagnosticoId, EliminarDiagnostico casoDeUso, CancellationToken cancellationToken) =>
            {
                await casoDeUso.EjecutarAsync(id, diagnosticoId, cancellationToken);
                return Results.NoContent();
            })
            .RequierePermiso("diagnosticos.editar");
    }
}
