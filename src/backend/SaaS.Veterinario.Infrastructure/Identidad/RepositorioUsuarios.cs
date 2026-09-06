using Microsoft.EntityFrameworkCore;
using SaaS.Veterinario.Application.Abstracciones;
using SaaS.Veterinario.Domain.ObjetosValor;
using SaaS.Veterinario.Domain.Usuarios;
using SaaS.Veterinario.Infrastructure.Persistencia;

namespace SaaS.Veterinario.Infrastructure.Identidad;

public sealed class RepositorioUsuarios(SaaSVeterinarioDbContext contexto) : IRepositorioUsuarios
{
    public Task<bool> ExisteCorreoAsync(string correo, CancellationToken cancellationToken)
        => contexto.Usuarios.AnyAsync(u => u.Correo == Correo.Crear(correo), cancellationToken);

    public Task<Usuario?> ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken)
        => contexto.Usuarios.FirstOrDefaultAsync(u => u.Id == id, cancellationToken);

    public Task<Usuario?> ObtenerPorCorreoAsync(string correo, CancellationToken cancellationToken)
        => contexto.Usuarios.FirstOrDefaultAsync(u => u.Correo == Correo.Crear(correo), cancellationToken);

    public async Task AgregarAsync(Usuario usuario, CancellationToken cancellationToken)
    {
        contexto.Usuarios.Add(usuario);
        await contexto.SaveChangesAsync(cancellationToken);
    }

    public Task GuardarCambiosAsync(CancellationToken cancellationToken)
        => contexto.SaveChangesAsync(cancellationToken);
}
