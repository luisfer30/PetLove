using SaaS.Veterinario.Application.Abstracciones;
using SaaS.Veterinario.Application.Errores;

namespace SaaS.Veterinario.Application.Autenticacion;

public sealed record RenovarTokenComando(string RefreshToken, string? Dispositivo, string? DireccionIp);

/// <summary>
/// Rotacion: un Refresh Token valido se revoca y se emite un par nuevo. Ver
/// IServicioTokens.RenovarAsync (Infrastructure) para el detalle de la rotacion.
/// </summary>
public sealed class RenovarToken(IServicioTokens servicioTokens)
{
    public async Task<ParTokens> EjecutarAsync(RenovarTokenComando comando, CancellationToken cancellationToken)
    {
        var resultado = await servicioTokens.RenovarAsync(
            comando.RefreshToken,
            comando.Dispositivo,
            comando.DireccionIp,
            cancellationToken);

        return resultado.Estado switch
        {
            EstadoRenovacion.Exitoso => resultado.Tokens!,
            EstadoRenovacion.Expirado => throw new ExcepcionAplicacion(CodigosError.RefreshTokenExpirado, "El refresh token expiró."),
            EstadoRenovacion.Revocado => throw new ExcepcionAplicacion(CodigosError.RefreshTokenRevocado, "El refresh token ya fue revocado."),
            _ => throw new ExcepcionAplicacion(CodigosError.RefreshTokenInvalido, "El refresh token no es válido."),
        };
    }
}
