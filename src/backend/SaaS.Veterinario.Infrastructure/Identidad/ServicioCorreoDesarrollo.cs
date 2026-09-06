using Microsoft.Extensions.Logging;
using SaaS.Veterinario.Application.Abstracciones;

namespace SaaS.Veterinario.Infrastructure.Identidad;

/// <summary>
/// Implementacion de desarrollo de IServicioCorreo: no envia nada, solo deja constancia en
/// logs de que la operacion ocurrio. Nunca registra el token completo (seguridad de logs).
/// Un proveedor real (SMTP/SendGrid/etc.) se conectara en una etapa posterior detras de la
/// misma interfaz, sin tocar los casos de uso de Application.
/// </summary>
public sealed class ServicioCorreoDesarrollo(ILogger<ServicioCorreoDesarrollo> logger) : IServicioCorreo
{
    public Task EnviarConfirmacionCorreoAsync(string correo, string token, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "[DEV] Correo de confirmacion generado para {Correo} (token omitido de logs).",
            correo);

        return Task.CompletedTask;
    }

    public Task EnviarRecuperacionPasswordAsync(string correo, string token, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "[DEV] Correo de recuperacion de contraseña generado para {Correo} (token omitido de logs).",
            correo);

        return Task.CompletedTask;
    }
}
