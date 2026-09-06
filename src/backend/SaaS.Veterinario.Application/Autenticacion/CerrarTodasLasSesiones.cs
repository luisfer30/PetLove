using SaaS.Veterinario.Application.Abstracciones;

namespace SaaS.Veterinario.Application.Autenticacion;

public sealed record CerrarTodasLasSesionesComando(Guid UsuarioId);

/// <summary>
/// Revoca todos los Refresh Tokens activos del usuario autenticado (requiere JWT valido,
/// ver el endpoint correspondiente).
/// </summary>
public sealed class CerrarTodasLasSesiones(IServicioTokens servicioTokens)
{
    public Task EjecutarAsync(CerrarTodasLasSesionesComando comando, CancellationToken cancellationToken)
        => servicioTokens.RevocarTodasLasSesionesAsync(comando.UsuarioId, cancellationToken);
}
