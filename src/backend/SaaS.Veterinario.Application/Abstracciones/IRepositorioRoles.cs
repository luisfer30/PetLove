using SaaS.Veterinario.Domain.Roles;

namespace SaaS.Veterinario.Application.Abstracciones;

public interface IRepositorioRoles
{
    /// <summary>
    /// Resuelve los Rol existentes que coincidan con los codigos indicados (comparacion
    /// case-insensitive). Los codigos que no correspondan a ningun Rol simplemente no
    /// aparecen en el resultado -- el llamador es quien decide si eso es un error.
    /// </summary>
    Task<IReadOnlyCollection<Rol>> ObtenerPorCodigosAsync(IReadOnlyCollection<string> codigos, CancellationToken cancellationToken);
}
