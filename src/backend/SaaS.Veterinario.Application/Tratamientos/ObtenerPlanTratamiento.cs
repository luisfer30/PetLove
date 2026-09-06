using SaaS.Veterinario.Application.Abstracciones;
using SaaS.Veterinario.Domain.Tratamientos;

namespace SaaS.Veterinario.Application.Tratamientos;

public sealed class ObtenerPlanTratamiento(IContextoVeterinaria contexto, IRepositorioPlanesTratamiento repositorioPlanes)
{
    public Task<PlanTratamiento?> EjecutarAsync(Guid planTratamientoId, CancellationToken cancellationToken)
    {
        var veterinariaId = contexto.VeterinariaId
            ?? throw new InvalidOperationException("Se invocó ObtenerPlanTratamiento sin contexto veterinario resuelto.");

        return repositorioPlanes.ObtenerPorIdAsync(planTratamientoId, veterinariaId, cancellationToken);
    }
}
