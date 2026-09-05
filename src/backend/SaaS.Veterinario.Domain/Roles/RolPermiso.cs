using SaaS.Veterinario.Domain.Errores;

namespace SaaS.Veterinario.Domain.Roles;

public sealed class RolPermiso
{
    public Guid RolId { get; }
    public Guid PermisoId { get; }

    internal RolPermiso(Guid rolId, Guid permisoId)
    {
        if (rolId == Guid.Empty)
        {
            throw new ExcepcionDominio("El identificador del rol es obligatorio.");
        }

        if (permisoId == Guid.Empty)
        {
            throw new ExcepcionDominio("El identificador del permiso es obligatorio.");
        }

        RolId = rolId;
        PermisoId = permisoId;
    }
}
