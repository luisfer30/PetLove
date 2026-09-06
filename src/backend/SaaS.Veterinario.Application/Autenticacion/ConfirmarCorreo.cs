using SaaS.Veterinario.Application.Abstracciones;
using SaaS.Veterinario.Application.Errores;

namespace SaaS.Veterinario.Application.Autenticacion;

public sealed record ConfirmarCorreoComando(Guid UsuarioId, string Token);

/// <summary>
/// Cuando Identity valida el token de confirmacion, se ejecutan en la misma operacion las
/// dos transiciones de dominio: ConfirmarCorreo() y luego Activar() (PENDIENTE_VERIFICACION
/// -> ACTIVO), tal como se definio para esta V1.
/// </summary>
public sealed class ConfirmarCorreo(IServicioIdentidad servicioIdentidad, IRepositorioUsuarios repositorioUsuarios)
{
    public async Task EjecutarAsync(ConfirmarCorreoComando comando, CancellationToken cancellationToken)
    {
        var resultado = await servicioIdentidad.ConfirmarCorreoAsync(comando.UsuarioId, comando.Token, cancellationToken);

        if (!resultado.Exitoso)
        {
            throw new ExcepcionAplicacion(CodigosError.TokenInvalido, "El token de confirmación de correo no es válido o expiró.");
        }

        var usuario = await repositorioUsuarios.ObtenerPorIdAsync(comando.UsuarioId, cancellationToken)
            ?? throw new ExcepcionAplicacion(CodigosError.TokenInvalido, "El token de confirmación de correo no es válido o expiró.");

        usuario.ConfirmarCorreo();
        usuario.Activar(DateTimeOffset.UtcNow);

        await repositorioUsuarios.GuardarCambiosAsync(cancellationToken);
    }
}
