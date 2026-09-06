namespace SaaS.Veterinario.Infrastructure.Identidad;

/// <summary>
/// Configuracion tipada de la seccion "Jwt". SigningKey nunca esta en el repositorio en
/// texto real: en Development sale de User Secrets/variable de entorno; en produccion,
/// de variable de entorno (y mas adelante, Azure Key Vault).
/// </summary>
public sealed class JwtOptions
{
    public const string Seccion = "Jwt";

    public required string Issuer { get; init; }

    public required string Audience { get; init; }

    public required string SigningKey { get; init; }

    public int AccessTokenMinutes { get; init; } = 15;

    public int RefreshTokenDays { get; init; } = 30;
}
