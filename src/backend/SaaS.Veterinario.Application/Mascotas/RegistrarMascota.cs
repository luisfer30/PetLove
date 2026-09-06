using SaaS.Veterinario.Application.Abstracciones;
using SaaS.Veterinario.Application.Errores;
using SaaS.Veterinario.Domain.Mascotas;

namespace SaaS.Veterinario.Application.Mascotas;

public sealed record RegistrarMascotaComando(
    string Nombre,
    Guid EspecieId,
    Guid? RazaId,
    string Sexo,
    DateOnly? FechaNacimiento,
    bool FechaNacimientoAproximada,
    string? Color,
    string? NumeroMicrochip,
    bool? Esterilizado,
    string? NumeroHistoria);

public sealed record RegistrarMascotaResultado(Guid MascotaVeterinariaId, Guid MascotaId, string CodigoPublico);

/// <summary>
/// Crea la Mascota global y su MascotaVeterinaria para el contexto actual dentro de UNA
/// transaccion (IUnidadDeTrabajo usado solo como frontera, no como Unit of Work generico):
/// si la segunda escritura falla, la Mascota global recien creada no debe quedar huerfana.
/// </summary>
public sealed class RegistrarMascota(
    IContextoVeterinaria contexto,
    IRepositorioMascotas repositorioMascotas,
    IRepositorioMascotasVeterinarias repositorioMascotasVeterinarias,
    IRepositorioEspecies repositorioEspecies,
    IGeneradorCodigoPublico generadorCodigoPublico,
    IUnidadDeTrabajo unidadDeTrabajo)
{
    public async Task<RegistrarMascotaResultado> EjecutarAsync(RegistrarMascotaComando comando, CancellationToken cancellationToken)
    {
        var veterinariaId = contexto.VeterinariaId
            ?? throw new InvalidOperationException("Se invocó RegistrarMascota sin contexto veterinario resuelto.");

        if (!Enum.TryParse<SexoMascota>(comando.Sexo, ignoreCase: true, out var sexo))
        {
            throw new ExcepcionAplicacion(CodigosError.MascotaSexoInvalido, "El sexo indicado no es válido.");
        }

        var especie = await repositorioEspecies.ObtenerPorIdAsync(comando.EspecieId, cancellationToken)
            ?? throw new ExcepcionAplicacion(CodigosError.MascotaEspecieInvalida, "La especie indicada no existe.");

        if (comando.RazaId is { } razaId)
        {
            var raza = await repositorioEspecies.ObtenerRazaPorIdAsync(razaId, cancellationToken);
            if (raza is null || raza.EspecieId != especie.Id)
            {
                throw new ExcepcionAplicacion(CodigosError.MascotaRazaInvalida, "La raza indicada no existe o no corresponde a la especie.");
            }
        }

        if (!string.IsNullOrWhiteSpace(comando.NumeroMicrochip)
            && await repositorioMascotas.ExisteMicrochipAsync(comando.NumeroMicrochip, cancellationToken))
        {
            throw new ExcepcionAplicacion(CodigosError.MascotaMicrochipDuplicado, "Ya existe una mascota registrada con ese número de microchip.");
        }

        var codigoPublico = await generadorCodigoPublico.GenerarUnicoAsync(
            Mascota.PrefijoCodigoPublico,
            (candidato, token) => repositorioMascotas.ExisteCodigoPublicoAsync(candidato, token),
            cancellationToken);

        Mascota? mascota = null;
        MascotaVeterinaria? mascotaVeterinaria = null;

        await unidadDeTrabajo.EjecutarEnTransaccionAsync(async token =>
        {
            mascota = Mascota.Crear(
                codigoPublico,
                comando.Nombre,
                comando.EspecieId,
                comando.RazaId,
                sexo,
                comando.FechaNacimiento,
                comando.FechaNacimientoAproximada,
                comando.Color,
                comando.NumeroMicrochip,
                comando.Esterilizado);

            await repositorioMascotas.AgregarAsync(mascota, token);

            mascotaVeterinaria = MascotaVeterinaria.Crear(veterinariaId, mascota.Id, comando.NumeroHistoria);

            await repositorioMascotasVeterinarias.AgregarAsync(mascotaVeterinaria, token);
        }, cancellationToken);

        return new RegistrarMascotaResultado(mascotaVeterinaria!.Id, mascota!.Id, mascota.CodigoPublico.Valor);
    }
}
