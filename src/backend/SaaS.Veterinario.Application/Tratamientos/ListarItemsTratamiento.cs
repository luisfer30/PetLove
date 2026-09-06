using SaaS.Veterinario.Application.Abstracciones;
using SaaS.Veterinario.Application.Errores;
using SaaS.Veterinario.Domain.Tratamientos;

namespace SaaS.Veterinario.Application.Tratamientos;

public sealed class ListarItemsTratamiento(IContextoVeterinaria contexto, IRepositorioPlanesTratamiento repositorioPlanes, IRepositorioItemsTratamiento repositorioItems)
{
    public async Task<IReadOnlyCollection<ItemTratamiento>> EjecutarAsync(Guid planTratamientoId, CancellationToken cancellationToken)
    {
        var veterinariaId = contexto.VeterinariaId
            ?? throw new InvalidOperationException("Se invocó ListarItemsTratamiento sin contexto veterinario resuelto.");

        _ = await repositorioPlanes.ObtenerPorIdAsync(planTratamientoId, veterinariaId, cancellationToken)
            ?? throw new ExcepcionAplicacion(CodigosError.TratamientoNoEncontrado, "El plan de tratamiento no existe en esta veterinaria.");

        return await repositorioItems.ListarPorPlanAsync(planTratamientoId, cancellationToken);
    }
}
