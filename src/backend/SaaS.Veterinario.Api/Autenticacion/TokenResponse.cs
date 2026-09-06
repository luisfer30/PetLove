using SaaS.Veterinario.Application.Abstracciones;

namespace SaaS.Veterinario.Api.Autenticacion;

public sealed record TokenResponse(
    string AccessToken,
    string RefreshToken,
    DateTimeOffset AccessTokenExpiraEn,
    DateTimeOffset RefreshTokenExpiraEn)
{
    public static TokenResponse DeDominio(ParTokens tokens)
        => new(tokens.AccessToken, tokens.RefreshToken, tokens.AccessTokenExpiraEn, tokens.RefreshTokenExpiraEn);
}
