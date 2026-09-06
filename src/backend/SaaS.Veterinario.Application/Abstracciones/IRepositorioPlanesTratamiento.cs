using SaaS.Veterinario.Domain.Tratamientos;

namespace SaaS.Veterinario.Application.Abstracciones;

/// <summary>Toda lectura por Id exige tambien VeterinariaId (R06): un plan de otra veterinaria nunca debe distinguirse de uno inexistente.</summary>
public interface IRepositorioPlanesTratamiento
{
    Task<PlanTratamiento?> ObtenerPorIdAsync(Guid id, Guid veterinariaId, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<PlanTratamiento>> ListarPorVeterinariaAsync(Guid veterinariaId, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<PlanTratamiento>> ListarPorMascotaVeterinariaAsync(Guid mascotaVeterinariaId, Guid veterinariaId, CancellationToken cancellationToken);

    Task AgregarAsync(PlanTratamiento plan, CancellationToken cancellationToken);

    Task GuardarCambiosAsync(CancellationToken cancellationToken);
}
