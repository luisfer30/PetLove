using SaaS.Veterinario.Domain.Errores;
using SaaS.Veterinario.Domain.ObjetosValor;

namespace SaaS.Veterinario.Domain.Veterinarias;

public sealed class Veterinaria
{
    public const string PrefijoCodigoPublico = "VET";

    private static readonly Dictionary<EstadoVeterinaria, EstadoVeterinaria[]> TransicionesPermitidas = new()
    {
        [EstadoVeterinaria.PendienteActivacion] = [EstadoVeterinaria.Activa],
        [EstadoVeterinaria.Activa] = [EstadoVeterinaria.Suspendida, EstadoVeterinaria.Inactiva],
        [EstadoVeterinaria.Suspendida] = [EstadoVeterinaria.Activa],
        [EstadoVeterinaria.Inactiva] = [],
    };

    public Guid Id { get; }
    public CodigoPublico CodigoPublico { get; }
    public string NombreComercial { get; private set; }
    public string? RazonSocial { get; private set; }
    public string? Ruc { get; private set; }
    public Correo Correo { get; private set; }
    public string? Telefono { get; private set; }
    public string? Direccion { get; private set; }
    public string? Ciudad { get; private set; }
    public string Pais { get; private set; }
    public string ZonaHoraria { get; private set; }
    public EstadoVeterinaria Estado { get; private set; }
    public DateTimeOffset FechaCreacion { get; }
    public DateTimeOffset FechaActualizacion { get; private set; }
    public DateTimeOffset? FechaEliminacion { get; private set; }

    private Veterinaria(
        Guid id,
        CodigoPublico codigoPublico,
        string nombreComercial,
        string? razonSocial,
        string? ruc,
        Correo correo,
        string? telefono,
        string? direccion,
        string? ciudad,
        string pais,
        string zonaHoraria,
        DateTimeOffset fechaCreacion)
    {
        Id = id;
        CodigoPublico = codigoPublico;
        NombreComercial = nombreComercial;
        RazonSocial = razonSocial;
        Ruc = ruc;
        Correo = correo;
        Telefono = telefono;
        Direccion = direccion;
        Ciudad = ciudad;
        Pais = pais;
        ZonaHoraria = zonaHoraria;
        Estado = EstadoVeterinaria.PendienteActivacion;
        FechaCreacion = fechaCreacion;
        FechaActualizacion = fechaCreacion;
    }

    public static Veterinaria Crear(
        string codigoPublico,
        string nombreComercial,
        string correo,
        string pais,
        string zonaHoraria,
        string? razonSocial = null,
        string? ruc = null,
        string? telefono = null,
        string? direccion = null,
        string? ciudad = null,
        DateTimeOffset? fechaCreacion = null)
    {
        if (string.IsNullOrWhiteSpace(nombreComercial))
        {
            throw new ExcepcionDominio("El nombre comercial de la veterinaria es obligatorio.");
        }

        if (string.IsNullOrWhiteSpace(pais))
        {
            throw new ExcepcionDominio("El país de la veterinaria es obligatorio.");
        }

        if (string.IsNullOrWhiteSpace(zonaHoraria))
        {
            throw new ExcepcionDominio("La zona horaria de la veterinaria es obligatoria.");
        }

        var codigoValidado = CodigoPublico.Crear(codigoPublico, PrefijoCodigoPublico);
        var correoValidado = Correo.Crear(correo);
        var fecha = fechaCreacion ?? DateTimeOffset.UtcNow;

        return new Veterinaria(
            Guid.NewGuid(),
            codigoValidado,
            nombreComercial.Trim(),
            NormalizarOpcional(razonSocial),
            NormalizarOpcional(ruc),
            correoValidado,
            NormalizarOpcional(telefono),
            NormalizarOpcional(direccion),
            NormalizarOpcional(ciudad),
            pais.Trim(),
            zonaHoraria.Trim(),
            fecha);
    }

    /// <summary>
    /// Actualiza los datos descriptivos de negocio. No toca Correo (identificador de la
    /// cuenta, con sus propias implicaciones si cambiara), CodigoPublico, Estado ni
    /// FechaEliminacion -- esos tienen su propio metodo dedicado o quedan fuera de esta
    /// version por decision explicita.
    /// </summary>
    public void ActualizarInformacion(
        string nombreComercial,
        string? razonSocial,
        string? ruc,
        string? telefono,
        string? direccion,
        string? ciudad,
        string pais,
        string zonaHoraria,
        DateTimeOffset momento)
    {
        if (string.IsNullOrWhiteSpace(nombreComercial))
        {
            throw new ExcepcionDominio("El nombre comercial de la veterinaria es obligatorio.");
        }

        if (string.IsNullOrWhiteSpace(pais))
        {
            throw new ExcepcionDominio("El país de la veterinaria es obligatorio.");
        }

        if (string.IsNullOrWhiteSpace(zonaHoraria))
        {
            throw new ExcepcionDominio("La zona horaria de la veterinaria es obligatoria.");
        }

        NombreComercial = nombreComercial.Trim();
        RazonSocial = NormalizarOpcional(razonSocial);
        Ruc = NormalizarOpcional(ruc);
        Telefono = NormalizarOpcional(telefono);
        Direccion = NormalizarOpcional(direccion);
        Ciudad = NormalizarOpcional(ciudad);
        Pais = pais.Trim();
        ZonaHoraria = zonaHoraria.Trim();
        FechaActualizacion = momento;
    }

    public void Activar(DateTimeOffset momento) => CambiarEstado(EstadoVeterinaria.Activa, momento);

    public void Suspender(DateTimeOffset momento) => CambiarEstado(EstadoVeterinaria.Suspendida, momento);

    public void Desactivar(DateTimeOffset momento) => CambiarEstado(EstadoVeterinaria.Inactiva, momento);

    private void CambiarEstado(EstadoVeterinaria nuevoEstado, DateTimeOffset momento)
    {
        if (!TransicionesPermitidas[Estado].Contains(nuevoEstado))
        {
            throw new ExcepcionDominio(
                $"No se puede transicionar la veterinaria de '{Estado}' a '{nuevoEstado}'.");
        }

        Estado = nuevoEstado;
        FechaActualizacion = momento;
    }

    private static string? NormalizarOpcional(string? valor)
        => string.IsNullOrWhiteSpace(valor) ? null : valor.Trim();
}
