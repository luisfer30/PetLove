using SaaS.Veterinario.Domain.Errores;

namespace SaaS.Veterinario.Domain.Membresias;

public sealed class MembresiaVeterinaria
{
    private static readonly Dictionary<EstadoMembresiaVeterinaria, EstadoMembresiaVeterinaria[]> TransicionesPermitidas = new()
    {
        [EstadoMembresiaVeterinaria.Invitada] = [EstadoMembresiaVeterinaria.Activa, EstadoMembresiaVeterinaria.Finalizada],
        [EstadoMembresiaVeterinaria.Activa] = [EstadoMembresiaVeterinaria.Suspendida, EstadoMembresiaVeterinaria.Finalizada],
        [EstadoMembresiaVeterinaria.Suspendida] = [EstadoMembresiaVeterinaria.Activa, EstadoMembresiaVeterinaria.Finalizada],
        [EstadoMembresiaVeterinaria.Finalizada] = [],
    };

    private readonly List<MembresiaRol> _roles = [];

    public Guid Id { get; }
    public Guid UsuarioId { get; }
    public Guid VeterinariaId { get; }
    public EstadoMembresiaVeterinaria Estado { get; private set; }
    public DateTimeOffset FechaIngreso { get; }
    public DateTimeOffset? FechaFinalizacion { get; private set; }
    public DateTimeOffset FechaCreacion { get; }
    public DateTimeOffset FechaActualizacion { get; private set; }
    public IReadOnlyCollection<MembresiaRol> Roles => _roles.AsReadOnly();

    private MembresiaVeterinaria(
        Guid id,
        Guid usuarioId,
        Guid veterinariaId,
        DateTimeOffset fechaIngreso,
        DateTimeOffset fechaCreacion)
    {
        Id = id;
        UsuarioId = usuarioId;
        VeterinariaId = veterinariaId;
        Estado = EstadoMembresiaVeterinaria.Invitada;
        FechaIngreso = fechaIngreso;
        FechaCreacion = fechaCreacion;
        FechaActualizacion = fechaCreacion;
    }

    public static MembresiaVeterinaria Crear(
        Guid usuarioId,
        Guid veterinariaId,
        DateTimeOffset? fechaIngreso = null,
        DateTimeOffset? fechaCreacion = null)
    {
        if (usuarioId == Guid.Empty)
        {
            throw new ExcepcionDominio("El identificador del usuario es obligatorio.");
        }

        if (veterinariaId == Guid.Empty)
        {
            throw new ExcepcionDominio("El identificador de la veterinaria es obligatorio.");
        }

        var fecha = fechaCreacion ?? DateTimeOffset.UtcNow;
        var ingreso = fechaIngreso ?? fecha;

        return new MembresiaVeterinaria(Guid.NewGuid(), usuarioId, veterinariaId, ingreso, fecha);
    }

    public void Activar(DateTimeOffset momento) => CambiarEstado(EstadoMembresiaVeterinaria.Activa, momento);

    public void Suspender(DateTimeOffset momento) => CambiarEstado(EstadoMembresiaVeterinaria.Suspendida, momento);

    public void Finalizar(DateTimeOffset momento)
    {
        CambiarEstado(EstadoMembresiaVeterinaria.Finalizada, momento);
        FechaFinalizacion = momento;
    }

    public void AsignarRol(Guid rolId)
    {
        if (_roles.Any(r => r.RolId == rolId))
        {
            throw new ExcepcionDominio("La membresía ya tiene asignado ese rol.");
        }

        _roles.Add(new MembresiaRol(Id, rolId));
    }

    public void QuitarRol(Guid rolId)
    {
        var existente = _roles.FirstOrDefault(r => r.RolId == rolId);

        if (existente is null)
        {
            throw new ExcepcionDominio("La membresía no tiene asignado ese rol.");
        }

        _roles.Remove(existente);
    }

    private void CambiarEstado(EstadoMembresiaVeterinaria nuevoEstado, DateTimeOffset momento)
    {
        if (!TransicionesPermitidas[Estado].Contains(nuevoEstado))
        {
            throw new ExcepcionDominio(
                $"No se puede transicionar la membresía de '{Estado}' a '{nuevoEstado}'.");
        }

        Estado = nuevoEstado;
        FechaActualizacion = momento;
    }
}
