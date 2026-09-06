using SaaS.Veterinario.Domain.Tratamientos;

namespace SaaS.Veterinario.Application.Abstracciones;

/// <summary>
/// El aislamiento multi-tenant de un item se hereda de su PlanTratamiento (siempre se llega
/// aqui despues de cargar y validar el plan dueño), por eso no repite VeterinariaId.
/// </summary>
public interface IRepositorioItemsTratamiento
{
    Task<ItemTratamiento?> ObtenerPorIdAsync(Guid id, Guid planTratamientoId, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<ItemTratamiento>> ListarPorPlanAsync(Guid planTratamientoId, CancellationToken cancellationToken);

    Task AgregarAsync(ItemTratamiento item, CancellationToken cancellationToken);

    Task GuardarCambiosAsync(CancellationToken cancellationToken);
}
