using SaaS.Veterinario.Domain.Mascotas;

namespace SaaS.Veterinario.Application.Abstracciones;

/// <summary>
/// Especie y Raza son catalogo global (no pertenecen a ninguna veterinaria), por eso ambas
/// se resuelven desde el mismo repositorio en vez de crear IRepositorioRazas aparte.
/// </summary>
public interface IRepositorioEspecies
{
    Task<Especie?> ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken);

    Task<Raza?> ObtenerRazaPorIdAsync(Guid id, CancellationToken cancellationToken);
}
