using Microsoft.EntityFrameworkCore;
using SaaS.Veterinario.Application.Abstracciones;
using SaaS.Veterinario.Domain.Mascotas;
using SaaS.Veterinario.Infrastructure.Persistencia;

namespace SaaS.Veterinario.Infrastructure.Mascotas;

public sealed class RepositorioEspecies(SaaSVeterinarioDbContext contexto) : IRepositorioEspecies
{
    public Task<Especie?> ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken)
        => contexto.Especies.FirstOrDefaultAsync(e => e.Id == id, cancellationToken);

    public Task<Raza?> ObtenerRazaPorIdAsync(Guid id, CancellationToken cancellationToken)
        => contexto.Razas.FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
}
