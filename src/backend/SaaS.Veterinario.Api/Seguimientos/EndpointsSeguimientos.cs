using SaaS.Veterinario.Api.Autorizacion;
using SaaS.Veterinario.Application.Seguimientos;

namespace SaaS.Veterinario.Api.Seguimientos;

public static class EndpointsSeguimientos
{
    public static void MapearEndpointsSeguimientos(this IEndpointRouteBuilder app)
    {
        var grupo = app.MapGroup("/api/v1/seguimientos").RequireAuthorization();

        grupo.MapPost("/{id:guid}/agendar", async (Guid id, MarcarSeguimientoAgendadoRequest solicitud, MarcarSeguimientoAgendado casoDeUso, CancellationToken cancellationToken) =>
            {
                await casoDeUso.EjecutarAsync(new MarcarSeguimientoAgendadoComando(id, solicitud.CitaId), cancellationToken);
                return Results.NoContent();
            })
            .RequierePermiso("seguimientos.editar");

        grupo.MapPost("/{id:guid}/realizar", async (Guid id, RegistrarSeguimientoRealizado casoDeUso, CancellationToken cancellationToken) =>
            {
                await casoDeUso.EjecutarAsync(id, cancellationToken);
                return Results.NoContent();
            })
            .RequierePermiso("seguimientos.realizar");

        grupo.MapPost("/{id:guid}/cancelar", async (Guid id, CancelarSeguimiento casoDeUso, CancellationToken cancellationToken) =>
            {
                await casoDeUso.EjecutarAsync(id, cancellationToken);
                return Results.NoContent();
            })
            .RequierePermiso("seguimientos.cancelar");
    }
}
