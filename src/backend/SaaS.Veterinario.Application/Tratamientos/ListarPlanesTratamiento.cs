using SaaS.Veterinario.Application.Abstracciones;
using SaaS.Veterinario.Domain.Tratamientos;

namespace SaaS.Veterinario.Application.Tratamientos;

public sealed class ListarPlanesTratamiento(IContextoVeterinaria contexto, IRepositorioPlanesTratamiento repositorioPlanes)
{
    public Task<IReadOnlyCollection<PlanTratamiento>> EjecutarAsync(CancellationToken cancellationToken)
    {
        var veterinariaId = contexto.VeterinariaId
            ?? throw new InvalidOperationException("Se invocó ListarPlanesTratamiento sin contexto veterinario resuelto.");

        return repositorioPlanes.ListarPorVeterinariaAsync(veterinariaId, cancellationToken);
    }
}
