using Microsoft.EntityFrameworkCore;
using SaaS.Veterinario.Application.Abstracciones;
using SaaS.Veterinario.Domain.ObjetosValor;
using SaaS.Veterinario.Domain.Mascotas;
using SaaS.Veterinario.Infrastructure.Persistencia;

namespace SaaS.Veterinario.Infrastructure.Mascotas;

/// <summary>Opera sobre la identidad GLOBAL de Mascota (R01); nunca filtra por veterinaria.</summary>
public sealed class RepositorioMascotas(SaaSVeterinarioDbContext contexto) : IRepositorioMascotas
{
    public Task<bool> ExisteCodigoPublicoAsync(string codigoPublico, CancellationToken cancellationToken)
        => contexto.Mascotas.AnyAsync(m => m.CodigoPublico == CodigoPublico.Crear(codigoPublico, Mascota.PrefijoCodigoPublico), cancellationToken);

    public Task<bool> ExisteMicrochipAsync(string numeroMicrochip, CancellationToken cancellationToken)
        => contexto.Mascotas.AnyAsync(m => m.NumeroMicrochip == numeroMicrochip, cancellationToken);

    public Task<Mascota?> ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken)
        => contexto.Mascotas.FirstOrDefaultAsync(m => m.Id == id, cancellationToken);

    public Task<Mascota?> ObtenerPorCodigoPublicoAsync(string codigoPublico, CancellationToken cancellationToken)
        => contexto.Mascotas.FirstOrDefaultAsync(m => m.CodigoPublico == CodigoPublico.Crear(codigoPublico, Mascota.PrefijoCodigoPublico), cancellationToken);

    public Task<Mascota?> ObtenerPorMicrochipAsync(string numeroMicrochip, CancellationToken cancellationToken)
        => contexto.Mascotas.FirstOrDefaultAsync(m => m.NumeroMicrochip == numeroMicrochip, cancellationToken);

    public async Task AgregarAsync(Mascota mascota, CancellationToken cancellationToken)
    {
        contexto.Mascotas.Add(mascota);
        await contexto.SaveChangesAsync(cancellationToken);
    }
}
