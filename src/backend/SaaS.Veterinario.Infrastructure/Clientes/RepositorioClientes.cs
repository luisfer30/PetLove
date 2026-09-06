using Microsoft.EntityFrameworkCore;
using SaaS.Veterinario.Application.Abstracciones;
using SaaS.Veterinario.Domain.Clientes;
using SaaS.Veterinario.Infrastructure.Persistencia;

namespace SaaS.Veterinario.Infrastructure.Clientes;

public sealed class RepositorioClientes(SaaSVeterinarioDbContext contexto) : IRepositorioClientes
{
    public Task<ClienteVeterinaria?> ObtenerPorIdAsync(Guid id, Guid veterinariaId, CancellationToken cancellationToken)
        => contexto.ClientesVeterinaria.FirstOrDefaultAsync(c => c.Id == id && c.VeterinariaId == veterinariaId, cancellationToken);

    public async Task<IReadOnlyCollection<ClienteVeterinaria>> ListarPorVeterinariaAsync(Guid veterinariaId, CancellationToken cancellationToken)
        => await contexto.ClientesVeterinaria
            .Where(c => c.VeterinariaId == veterinariaId)
            .ToListAsync(cancellationToken);

    public async Task AgregarAsync(ClienteVeterinaria cliente, CancellationToken cancellationToken)
    {
        contexto.ClientesVeterinaria.Add(cliente);
        await contexto.SaveChangesAsync(cancellationToken);
    }
}
