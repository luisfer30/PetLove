using SaaS.Veterinario.Api.Autorizacion;
using SaaS.Veterinario.Application.Tratamientos;

namespace SaaS.Veterinario.Api.Tratamientos;

public static class EndpointsTratamientos
{
    public static void MapearEndpointsTratamientos(this IEndpointRouteBuilder app)
    {
        var grupo = app.MapGroup("/api/v1/tratamientos").RequireAuthorization();

        grupo.MapGet("/", async (ListarPlanesTratamiento casoDeUso, CancellationToken cancellationToken) =>
            {
                var planes = await casoDeUso.EjecutarAsync(cancellationToken);
                return Results.Ok(planes.Select(PlanTratamientoResponse.DeDominio));
            })
            .RequierePermiso("tratamientos.ver");

        grupo.MapGet("/{id:guid}", async (Guid id, ObtenerPlanTratamiento casoDeUso, CancellationToken cancellationToken) =>
            {
                var plan = await casoDeUso.EjecutarAsync(id, cancellationToken);
                return plan is null ? Results.NotFound() : Results.Ok(PlanTratamientoResponse.DeDominio(plan));
            })
            .RequierePermiso("tratamientos.ver");

        grupo.MapPost("/", async (CrearPlanTratamientoRequest solicitud, CrearPlanTratamiento casoDeUso, CancellationToken cancellationToken) =>
            {
                var resultado = await casoDeUso.EjecutarAsync(
                    new CrearPlanTratamientoComando(
                        solicitud.MascotaVeterinariaId, solicitud.ConsultaId, solicitud.Nombre, solicitud.Descripcion,
                        solicitud.FechaInicio, solicitud.FechaFinEstimada),
                    cancellationToken);

                return Results.Ok(new CrearPlanTratamientoResponse(resultado.PlanTratamientoId, resultado.Estado.ToString()));
            })
            .RequierePermiso("tratamientos.crear");

        grupo.MapPut("/{id:guid}", async (Guid id, ActualizarPlanTratamientoRequest solicitud, ActualizarPlanTratamiento casoDeUso, CancellationToken cancellationToken) =>
            {
                await casoDeUso.EjecutarAsync(
                    new ActualizarPlanTratamientoComando(id, solicitud.Nombre, solicitud.Descripcion, solicitud.FechaInicio, solicitud.FechaFinEstimada),
                    cancellationToken);

                return Results.NoContent();
            })
            .RequierePermiso("tratamientos.editar");

        grupo.MapPost("/{id:guid}/activar", async (Guid id, ActivarPlanTratamiento casoDeUso, CancellationToken cancellationToken) =>
            {
                await casoDeUso.EjecutarAsync(id, cancellationToken);
                return Results.NoContent();
            })
            .RequierePermiso("tratamientos.activar");

        grupo.MapPost("/{id:guid}/reanudar", async (Guid id, ReanudarPlanTratamiento casoDeUso, CancellationToken cancellationToken) =>
            {
                await casoDeUso.EjecutarAsync(id, cancellationToken);
                return Results.NoContent();
            })
            .RequierePermiso("tratamientos.activar");

        grupo.MapPost("/{id:guid}/suspender", async (Guid id, SuspenderPlanTratamientoRequest solicitud, SuspenderPlanTratamiento casoDeUso, CancellationToken cancellationToken) =>
            {
                await casoDeUso.EjecutarAsync(new SuspenderPlanTratamientoComando(id, solicitud.Motivo), cancellationToken);
                return Results.NoContent();
            })
            .RequierePermiso("tratamientos.suspender");

        grupo.MapPost("/{id:guid}/completar", async (Guid id, CompletarPlanTratamiento casoDeUso, CancellationToken cancellationToken) =>
            {
                await casoDeUso.EjecutarAsync(id, cancellationToken);
                return Results.NoContent();
            })
            .RequierePermiso("tratamientos.completar");

        grupo.MapPost("/{id:guid}/cancelar", async (Guid id, CancelarPlanTratamientoRequest solicitud, CancelarPlanTratamiento casoDeUso, CancellationToken cancellationToken) =>
            {
                await casoDeUso.EjecutarAsync(new CancelarPlanTratamientoComando(id, solicitud.Motivo), cancellationToken);
                return Results.NoContent();
            })
            .RequierePermiso("tratamientos.cancelar");

        grupo.MapGet("/{id:guid}/items", async (Guid id, ListarItemsTratamiento casoDeUso, CancellationToken cancellationToken) =>
            {
                var items = await casoDeUso.EjecutarAsync(id, cancellationToken);
                return Results.Ok(items.Select(ItemTratamientoResponse.DeDominio));
            })
            .RequierePermiso("tratamientos.ver");

        grupo.MapPost("/{id:guid}/items", async (Guid id, AgregarItemTratamientoRequest solicitud, AgregarItemTratamiento casoDeUso, CancellationToken cancellationToken) =>
            {
                var resultado = await casoDeUso.EjecutarAsync(
                    new AgregarItemTratamientoComando(
                        id, solicitud.Tipo, solicitud.Nombre, solicitud.Descripcion, solicitud.DosisCantidad, solicitud.DosisUnidad,
                        solicitud.ViaAdministracion, solicitud.FrecuenciaTipo, solicitud.IntervaloHoras, solicitud.VecesPorDia,
                        solicitud.Horarios, solicitud.FechaInicio, solicitud.FechaFin, solicitud.DuracionDias, solicitud.Instrucciones, solicitud.Orden),
                    cancellationToken);

                return Results.Ok(new AgregarItemTratamientoResponse(resultado.ItemTratamientoId));
            })
            .RequierePermiso("tratamientos.crear");

        grupo.MapPut("/{id:guid}/items/{itemId:guid}", async (
                Guid id, Guid itemId, ActualizarItemTratamientoRequest solicitud, ActualizarItemTratamiento casoDeUso, CancellationToken cancellationToken) =>
            {
                await casoDeUso.EjecutarAsync(new ActualizarItemTratamientoComando(id, itemId, solicitud.Descripcion, solicitud.Instrucciones), cancellationToken);
                return Results.NoContent();
            })
            .RequierePermiso("tratamientos.editar");

        grupo.MapPost("/{id:guid}/items/{itemId:guid}/suspender", async (Guid id, Guid itemId, SuspenderItemTratamiento casoDeUso, CancellationToken cancellationToken) =>
            {
                await casoDeUso.EjecutarAsync(id, itemId, cancellationToken);
                return Results.NoContent();
            })
            .RequierePermiso("tratamientos.suspender");

        grupo.MapPost("/{id:guid}/items/{itemId:guid}/cancelar", async (Guid id, Guid itemId, CancelarItemTratamiento casoDeUso, CancellationToken cancellationToken) =>
            {
                await casoDeUso.EjecutarAsync(id, itemId, cancellationToken);
                return Results.NoContent();
            })
            .RequierePermiso("tratamientos.cancelar");

        grupo.MapGet("/{id:guid}/programaciones", async (Guid id, ListarProgramacionesPorPlan casoDeUso, CancellationToken cancellationToken) =>
            {
                var programaciones = await casoDeUso.EjecutarAsync(id, cancellationToken);
                return Results.Ok(programaciones.Select(ProgramacionTratamientoResponse.DeDominio));
            })
            .RequierePermiso("programaciones.ver");

        grupo.MapGet("/{id:guid}/adherencia", async (Guid id, ObtenerAdherencia casoDeUso, CancellationToken cancellationToken) =>
            {
                var adherencia = await casoDeUso.EjecutarAsync(id, cancellationToken);
                return Results.Ok(AdherenciaResponse.DeAplicacion(adherencia));
            })
            .RequierePermiso("tratamientos.ver");
    }

    public static void MapearEndpointsProgramacionesTratamiento(this IEndpointRouteBuilder app)
    {
        var grupo = app.MapGroup("/api/v1/programaciones-tratamiento").RequireAuthorization();

        grupo.MapPost("/{id:guid}/realizar", async (Guid id, RegistrarProgramacionRealizadaRequest solicitud, RegistrarProgramacionRealizada casoDeUso, CancellationToken cancellationToken) =>
            {
                await casoDeUso.EjecutarAsync(new RegistrarProgramacionRealizadaComando(id, solicitud.Observacion), cancellationToken);
                return Results.NoContent();
            })
            .RequierePermiso("programaciones.registrar_realizacion");

        grupo.MapPost("/{id:guid}/omitir", async (Guid id, MarcarProgramacionOmitidaRequest solicitud, MarcarProgramacionOmitida casoDeUso, CancellationToken cancellationToken) =>
            {
                await casoDeUso.EjecutarAsync(new MarcarProgramacionOmitidaComando(id, solicitud.Observacion), cancellationToken);
                return Results.NoContent();
            })
            .RequierePermiso("programaciones.registrar_omision");
    }
}
