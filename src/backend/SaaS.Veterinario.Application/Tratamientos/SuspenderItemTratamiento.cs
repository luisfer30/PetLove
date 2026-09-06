using SaaS.Veterinario.Application.Abstracciones;
using SaaS.Veterinario.Application.Clinico;
using SaaS.Veterinario.Application.Errores;
using SaaS.Veterinario.Domain.Errores;
using SaaS.Veterinario.Domain.Tratamientos;

namespace SaaS.Veterinario.Application.Tratamientos;

public sealed class SuspenderItemTratamiento(
    IContextoVeterinaria contexto, IRepositorioPlanesTratamiento repositorioPlanes, IRepositorioItemsTratamiento repositorioItems)
{
    public async Task EjecutarAsync(Guid planTratamientoId, Guid itemTratamientoId, CancellationToken cancellationToken)
    {
        ValidadorPermisoClinico.ValidarUsuarioActual(contexto);

        var veterinariaId = contexto.VeterinariaId
            ?? throw new InvalidOperationException("Se invocó SuspenderItemTratamiento sin contexto veterinario resuelto.");

        var plan = await repositorioPlanes.ObtenerPorIdAsync(planTratamientoId, veterinariaId, cancellationToken)
            ?? throw new ExcepcionAplicacion(CodigosError.TratamientoNoEncontrado, "El plan de tratamiento no existe en esta veterinaria.");

        if (plan.Estado is EstadoPlanTratamiento.Completado or EstadoPlanTratamiento.Cancelado)
        {
            throw new ExcepcionAplicacion(CodigosError.TratamientoEstadoInvalido, $"No se puede modificar un item de un plan en estado '{plan.Estado}'.");
        }

        var item = await repositorioItems.ObtenerPorIdAsync(itemTratamientoId, plan.Id, cancellationToken)
            ?? throw new ExcepcionAplicacion(CodigosError.TratamientoItemNoEncontrado, "El item de tratamiento no existe en este plan.");

        try
        {
            item.Suspender(DateTimeOffset.UtcNow);
        }
        catch (ExcepcionDominio excepcion)
        {
            throw new ExcepcionAplicacion(CodigosError.TratamientoItemEstadoInvalido, excepcion.Message);
        }

        await repositorioItems.GuardarCambiosAsync(cancellationToken);
    }
}
