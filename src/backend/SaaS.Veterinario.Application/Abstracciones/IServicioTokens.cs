namespace SaaS.Veterinario.Application.Abstracciones;

/// <summary>
/// Abstrae la generacion/rotacion/revocacion de Access y Refresh Tokens. La implementacion
/// (Infrastructure) conoce JWT y persiste RefreshToken; Application solo ve estos contratos.
/// </summary>
public interface IServicioTokens
{
    Task<ParTokens> GenerarParAsync(
        Guid usuarioId,
        string correo,
        string? dispositivo,
        string? direccionIp,
        CancellationToken cancellationToken);

    Task<ResultadoRenovacion> RenovarAsync(
        string refreshTokenPlano,
        string? dispositivo,
        string? direccionIp,
        CancellationToken cancellationToken);

    Task RevocarAsync(string refreshTokenPlano, CancellationToken cancellationToken);

    Task RevocarTodasLasSesionesAsync(Guid usuarioId, CancellationToken cancellationToken);
}

public sealed record ParTokens(
    string AccessToken,
    string RefreshToken,
    DateTimeOffset AccessTokenExpiraEn,
    DateTimeOffset RefreshTokenExpiraEn);

public enum EstadoRenovacion
{
    Exitoso,
    Invalido,
    Expirado,
    Revocado,
}

public sealed record ResultadoRenovacion(EstadoRenovacion Estado, ParTokens? Tokens);
