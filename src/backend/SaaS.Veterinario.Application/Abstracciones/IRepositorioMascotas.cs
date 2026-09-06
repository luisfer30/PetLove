using SaaS.Veterinario.Domain.Mascotas;

namespace SaaS.Veterinario.Application.Abstracciones;

/// <summary>
/// Opera sobre la identidad GLOBAL de Mascota (R01) -- nunca filtra por veterinaria, porque
/// Mascota en si no conoce ninguna. El aislamiento por tenant sucede en
/// IRepositorioMascotasVeterinarias, no aqui.
/// </summary>
public interface IRepositorioMascotas
{
    Task<bool> ExisteCodigoPublicoAsync(string codigoPublico, CancellationToken cancellationToken);

    Task<bool> ExisteMicrochipAsync(string numeroMicrochip, CancellationToken cancellationToken);

    Task<Mascota?> ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken);

    Task<Mascota?> ObtenerPorCodigoPublicoAsync(string codigoPublico, CancellationToken cancellationToken);

    Task<Mascota?> ObtenerPorMicrochipAsync(string numeroMicrochip, CancellationToken cancellationToken);

    Task AgregarAsync(Mascota mascota, CancellationToken cancellationToken);
}
