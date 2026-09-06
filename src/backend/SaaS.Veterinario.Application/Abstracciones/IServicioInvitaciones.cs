namespace SaaS.Veterinario.Application.Abstracciones;

/// <summary>
/// Crea y persiste una InvitacionPersonal, incluyendo la generacion criptografica del token
/// (Application no debe generar aleatoriedad de seguridad, igual que con RefreshToken).
/// </summary>
public interface IServicioInvitaciones
{
    Task<InvitacionCreada> CrearAsync(
        Guid veterinariaId,
        Guid creadoPorUsuarioId,
        string correo,
        IReadOnlyCollection<Guid> rolIds,
        CancellationToken cancellationToken);
}

public sealed record InvitacionCreada(Guid InvitacionId, string TokenPlano);
