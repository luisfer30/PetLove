using SaaS.Veterinario.Application.Abstracciones;
using SaaS.Veterinario.Application.Errores;
using SaaS.Veterinario.Domain.Usuarios;

namespace SaaS.Veterinario.Application.Autenticacion;

public sealed record IniciarSesionComando(string Correo, string Password, string? Dispositivo, string? DireccionIp);

/// <summary>
/// Valida credenciales via Identity (que ademas maneja el lockout por intentos fallidos) y,
/// si son correctas, exige que el Usuario de dominio este ACTIVO antes de emitir tokens.
/// </summary>
public sealed class IniciarSesion(
    IServicioIdentidad servicioIdentidad,
    IRepositorioUsuarios repositorioUsuarios,
    IServicioTokens servicioTokens)
{
    public async Task<ParTokens> EjecutarAsync(IniciarSesionComando comando, CancellationToken cancellationToken)
    {
        var resultado = await servicioIdentidad.ValidarCredencialesAsync(comando.Correo, comando.Password, cancellationToken);

        if (resultado.Estado == EstadoInicioSesion.Bloqueado)
        {
            throw new ExcepcionAplicacion(CodigosError.UsuarioBloqueado, "La cuenta esta bloqueada temporalmente por intentos fallidos.");
        }

        if (resultado.Estado == EstadoInicioSesion.CredencialesInvalidas || resultado.UsuarioId is null)
        {
            throw new ExcepcionAplicacion(CodigosError.CredencialesInvalidas, "El correo o la contraseña son incorrectos.");
        }

        var usuario = await repositorioUsuarios.ObtenerPorIdAsync(resultado.UsuarioId.Value, cancellationToken)
            ?? throw new ExcepcionAplicacion(CodigosError.CredencialesInvalidas, "El correo o la contraseña son incorrectos.");

        switch (usuario.Estado)
        {
            case EstadoUsuario.PendienteVerificacion:
                throw new ExcepcionAplicacion(CodigosError.CorreoNoVerificado, "Debes confirmar tu correo antes de iniciar sesión.");
            case EstadoUsuario.Bloqueado:
            case EstadoUsuario.Suspendido:
                throw new ExcepcionAplicacion(CodigosError.UsuarioBloqueado, "La cuenta no esta activa.");
        }

        return await servicioTokens.GenerarParAsync(
            usuario.Id,
            usuario.Correo.Valor,
            comando.Dispositivo,
            comando.DireccionIp,
            cancellationToken);
    }
}
