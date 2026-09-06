using SaaS.Veterinario.Application.Abstracciones;

namespace SaaS.Veterinario.Application.Autenticacion;

public sealed record CerrarSesionComando(string RefreshToken);

/// <summary>
/// Revoca el Refresh Token indicado. Es intencionalmente silenciosa si el token no existe
/// o ya estaba revocado: cerrar sesion nunca debe fallar de forma visible para el cliente.
/// No es necesario invalidar el Access Token de corta duracion ya emitido.
/// </summary>
public sealed class CerrarSesion(IServicioTokens servicioTokens)
{
    public Task EjecutarAsync(CerrarSesionComando comando, CancellationToken cancellationToken)
        => servicioTokens.RevocarAsync(comando.RefreshToken, cancellationToken);
}
