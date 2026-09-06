using Microsoft.EntityFrameworkCore;
using SaaS.Veterinario.Application.Abstracciones;
using SaaS.Veterinario.Domain.Mascotas;
using SaaS.Veterinario.Infrastructure.Persistencia;

namespace SaaS.Veterinario.Infrastructure.Mascotas;

public sealed class RepositorioResponsables(SaaSVeterinarioDbContext contexto) : IRepositorioResponsables
{
    public Task<bool> ExistePrincipalActivoAsync(Guid mascotaVeterinariaId, CancellationToken cancellationToken)
        => contexto.ResponsablesMascota.AnyAsync(
            r => r.MascotaVeterinariaId == mascotaVeterinariaId
                && r.EsPrincipal
                && r.Estado == EstadoResponsableMascota.Activo,
            cancellationToken);

    public async Task<IReadOnlyCollection<ResponsableMascota>> ListarPorMascotaVeterinariaAsync(
        Guid mascotaVeterinariaId, Guid veterinariaId, CancellationToken cancellationToken)
        => await contexto.ResponsablesMascota
            .Where(r => r.MascotaVeterinariaId == mascotaVeterinariaId && r.VeterinariaId == veterinariaId)
            .ToListAsync(cancellationToken);

    public async Task AgregarAsync(ResponsableMascota responsable, CancellationToken cancellationToken)
    {
        contexto.ResponsablesMascota.Add(responsable);
        await contexto.SaveChangesAsync(cancellationToken);
    }
}
