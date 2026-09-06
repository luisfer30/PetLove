using SaaS.Veterinario.Domain.Errores;
using SaaS.Veterinario.Domain.ObjetosValor;

namespace SaaS.Veterinario.Domain.Mascotas;

/// <summary>
/// Identidad global de plataforma (R01): Mascota NO conoce ninguna Veterinaria, Cliente ni
/// Responsable (R02, "No incluir VeterinariaId/ClienteId/ResponsableId dentro de Mascota").
/// Toda relacion con una veterinaria especifica vive en MascotaVeterinaria.
/// </summary>
public sealed class Mascota
{
    public const string PrefijoCodigoPublico = "PET";

    private static readonly Dictionary<EstadoMascota, EstadoMascota[]> TransicionesPermitidas = new()
    {
        [EstadoMascota.Activa] = [EstadoMascota.Inactiva, EstadoMascota.Fallecida],
        [EstadoMascota.Inactiva] = [EstadoMascota.Activa, EstadoMascota.Fallecida],
        [EstadoMascota.Fallecida] = [],
    };

    public Guid Id { get; }
    public CodigoPublico CodigoPublico { get; }
    public string Nombre { get; private set; }
    public Guid EspecieId { get; private set; }
    public Guid? RazaId { get; private set; }
    public SexoMascota Sexo { get; private set; }
    public DateOnly? FechaNacimiento { get; private set; }
    public bool FechaNacimientoAproximada { get; private set; }
    public string? Color { get; private set; }
    public string? NumeroMicrochip { get; private set; }
    public bool? Esterilizado { get; private set; }
    public EstadoMascota Estado { get; private set; }
    public DateTimeOffset FechaCreacion { get; }
    public DateTimeOffset FechaActualizacion { get; private set; }
    public DateTimeOffset? FechaEliminacion { get; private set; }

    private Mascota(
        Guid id,
        CodigoPublico codigoPublico,
        string nombre,
        Guid especieId,
        Guid? razaId,
        SexoMascota sexo,
        DateOnly? fechaNacimiento,
        bool fechaNacimientoAproximada,
        string? color,
        string? numeroMicrochip,
        bool? esterilizado,
        DateTimeOffset fechaCreacion)
    {
        Id = id;
        CodigoPublico = codigoPublico;
        Nombre = nombre;
        EspecieId = especieId;
        RazaId = razaId;
        Sexo = sexo;
        FechaNacimiento = fechaNacimiento;
        FechaNacimientoAproximada = fechaNacimientoAproximada;
        Color = color;
        NumeroMicrochip = numeroMicrochip;
        Esterilizado = esterilizado;
        Estado = EstadoMascota.Activa;
        FechaCreacion = fechaCreacion;
        FechaActualizacion = fechaCreacion;
    }

    public static Mascota Crear(
        string codigoPublico,
        string nombre,
        Guid especieId,
        Guid? razaId,
        SexoMascota sexo,
        DateOnly? fechaNacimiento,
        bool fechaNacimientoAproximada,
        string? color,
        string? numeroMicrochip,
        bool? esterilizado,
        DateTimeOffset? fechaCreacion = null)
    {
        if (string.IsNullOrWhiteSpace(nombre))
        {
            throw new ExcepcionDominio("El nombre de la mascota es obligatorio.");
        }

        if (especieId == Guid.Empty)
        {
            throw new ExcepcionDominio("La especie de la mascota es obligatoria.");
        }

        var fecha = fechaCreacion ?? DateTimeOffset.UtcNow;

        if (fechaNacimiento is { } nacimiento && nacimiento > DateOnly.FromDateTime(fecha.UtcDateTime))
        {
            throw new ExcepcionDominio("La fecha de nacimiento no puede estar en el futuro.");
        }

        var codigoValidado = CodigoPublico.Crear(codigoPublico, PrefijoCodigoPublico);

        return new Mascota(
            Guid.NewGuid(),
            codigoValidado,
            nombre.Trim(),
            especieId,
            razaId,
            sexo,
            fechaNacimiento,
            fechaNacimientoAproximada,
            NormalizarOpcional(color),
            NormalizarMicrochip(numeroMicrochip),
            esterilizado,
            fecha);
    }

    public void Activar(DateTimeOffset momento) => CambiarEstado(EstadoMascota.Activa, momento);

    public void Desactivar(DateTimeOffset momento) => CambiarEstado(EstadoMascota.Inactiva, momento);

    public void MarcarFallecida(DateTimeOffset momento) => CambiarEstado(EstadoMascota.Fallecida, momento);

    private void CambiarEstado(EstadoMascota nuevoEstado, DateTimeOffset momento)
    {
        if (!TransicionesPermitidas[Estado].Contains(nuevoEstado))
        {
            throw new ExcepcionDominio($"No se puede transicionar la mascota de '{Estado}' a '{nuevoEstado}'.");
        }

        Estado = nuevoEstado;
        FechaActualizacion = momento;
    }

    private static string? NormalizarOpcional(string? valor)
        => string.IsNullOrWhiteSpace(valor) ? null : valor.Trim();

    // Una cadena vacia nunca se trata como microchip real.
    private static string? NormalizarMicrochip(string? valor)
        => string.IsNullOrWhiteSpace(valor) ? null : valor.Trim();
}
