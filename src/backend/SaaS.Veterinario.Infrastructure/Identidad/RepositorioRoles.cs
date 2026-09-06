using Microsoft.EntityFrameworkCore;
using SaaS.Veterinario.Application.Abstracciones;
using SaaS.Veterinario.Domain.Roles;
using SaaS.Veterinario.Infrastructure.Persistencia;

namespace SaaS.Veterinario.Infrastructure.Identidad;

public sealed class RepositorioRoles(SaaSVeterinarioDbContext contexto) : IRepositorioRoles
{
    public async Task<IReadOnlyCollection<Rol>> ObtenerPorCodigosAsync(IReadOnlyCollection<string> codigos, CancellationToken cancellationToken)
    {
        var codigosNormalizados = codigos.Select(c => c.Trim().ToUpperInvariant()).ToArray();

        return await contexto.Roles
            .Where(r => codigosNormalizados.Contains(r.Codigo))
            .ToListAsync(cancellationToken);
    }
}
