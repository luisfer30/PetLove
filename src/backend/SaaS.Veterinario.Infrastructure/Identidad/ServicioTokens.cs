using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SaaS.Veterinario.Application.Abstracciones;
using SaaS.Veterinario.Infrastructure.Persistencia;

namespace SaaS.Veterinario.Infrastructure.Identidad;

/// <summary>
/// Genera Access Tokens JWT (HMAC SHA-256) y administra el ciclo de vida de los Refresh
/// Tokens: nunca se persiste el valor en texto plano (solo su hash SHA-256), y cada uso
/// valido rota el token (el anterior queda revocado, se emite uno nuevo).
/// </summary>
public sealed class ServicioTokens(SaaSVeterinarioDbContext contexto, IOptions<JwtOptions> opciones) : IServicioTokens
{
    private readonly JwtOptions _opciones = opciones.Value;

    public async Task<ParTokens> GenerarParAsync(
        Guid usuarioId,
        string correo,
        string? dispositivo,
        string? direccionIp,
        CancellationToken cancellationToken)
    {
        var ahora = DateTimeOffset.UtcNow;
        var accessToken = GenerarAccessToken(usuarioId, correo, ahora, out var expiraAccess);
        var (refreshTokenPlano, refreshToken) = CrearRefreshToken(usuarioId, ahora, dispositivo, direccionIp);

        contexto.Add(refreshToken);
        await contexto.SaveChangesAsync(cancellationToken);

        return new ParTokens(accessToken, refreshTokenPlano, expiraAccess, refreshToken.FechaExpiracion);
    }

    public async Task<ResultadoRenovacion> RenovarAsync(
        string refreshTokenPlano,
        string? dispositivo,
        string? direccionIp,
        CancellationToken cancellationToken)
    {
        var hash = Hashear(refreshTokenPlano);
        var existente = await contexto.Set<RefreshToken>().FirstOrDefaultAsync(t => t.TokenHash == hash, cancellationToken);

        if (existente is null)
        {
            return new ResultadoRenovacion(EstadoRenovacion.Invalido, null);
        }

        if (existente.EstaRevocado)
        {
            return new ResultadoRenovacion(EstadoRenovacion.Revocado, null);
        }

        var ahora = DateTimeOffset.UtcNow;

        if (existente.EstaExpirado(ahora))
        {
            return new ResultadoRenovacion(EstadoRenovacion.Expirado, null);
        }

        var usuario = await contexto.Usuarios.FirstAsync(u => u.Id == existente.UsuarioId, cancellationToken);

        // Rotacion: el token usado se revoca y nunca se reutiliza.
        existente.Revocar(ahora);

        var accessToken = GenerarAccessToken(existente.UsuarioId, usuario.Correo.Valor, ahora, out var expiraAccess);
        var (nuevoRefreshPlano, nuevoRefreshToken) = CrearRefreshToken(existente.UsuarioId, ahora, dispositivo, direccionIp);

        contexto.Add(nuevoRefreshToken);
        await contexto.SaveChangesAsync(cancellationToken);

        return new ResultadoRenovacion(
            EstadoRenovacion.Exitoso,
            new ParTokens(accessToken, nuevoRefreshPlano, expiraAccess, nuevoRefreshToken.FechaExpiracion));
    }

    public async Task RevocarAsync(string refreshTokenPlano, CancellationToken cancellationToken)
    {
        var hash = Hashear(refreshTokenPlano);
        var existente = await contexto.Set<RefreshToken>().FirstOrDefaultAsync(t => t.TokenHash == hash, cancellationToken);

        if (existente is null || existente.EstaRevocado)
        {
            return;
        }

        existente.Revocar(DateTimeOffset.UtcNow);
        await contexto.SaveChangesAsync(cancellationToken);
    }

    public async Task RevocarTodasLasSesionesAsync(Guid usuarioId, CancellationToken cancellationToken)
    {
        var ahora = DateTimeOffset.UtcNow;
        var activos = await contexto.Set<RefreshToken>()
            .Where(t => t.UsuarioId == usuarioId && t.FechaRevocacion == null)
            .ToListAsync(cancellationToken);

        foreach (var token in activos)
        {
            token.Revocar(ahora);
        }

        await contexto.SaveChangesAsync(cancellationToken);
    }

    private string GenerarAccessToken(Guid usuarioId, string correo, DateTimeOffset ahora, out DateTimeOffset expiraEn)
    {
        expiraEn = ahora.AddMinutes(_opciones.AccessTokenMinutes);

        // Claims minimos a proposito: sin VeterinariaId, roles ni permisos (se resuelven
        // del lado servidor en etapas posteriores, nunca se confia en lo que trae el JWT).
        Claim[] claims =
        [
            new(JwtRegisteredClaimNames.Sub, usuarioId.ToString()),
            new(JwtRegisteredClaimNames.Email, correo),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        ];

        var credenciales = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_opciones.SigningKey)),
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _opciones.Issuer,
            audience: _opciones.Audience,
            claims: claims,
            notBefore: ahora.UtcDateTime,
            expires: expiraEn.UtcDateTime,
            signingCredentials: credenciales);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private (string Plano, RefreshToken Entidad) CrearRefreshToken(
        Guid usuarioId,
        DateTimeOffset ahora,
        string? dispositivo,
        string? direccionIp)
    {
        var plano = GenerarValorAleatorioSeguro();
        var hash = Hashear(plano);
        var expiracion = ahora.AddDays(_opciones.RefreshTokenDays);

        var entidad = new RefreshToken(usuarioId, hash, ahora, expiracion, dispositivo, direccionIp);

        return (plano, entidad);
    }

    private static string GenerarValorAleatorioSeguro()
    {
        var bytes = RandomNumberGenerator.GetBytes(64);
        return Convert.ToBase64String(bytes);
    }

    private static string Hashear(string valor)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(valor));
        return Convert.ToHexString(bytes);
    }
}
