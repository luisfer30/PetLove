using SaaS.Veterinario.Domain.Clientes;

namespace SaaS.Veterinario.Application.Abstracciones;

/// <summary>
/// Todas las lecturas por Id exigen tambien VeterinariaId: un cliente de otra veterinaria
/// nunca debe distinguirse de uno inexistente (ver seccion 26 de la Etapa 6 -- aislamiento
/// multi-tenant explicito en cada consulta, nunca solo confiando en el UUID).
/// </summary>
public interface IRepositorioClientes
{
    Task<ClienteVeterinaria?> ObtenerPorIdAsync(Guid id, Guid veterinariaId, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<ClienteVeterinaria>> ListarPorVeterinariaAsync(Guid veterinariaId, CancellationToken cancellationToken);

    Task AgregarAsync(ClienteVeterinaria cliente, CancellationToken cancellationToken);
}
