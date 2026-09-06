using SaaS.Veterinario.Application.Abstracciones;
using SaaS.Veterinario.Application.Clinico;
using SaaS.Veterinario.Application.Errores;
using SaaS.Veterinario.Domain.Errores;
using SaaS.Veterinario.Domain.Tratamientos;

namespace SaaS.Veterinario.Application.Tratamientos;

public sealed record CancelarPlanTratamientoComando(Guid PlanTratamientoId, string Motivo);

/// <summary>PENDIENTE, ACTIVO o SUSPENDIDO -> CANCELADO. Cancela programaciones pendientes y los items vigentes, en una sola transaccion.</summary>
public sealed class CancelarPlanTratamiento(
    IContextoVeterinaria contexto,
    IRepositorioPlanesTratamiento repositorioPlanes,
    IRepositorioItemsTratamiento repositorioItems,
    IRepositorioProgramacionesTratamiento repositorioProgramaciones,
    IUnidadDeTrabajo unidadDeTrabajo)
{
    public async Task EjecutarAsync(CancelarPlanTratamientoComando comando, CancellationToken cancellationToken)
    {
        ValidadorPermisoClinico.ValidarUsuarioActual(contexto);

        var veterinariaId = contexto.VeterinariaId
            ?? throw new InvalidOperationException("Se invocó CancelarPlanTratamiento sin contexto veterinario resuelto.");

        var plan = await repositorioPlanes.ObtenerPorIdAsync(comando.PlanTratamientoId, veterinariaId, cancellationToken)
            ?? throw new ExcepcionAplicacion(CodigosError.TratamientoNoEncontrado, "El plan de tratamiento no existe en esta veterinaria.");

        await unidadDeTrabajo.EjecutarEnTransaccionAsync(async token =>
        {
            var momento = DateTimeOffset.UtcNow;

            try
            {
                plan.Cancelar(contexto.UsuarioId, comando.Motivo, momento);
            }
            catch (ExcepcionDominio excepcion)
            {
                throw new ExcepcionAplicacion(CodigosError.TratamientoEstadoInvalido, excepcion.Message);
            }

            await repositorioPlanes.GuardarCambiosAsync(token);
            await SuspenderPlanTratamiento.CancelarProgramacionesPendientesAsync(repositorioProgramaciones, plan.Id, momento, token);

            var items = await repositorioItems.ListarPorPlanAsync(plan.Id, token);
            var vigentes = items.Where(i => i.Estado is EstadoItemTratamiento.Activo or EstadoItemTratamiento.Suspendido).ToList();
            foreach (var item in vigentes)
            {
                item.Cancelar(momento);
            }

            if (vigentes.Count > 0)
            {
                await repositorioItems.GuardarCambiosAsync(token);
            }
        }, cancellationToken);
    }
}
