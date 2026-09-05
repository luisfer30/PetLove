using SaaS.Veterinario.Domain.Errores;

namespace SaaS.Veterinario.Domain.Roles;

public sealed class Rol
{
    private readonly List<RolPermiso> _permisos = [];

    public Guid Id { get; }
    public string Codigo { get; }
    public string Nombre { get; private set; }
    public string? Descripcion { get; private set; }
    public EstadoRol Estado { get; private set; }
    public IReadOnlyCollection<RolPermiso> Permisos => _permisos.AsReadOnly();

    private Rol(Guid id, string codigo, string nombre, string? descripcion)
    {
        Id = id;
        Codigo = codigo;
        Nombre = nombre;
        Descripcion = descripcion;
        Estado = EstadoRol.Activo;
    }

    public static Rol Crear(string codigo, string nombre, string? descripcion = null)
    {
        if (string.IsNullOrWhiteSpace(codigo))
        {
            throw new ExcepcionDominio("El código del rol es obligatorio.");
        }

        if (string.IsNullOrWhiteSpace(nombre))
        {
            throw new ExcepcionDominio("El nombre del rol es obligatorio.");
        }

        var descripcionNormalizada = string.IsNullOrWhiteSpace(descripcion) ? null : descripcion.Trim();

        return new Rol(Guid.NewGuid(), codigo.Trim().ToUpperInvariant(), nombre.Trim(), descripcionNormalizada);
    }

    public void Activar() => Estado = EstadoRol.Activo;

    public void Desactivar() => Estado = EstadoRol.Inactivo;

    public void AsignarPermiso(Guid permisoId)
    {
        if (_permisos.Any(p => p.PermisoId == permisoId))
        {
            throw new ExcepcionDominio("El rol ya tiene asignado ese permiso.");
        }

        _permisos.Add(new RolPermiso(Id, permisoId));
    }

    public void QuitarPermiso(Guid permisoId)
    {
        var existente = _permisos.FirstOrDefault(p => p.PermisoId == permisoId);

        if (existente is null)
        {
            throw new ExcepcionDominio("El rol no tiene asignado ese permiso.");
        }

        _permisos.Remove(existente);
    }
}
