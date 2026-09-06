using SaaS.Veterinario.Application.Abstracciones;

namespace SaaS.Veterinario.Application.Autenticacion;

public sealed record SolicitarRecuperacionPasswordComando(string Correo);

/// <summary>
/// Nunca revela si el correo existe: si no hay cuenta asociada, simplemente no se envia
/// nada y el caso de uso termina en silencio, igual que si hubiera existido.
/// </summary>
public sealed class SolicitarRecuperacionPassword(IServicioIdentidad servicioIdentidad, IServicioCorreo servicioCorreo)
{
    public async Task EjecutarAsync(SolicitarRecuperacionPasswordComando comando, CancellationToken cancellationToken)
    {
        var usuarioId = await servicioIdentidad.ObtenerUsuarioIdPorCorreoAsync(comando.Correo, cancellationToken);

        if (usuarioId is null)
        {
            return;
        }

        var token = await servicioIdentidad.GenerarTokenRecuperacionPasswordAsync(usuarioId.Value, cancellationToken);

        await servicioCorreo.EnviarRecuperacionPasswordAsync(comando.Correo, token, cancellationToken);
    }
}
