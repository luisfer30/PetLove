using SaaS.Veterinario.Domain.Errores;

namespace SaaS.Veterinario.Domain.Personal;

public sealed class InvitacionPersonalRol
{
    public Guid InvitacionId { get; }
    public Guid RolId { get; }

    internal InvitacionPersonalRol(Guid invitacionId, Guid rolId)
    {
        if (invitacionId == Guid.Empty)
        {
            throw new ExcepcionDominio("El identificador de la invitación es obligatorio.");
        }

        if (rolId == Guid.Empty)
        {
            throw new ExcepcionDominio("El identificador del rol es obligatorio.");
        }

        InvitacionId = invitacionId;
        RolId = rolId;
    }
}
