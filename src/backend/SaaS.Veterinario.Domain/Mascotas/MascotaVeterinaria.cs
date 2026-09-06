using SaaS.Veterinario.Domain.Errores;

namespace SaaS.Veterinario.Domain.Mascotas;

/// <summary>
/// La relacion privada de una Veterinaria con una Mascota global (R04). Unicidad
/// (veterinaria_id, mascota_id) se protege en base de datos: una Mascota global puede tener
/// muchas MascotaVeterinaria (una por veterinaria distinta) pero nunca dos en la misma.
/// </summary>
public sealed class MascotaVeterinaria
{
    private static readonly Dictionary<EstadoMascotaVeterinaria, EstadoMascotaVeterinaria[]> TransicionesPermitidas = new()
    {
        [EstadoMascotaVeterinaria.Activa] = [EstadoMascotaVeterinaria.Inactiva],
        [EstadoMascotaVeterinaria.Inactiva] = [EstadoMascotaVeterinaria.Activa],
    };

    public Guid Id { get; }
    public Guid VeterinariaId { get; }
    public Guid MascotaId { get; }
    public string? NumeroHistoria { get; private set; }
    public EstadoMascotaVeterinaria Estado { get; private set; }
    public DateTimeOffset FechaRegistro { get; }
    public string? ObservacionesInternas { get; private set; }
    public DateTimeOffset FechaCreacion { get; }
    public DateTimeOffset FechaActualizacion { get; private set; }
    public DateTimeOffset? FechaEliminacion { get; private set; }

    private MascotaVeterinaria(
        Guid id,
        Guid veterinariaId,
        Guid mascotaId,
        string? numeroHistoria,
        string? observacionesInternas,
        DateTimeOffset fechaRegistro,
        DateTimeOffset fechaCreacion)
    {
        Id = id;
        VeterinariaId = veterinariaId;
        MascotaId = mascotaId;
        NumeroHistoria = numeroHistoria;
        Estado = EstadoMascotaVeterinaria.Activa;
        FechaRegistro = fechaRegistro;
        ObservacionesInternas = observacionesInternas;
        FechaCreacion = fechaCreacion;
        FechaActualizacion = fechaCreacion;
    }

    public static MascotaVeterinaria Crear(
        Guid veterinariaId,
        Guid mascotaId,
        string? numeroHistoria = null,
        string? observacionesInternas = null,
        DateTimeOffset? fechaRegistro = null,
        DateTimeOffset? fechaCreacion = null)
    {
        if (veterinariaId == Guid.Empty)
        {
            throw new ExcepcionDominio("El identificador de la veterinaria es obligatorio.");
        }

        if (mascotaId == Guid.Empty)
        {
            throw new ExcepcionDominio("El identificador de la mascota es obligatorio.");
        }

        var fecha = fechaCreacion ?? DateTimeOffset.UtcNow;

        return new MascotaVeterinaria(
            Guid.NewGuid(),
            veterinariaId,
            mascotaId,
            NormalizarOpcional(numeroHistoria),
            NormalizarOpcional(observacionesInternas),
            fechaRegistro ?? fecha,
            fecha);
    }

    public void Activar(DateTimeOffset momento) => CambiarEstado(EstadoMascotaVeterinaria.Activa, momento);

    public void Desactivar(DateTimeOffset momento) => CambiarEstado(EstadoMascotaVeterinaria.Inactiva, momento);

    private void CambiarEstado(EstadoMascotaVeterinaria nuevoEstado, DateTimeOffset momento)
    {
        if (!TransicionesPermitidas[Estado].Contains(nuevoEstado))
        {
            throw new ExcepcionDominio($"No se puede transicionar la ficha de la mascota de '{Estado}' a '{nuevoEstado}'.");
        }

        Estado = nuevoEstado;
        FechaActualizacion = momento;
    }

    private static string? NormalizarOpcional(string? valor)
        => string.IsNullOrWhiteSpace(valor) ? null : valor.Trim();
}
