namespace SaaS.Veterinario.Application.Abstracciones;

/// <summary>
/// Envio de correo relacionado a autenticacion. En esta etapa no existe un proveedor real:
/// Infrastructure trae una implementacion de desarrollo que solo registra la operacion
/// (sin el token completo) en logs.
/// </summary>
public interface IServicioCorreo
{
    Task EnviarConfirmacionCorreoAsync(string correo, string token, CancellationToken cancellationToken);

    Task EnviarRecuperacionPasswordAsync(string correo, string token, CancellationToken cancellationToken);

    Task EnviarInvitacionPersonalAsync(string correo, string token, CancellationToken cancellationToken);
}
