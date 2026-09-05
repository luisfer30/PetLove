using SaaS.Veterinario.Domain.Errores;

namespace SaaS.Veterinario.Domain.Membresias;

public sealed class MembresiaRol
{
    public Guid MembresiaId { get; }
    public Guid RolId { get; }

    internal MembresiaRol(Guid membresiaId, Guid rolId)
    {
        if (membresiaId == Guid.Empty)
        {
            throw new ExcepcionDominio("El identificador de la membresía es obligatorio.");
        }

        if (rolId == Guid.Empty)
        {
            throw new ExcepcionDominio("El identificador del rol es obligatorio.");
        }

        MembresiaId = membresiaId;
        RolId = rolId;
    }
}
