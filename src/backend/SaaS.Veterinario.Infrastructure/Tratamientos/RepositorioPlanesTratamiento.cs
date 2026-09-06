using Microsoft.EntityFrameworkCore;
using SaaS.Veterinario.Application.Abstracciones;
using SaaS.Veterinario.Domain.Tratamientos;
using SaaS.Veterinario.Infrastructure.Persistencia;

namespace SaaS.Veterinario.Infrastructure.Tratamientos;

public sealed class RepositorioPlanesTratamiento(SaaSVeterinarioDbContext contexto) : IRepositorioPlanesTratamiento
{
    public Task<PlanTratamiento?> ObtenerPorIdAsync(Guid id, Guid veterinariaId, CancellationToken cancellationToken)
        => contexto.PlanesTratamiento.FirstOrDefaultAsync(p => p.Id == id && p.VeterinariaId == veterinariaId, cancellationToken);

    public async Task<IReadOnlyCollection<PlanTratamiento>> ListarPorVeterinariaAsync(Guid veterinariaId, CancellationToken cancellationToken)
        => await contexto.PlanesTratamiento
            .Where(p => p.VeterinariaId == veterinariaId)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyCollection<PlanTratamiento>> ListarPorMascotaVeterinariaAsync(Guid mascotaVeterinariaId, Guid veterinariaId, CancellationToken cancellationToken)
        => await contexto.PlanesTratamiento
            .Where(p => p.MascotaVeterinariaId == mascotaVeterinariaId && p.VeterinariaId == veterinariaId)
            .ToListAsync(cancellationToken);

    public async Task AgregarAsync(PlanTratamiento plan, CancellationToken cancellationToken)
    {
        contexto.PlanesTratamiento.Add(plan);
        await contexto.SaveChangesAsync(cancellationToken);
    }

    public Task GuardarCambiosAsync(CancellationToken cancellationToken)
        => contexto.SaveChangesAsync(cancellationToken);
}
