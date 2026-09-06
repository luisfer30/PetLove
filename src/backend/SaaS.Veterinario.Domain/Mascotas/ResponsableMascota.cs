using SaaS.Veterinario.Domain.Errores;
using SaaS.Veterinario.Domain.ObjetosValor;

namespace SaaS.Veterinario.Domain.Mascotas;

/// <summary>
/// Pertenece al contexto de UNA veterinaria y UNA MascotaVeterinaria (nunca global). UsuarioId
/// no es parametro del factory a proposito: en V1 no se acepta libremente desde el cliente
/// HTTP, asi que la unica forma de que exista es una vinculacion explicita que todavia no
/// tiene caso de uso (quedara para la etapa que implemente esa vinculacion).
/// </summary>
public sealed class ResponsableMascota
{
    public Guid Id { get; }
    public Guid VeterinariaId { get; }
    public Guid MascotaVeterinariaId { get; }
    public Guid? ClienteVeterinariaId { get; }
    public Guid? UsuarioId { get; private set; }
    public string Nombre { get; private set; }
    public Correo? Correo { get; private set; }
    public string? Telefono { get; private set; }
    public TipoRelacionResponsable TipoRelacion { get; private set; }
    public bool EsPrincipal { get; private set; }
    public bool PuedeVerHistorial { get; private set; }
    public bool PuedeGestionarTratamientos { get; private set; }
    public EstadoVinculacionResponsable EstadoVinculacion { get; private set; }
    public EstadoResponsableMascota Estado { get; private set; }
    public DateTimeOffset FechaCreacion { get; }
    public DateTimeOffset FechaActualizacion { get; private set; }

    private ResponsableMascota(
        Guid id,
        Guid veterinariaId,
        Guid mascotaVeterinariaId,
        Guid? clienteVeterinariaId,
        string nombre,
        Correo? correo,
        string? telefono,
        TipoRelacionResponsable tipoRelacion,
        bool esPrincipal,
        bool puedeVerHistorial,
        bool puedeGestionarTratamientos,
        DateTimeOffset fechaCreacion)
    {
        Id = id;
        VeterinariaId = veterinariaId;
        MascotaVeterinariaId = mascotaVeterinariaId;
        ClienteVeterinariaId = clienteVeterinariaId;
        Nombre = nombre;
        Correo = correo;
        Telefono = telefono;
        TipoRelacion = tipoRelacion;
        EsPrincipal = esPrincipal;
        PuedeVerHistorial = puedeVerHistorial;
        PuedeGestionarTratamientos = puedeGestionarTratamientos;
        EstadoVinculacion = EstadoVinculacionResponsable.NoVinculado;
        Estado = EstadoResponsableMascota.Activo;
        FechaCreacion = fechaCreacion;
        FechaActualizacion = fechaCreacion;
    }

    public static ResponsableMascota Crear(
        Guid veterinariaId,
        Guid mascotaVeterinariaId,
        Guid? clienteVeterinariaId,
        string nombre,
        string? correo,
        string? telefono,
        TipoRelacionResponsable tipoRelacion,
        bool esPrincipal,
        bool puedeVerHistorial,
        bool puedeGestionarTratamientos,
        DateTimeOffset? fechaCreacion = null)
    {
        if (veterinariaId == Guid.Empty)
        {
            throw new ExcepcionDominio("El identificador de la veterinaria es obligatorio.");
        }

        if (mascotaVeterinariaId == Guid.Empty)
        {
            throw new ExcepcionDominio("El identificador de la ficha de la mascota es obligatorio.");
        }

        if (string.IsNullOrWhiteSpace(nombre))
        {
            throw new ExcepcionDominio("El nombre del responsable es obligatorio.");
        }

        var correoValidado = string.IsNullOrWhiteSpace(correo) ? null : Correo.Crear(correo);
        var fecha = fechaCreacion ?? DateTimeOffset.UtcNow;

        return new ResponsableMascota(
            Guid.NewGuid(),
            veterinariaId,
            mascotaVeterinariaId,
            clienteVeterinariaId,
            nombre.Trim(),
            correoValidado,
            string.IsNullOrWhiteSpace(telefono) ? null : telefono.Trim(),
            tipoRelacion,
            esPrincipal,
            puedeVerHistorial,
            puedeGestionarTratamientos,
            fecha);
    }
}
