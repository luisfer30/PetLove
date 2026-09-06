using SaaS.Veterinario.Application.Abstracciones;
using SaaS.Veterinario.Application.Clinico;
using SaaS.Veterinario.Application.Errores;
using SaaS.Veterinario.Domain.Errores;

namespace SaaS.Veterinario.Application.Tratamientos;

public sealed record SuspenderPlanTratamientoComando(Guid PlanTratamientoId, string Motivo);

/// <summary>
/// ACTIVO -> SUSPENDIDO. Cancela las programaciones PENDIENTE del plan en la MISMA transaccion
/// (seccion 6/54/64): si la cancelacion falla, el plan permanece ACTIVO y las programaciones
/// mantienen su estado original. No regenera nada al reanudar (ver ReanudarPlanTratamiento).
/// </summary>
public sealed class SuspenderPlanTratamiento(
    IContextoVeterinaria contexto,
    IRepositorioPlanesTratamiento repositorioPlanes,
    IRepositorioProgramacionesTratamiento repositorioProgramaciones,
    IUnidadDeTrabajo unidadDeTrabajo)
{
    public async Task EjecutarAsync(SuspenderPlanTratamientoComando comando, CancellationToken cancellationToken)
    {
        ValidadorPermisoClinico.ValidarUsuarioActual(contexto);

        var veterinariaId = contexto.VeterinariaId
            ?? throw new InvalidOperationException("Se invocó SuspenderPlanTratamiento sin contexto veterinario resuelto.");

        var plan = await repositorioPlanes.ObtenerPorIdAsync(comando.PlanTratamientoId, veterinariaId, cancellationToken)
            ?? throw new ExcepcionAplicacion(CodigosError.TratamientoNoEncontrado, "El plan de tratamiento no existe en esta veterinaria.");

        await unidadDeTrabajo.EjecutarEnTransaccionAsync(async token =>
        {
            var momento = DateTimeOffset.UtcNow;

            try
            {
                plan.Suspender(contexto.UsuarioId, comando.Motivo, momento);
            }
            catch (ExcepcionDominio excepcion)
            {
                throw new ExcepcionAplicacion(CodigosError.TratamientoEstadoInvalido, excepcion.Message);
            }

            await repositorioPlanes.GuardarCambiosAsync(token);
            await CancelarProgramacionesPendientesAsync(repositorioProgramaciones, plan.Id, momento, token);
        }, cancellationToken);
    }

    internal static async Task CancelarProgramacionesPendientesAsync(
        IRepositorioProgramacionesTratamiento repositorioProgramaciones, Guid planTratamientoId, DateTimeOffset momento, CancellationToken cancellationToken)
    {
        var pendientes = await repositorioProgramaciones.ListarPendientesPorPlanAsync(planTratamientoId, cancellationToken);
        foreach (var programacion in pendientes)
        {
            programacion.Cancelar(momento);
        }

        if (pendientes.Count > 0)
        {
            await repositorioProgramaciones.GuardarCambiosAsync(cancellationToken);
        }
    }
}
