using SaaS.Veterinario.Application.Abstracciones;
using SaaS.Veterinario.Domain.Veterinarias;
using SaaS.Veterinario.Infrastructure.Persistencia;

namespace SaaS.Veterinario.Infrastructure.Identidad;

public sealed class RepositorioVeterinarias(SaaSVeterinarioDbContext contexto) : IRepositorioVeterinarias
{
    public Task<Veterinaria?> ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken)
        => contexto.Veterinarias.FindAsync([id], cancellationToken).AsTask();

    public Task GuardarCambiosAsync(CancellationToken cancellationToken)
        => contexto.SaveChangesAsync(cancellationToken);
}
