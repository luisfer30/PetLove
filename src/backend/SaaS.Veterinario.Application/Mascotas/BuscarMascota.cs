using SaaS.Veterinario.Application.Abstracciones;
using SaaS.Veterinario.Domain.Mascotas;

namespace SaaS.Veterinario.Application.Mascotas;

/// <summary>
/// Identidad minima de una Mascota global -- nunca incluye MascotaId interno, veterinarias,
/// responsables, historial ni NumeroHistoria de nadie (R06). NumeroMicrochip solo se llena
/// cuando la busqueda fue POR microchip: en ese caso el buscador ya conocia el valor (se lo
/// esta devolviendo tal cual lo escribio), asi que no se revela nada nuevo. Buscando por
/// CodigoPublico, el microchip queda deliberadamente fuera de la respuesta.
/// </summary>
public sealed record MascotaEncontrada(
    string CodigoPublico,
    string Nombre,
    string Especie,
    string? Raza,
    string Sexo,
    DateOnly? FechaNacimiento,
    string? NumeroMicrochip);

public sealed class BuscarMascota(IRepositorioMascotas repositorioMascotas, IRepositorioEspecies repositorioEspecies)
{
    public async Task<MascotaEncontrada?> PorCodigoPublicoAsync(string codigoPublico, CancellationToken cancellationToken)
    {
        var mascota = await repositorioMascotas.ObtenerPorCodigoPublicoAsync(codigoPublico, cancellationToken);
        return mascota is null ? null : await MapearAsync(mascota, incluirMicrochip: false, cancellationToken);
    }

    public async Task<MascotaEncontrada?> PorMicrochipAsync(string numeroMicrochip, CancellationToken cancellationToken)
    {
        var mascota = await repositorioMascotas.ObtenerPorMicrochipAsync(numeroMicrochip, cancellationToken);
        return mascota is null ? null : await MapearAsync(mascota, incluirMicrochip: true, cancellationToken);
    }

    private async Task<MascotaEncontrada> MapearAsync(Mascota mascota, bool incluirMicrochip, CancellationToken cancellationToken)
    {
        var especie = await repositorioEspecies.ObtenerPorIdAsync(mascota.EspecieId, cancellationToken);
        var raza = mascota.RazaId is { } razaId ? await repositorioEspecies.ObtenerRazaPorIdAsync(razaId, cancellationToken) : null;

        return new MascotaEncontrada(
            mascota.CodigoPublico.Valor,
            mascota.Nombre,
            especie?.Nombre ?? "Desconocida",
            raza?.Nombre,
            mascota.Sexo.ToString(),
            mascota.FechaNacimiento,
            incluirMicrochip ? mascota.NumeroMicrochip : null);
    }
}
