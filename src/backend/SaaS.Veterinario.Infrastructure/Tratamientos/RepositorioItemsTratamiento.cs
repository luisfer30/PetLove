using Microsoft.EntityFrameworkCore;
using SaaS.Veterinario.Application.Abstracciones;
using SaaS.Veterinario.Domain.Tratamientos;
using SaaS.Veterinario.Infrastructure.Persistencia;

namespace SaaS.Veterinario.Infrastructure.Tratamientos;

public sealed class RepositorioItemsTratamiento(SaaSVeterinarioDbContext contexto) : IRepositorioItemsTratamiento
{
    public Task<ItemTratamiento?> ObtenerPorIdAsync(Guid id, Guid planTratamientoId, CancellationToken cancellationToken)
        => contexto.ItemsTratamiento
            .Include(i => i.Horarios)
            .FirstOrDefaultAsync(i => i.Id == id && i.PlanTratamientoId == planTratamientoId, cancellationToken);

    public async Task<IReadOnlyCollection<ItemTratamiento>> ListarPorPlanAsync(Guid planTratamientoId, CancellationToken cancellationToken)
        => await contexto.ItemsTratamiento
            .Include(i => i.Horarios)
            .Where(i => i.PlanTratamientoId == planTratamientoId)
            .ToListAsync(cancellationToken);

    public async Task AgregarAsync(ItemTratamiento item, CancellationToken cancellationToken)
    {
        contexto.ItemsTratamiento.Add(item);
        await contexto.SaveChangesAsync(cancellationToken);
    }

    public Task GuardarCambiosAsync(CancellationToken cancellationToken)
        => contexto.SaveChangesAsync(cancellationToken);
}
