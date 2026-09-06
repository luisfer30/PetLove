using SaaS.Veterinario.Domain.Veterinarias;

namespace SaaS.Veterinario.Application.Abstracciones;

public interface IRepositorioVeterinarias
{
    Task<Veterinaria?> ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken);

    Task GuardarCambiosAsync(CancellationToken cancellationToken);
}
