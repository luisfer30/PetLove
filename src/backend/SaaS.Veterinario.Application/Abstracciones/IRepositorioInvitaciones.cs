namespace SaaS.Veterinario.Application.Abstracciones;

public interface IRepositorioInvitaciones
{
    /// <summary>
    /// Solo cuenta invitaciones en estado PENDIENTE: una invitacion ACEPTADA, EXPIRADA o
    /// CANCELADA no bloquea una nueva. Es la primera barrera (comprobacion explicita antes de
    /// escribir); el indice unico parcial en BD es la segunda, solo para condiciones de
    /// carrera -- nunca se captura su violacion como mecanismo normal de negocio.
    /// </summary>
    Task<bool> ExisteInvitacionPendienteAsync(Guid veterinariaId, string correo, CancellationToken cancellationToken);
}
