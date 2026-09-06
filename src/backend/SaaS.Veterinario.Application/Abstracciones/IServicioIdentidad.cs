namespace SaaS.Veterinario.Application.Abstracciones;

/// <summary>
/// Abstrae ASP.NET Core Identity para que los casos de uso de Application no dependan de
/// UserManager/SignInManager directamente. La implementacion real vive en Infrastructure.
/// </summary>
public interface IServicioIdentidad
{
    Task<bool> ExisteCorreoAsync(string correo, CancellationToken cancellationToken);

    Task<ResultadoIdentidad> CrearAsync(Guid usuarioId, string correo, string password, CancellationToken cancellationToken);

    Task<ResultadoInicioSesion> ValidarCredencialesAsync(string correo, string password, CancellationToken cancellationToken);

    Task<string> GenerarTokenConfirmacionCorreoAsync(Guid usuarioId, CancellationToken cancellationToken);

    Task<ResultadoIdentidad> ConfirmarCorreoAsync(Guid usuarioId, string token, CancellationToken cancellationToken);

    Task<Guid?> ObtenerUsuarioIdPorCorreoAsync(string correo, CancellationToken cancellationToken);

    Task<string> GenerarTokenRecuperacionPasswordAsync(Guid usuarioId, CancellationToken cancellationToken);

    Task<ResultadoIdentidad> RestablecerPasswordAsync(Guid usuarioId, string token, string nuevoPassword, CancellationToken cancellationToken);
}

public sealed record ResultadoIdentidad(bool Exitoso, IReadOnlyCollection<string> Errores)
{
    public static ResultadoIdentidad Ok() => new(true, []);

    public static ResultadoIdentidad Fallo(IEnumerable<string> errores) => new(false, errores.ToArray());
}

public enum EstadoInicioSesion
{
    Exitoso,
    CredencialesInvalidas,
    Bloqueado,
}

public sealed record ResultadoInicioSesion(EstadoInicioSesion Estado, Guid? UsuarioId);
