using SaaS.Veterinario.Application.Abstracciones;
using SaaS.Veterinario.Application.Errores;
using SaaS.Veterinario.Domain.Mascotas;

namespace SaaS.Veterinario.Application.Mascotas;

public sealed record VincularMascotaExistenteComando(string CodigoPublico, string? NumeroHistoria);

public sealed record VincularMascotaExistenteResultado(Guid MascotaVeterinariaId, string CodigoPublico, string Nombre);

/// <summary>
/// No copia ninguna informacion de otras veterinarias (R06): solo lee de Mascota (global,
/// sin datos de tenant) y crea una MascotaVeterinaria nueva y vacia para el contexto actual.
/// </summary>
public sealed class VincularMascotaExistente(
    IContextoVeterinaria contexto,
    IRepositorioMascotas repositorioMascotas,
    IRepositorioMascotasVeterinarias repositorioMascotasVeterinarias)
{
    public async Task<VincularMascotaExistenteResultado> EjecutarAsync(VincularMascotaExistenteComando comando, CancellationToken cancellationToken)
    {
        var veterinariaId = contexto.VeterinariaId
            ?? throw new InvalidOperationException("Se invocó VincularMascotaExistente sin contexto veterinario resuelto.");

        var mascota = await repositorioMascotas.ObtenerPorCodigoPublicoAsync(comando.CodigoPublico, cancellationToken)
            ?? throw new ExcepcionAplicacion(CodigosError.MascotaNoEncontrada, "No se encontró ninguna mascota con ese código.");

        if (await repositorioMascotasVeterinarias.ExisteVinculoAsync(veterinariaId, mascota.Id, cancellationToken))
        {
            throw new ExcepcionAplicacion(
                CodigosError.MascotaYaRegistradaEnVeterinaria,
                "Esta mascota ya está registrada en tu veterinaria.");
        }

        var mascotaVeterinaria = MascotaVeterinaria.Crear(veterinariaId, mascota.Id, comando.NumeroHistoria);
        await repositorioMascotasVeterinarias.AgregarAsync(mascotaVeterinaria, cancellationToken);

        return new VincularMascotaExistenteResultado(mascotaVeterinaria.Id, mascota.CodigoPublico.Valor, mascota.Nombre);
    }
}
