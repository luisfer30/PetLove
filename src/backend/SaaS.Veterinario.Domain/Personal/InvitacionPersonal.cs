using SaaS.Veterinario.Domain.Errores;
using SaaS.Veterinario.Domain.ObjetosValor;

namespace SaaS.Veterinario.Domain.Personal;

/// <summary>
/// Invitacion de una persona a formar parte del personal de una Veterinaria. Esta etapa solo
/// cubre la creacion (Estado queda en Pendiente); Aceptar/Expirar/Cancelar se agregaran en la
/// etapa que implemente el flujo de aceptacion -- no existen todavia porque no hay caso de
/// uso real que los ejercite aun.
/// </summary>
public sealed class InvitacionPersonal
{
    private readonly List<InvitacionPersonalRol> _roles = [];

    public Guid Id { get; }
    public Guid VeterinariaId { get; }
    public Correo Correo { get; }
    public string TokenHash { get; }
    public EstadoInvitacionPersonal Estado { get; private set; }
    public DateTimeOffset FechaExpiracion { get; }
    public DateTimeOffset? FechaAceptacion { get; private set; }
    public Guid CreadoPorUsuarioId { get; }
    public DateTimeOffset FechaCreacion { get; }
    public IReadOnlyCollection<InvitacionPersonalRol> Roles => _roles.AsReadOnly();

    private InvitacionPersonal(
        Guid id,
        Guid veterinariaId,
        Correo correo,
        string tokenHash,
        Guid creadoPorUsuarioId,
        DateTimeOffset fechaExpiracion,
        DateTimeOffset fechaCreacion)
    {
        Id = id;
        VeterinariaId = veterinariaId;
        Correo = correo;
        TokenHash = tokenHash;
        Estado = EstadoInvitacionPersonal.Pendiente;
        FechaExpiracion = fechaExpiracion;
        CreadoPorUsuarioId = creadoPorUsuarioId;
        FechaCreacion = fechaCreacion;
    }

    /// <summary>
    /// TokenHash ya debe venir hasheado: el dominio no genera aleatoriedad criptografica
    /// (igual que CodigoPublico, esa responsabilidad es de Infrastructure).
    /// </summary>
    public static InvitacionPersonal Crear(
        Guid veterinariaId,
        string correo,
        string tokenHash,
        Guid creadoPorUsuarioId,
        DateTimeOffset fechaExpiracion,
        DateTimeOffset? fechaCreacion = null)
    {
        if (veterinariaId == Guid.Empty)
        {
            throw new ExcepcionDominio("El identificador de la veterinaria es obligatorio.");
        }

        if (creadoPorUsuarioId == Guid.Empty)
        {
            throw new ExcepcionDominio("El identificador de quien invita es obligatorio.");
        }

        if (string.IsNullOrWhiteSpace(tokenHash))
        {
            throw new ExcepcionDominio("El hash del token de invitación es obligatorio.");
        }

        var correoValidado = Correo.Crear(correo);
        var fecha = fechaCreacion ?? DateTimeOffset.UtcNow;

        if (fechaExpiracion <= fecha)
        {
            throw new ExcepcionDominio("La fecha de expiración de la invitación debe ser posterior a su fecha de creación.");
        }

        return new InvitacionPersonal(Guid.NewGuid(), veterinariaId, correoValidado, tokenHash, creadoPorUsuarioId, fechaExpiracion, fecha);
    }

    public void AsignarRol(Guid rolId)
    {
        if (_roles.Any(r => r.RolId == rolId))
        {
            throw new ExcepcionDominio("La invitación ya tiene asignado ese rol.");
        }

        _roles.Add(new InvitacionPersonalRol(Id, rolId));
    }
}
