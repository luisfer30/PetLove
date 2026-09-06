using SaaS.Veterinario.Application.Abstracciones;
using SaaS.Veterinario.Application.Clinico;
using SaaS.Veterinario.Application.Errores;
using SaaS.Veterinario.Domain.Errores;
using SaaS.Veterinario.Domain.Tratamientos;

namespace SaaS.Veterinario.Application.Tratamientos;

/// <summary>Cancela el item y, en la misma transaccion, sus propias programaciones PENDIENTE (mismo criterio que cancelar el plan completo, pero acotado a este item).</summary>
public sealed class CancelarItemTratamiento(
    IContextoVeterinaria contexto,
    IRepositorioPlanesTratamiento repositorioPlanes,
    IRepositorioItemsTratamiento repositorioItems,
    IRepositorioProgramacionesTratamiento repositorioProgramaciones,
    IUnidadDeTrabajo unidadDeTrabajo)
{
    public async Task EjecutarAsync(Guid planTratamientoId, Guid itemTratamientoId, CancellationToken cancellationToken)
    {
        ValidadorPermisoClinico.ValidarUsuarioActual(contexto);

        var veterinariaId = contexto.VeterinariaId
            ?? throw new InvalidOperationException("Se invocó CancelarItemTratamiento sin contexto veterinario resuelto.");

        var plan = await repositorioPlanes.ObtenerPorIdAsync(planTratamientoId, veterinariaId, cancellationToken)
            ?? throw new ExcepcionAplicacion(CodigosError.TratamientoNoEncontrado, "El plan de tratamiento no existe en esta veterinaria.");

        if (plan.Estado is EstadoPlanTratamiento.Completado or EstadoPlanTratamiento.Cancelado)
        {
            throw new ExcepcionAplicacion(CodigosError.TratamientoEstadoInvalido, $"No se puede modificar un item de un plan en estado '{plan.Estado}'.");
        }

        var item = await repositorioItems.ObtenerPorIdAsync(itemTratamientoId, plan.Id, cancellationToken)
            ?? throw new ExcepcionAplicacion(CodigosError.TratamientoItemNoEncontrado, "El item de tratamiento no existe en este plan.");

        await unidadDeTrabajo.EjecutarEnTransaccionAsync(async token =>
        {
            var momento = DateTimeOffset.UtcNow;

            try
            {
                item.Cancelar(momento);
            }
            catch (ExcepcionDominio excepcion)
            {
                throw new ExcepcionAplicacion(CodigosError.TratamientoItemEstadoInvalido, excepcion.Message);
            }

            await repositorioItems.GuardarCambiosAsync(token);

            var pendientesDelPlan = await repositorioProgramaciones.ListarPendientesPorPlanAsync(plan.Id, token);
            var pendientesDelItem = pendientesDelPlan.Where(p => p.ItemTratamientoId == item.Id).ToList();
            foreach (var programacion in pendientesDelItem)
            {
                programacion.Cancelar(momento);
            }

            if (pendientesDelItem.Count > 0)
            {
                await repositorioProgramaciones.GuardarCambiosAsync(token);
            }
        }, cancellationToken);
    }
}
