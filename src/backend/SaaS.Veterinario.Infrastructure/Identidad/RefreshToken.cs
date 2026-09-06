namespace SaaS.Veterinario.Infrastructure.Identidad;

/// <summary>
/// Entidad de infraestructura (no de dominio): un Refresh Token nunca se persiste en texto
/// plano, solo su hash. El valor real solo existe brevemente en memoria al generarlo y al
/// validarlo (ver ServicioTokens), nunca se registra en logs.
/// </summary>
public sealed class RefreshToken
{
    public Guid Id { get; private set; }

    public Guid UsuarioId { get; private set; }

    public string TokenHash { get; private set; }

    public DateTimeOffset FechaCreacion { get; private set; }

    public DateTimeOffset FechaExpiracion { get; private set; }

    public DateTimeOffset? FechaRevocacion { get; private set; }

    public string? Dispositivo { get; private set; }

    public string? DireccionIp { get; private set; }

    public RefreshToken(
        Guid usuarioId,
        string tokenHash,
        DateTimeOffset fechaCreacion,
        DateTimeOffset fechaExpiracion,
        string? dispositivo,
        string? direccionIp)
    {
        Id = Guid.NewGuid();
        UsuarioId = usuarioId;
        TokenHash = tokenHash;
        FechaCreacion = fechaCreacion;
        FechaExpiracion = fechaExpiracion;
        Dispositivo = dispositivo;
        DireccionIp = direccionIp;
    }

    public bool EstaRevocado => FechaRevocacion is not null;

    public bool EstaExpirado(DateTimeOffset momento) => momento >= FechaExpiracion;

    public void Revocar(DateTimeOffset momento)
    {
        FechaRevocacion ??= momento;
    }
}
