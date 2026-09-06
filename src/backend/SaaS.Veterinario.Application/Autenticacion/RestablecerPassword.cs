using SaaS.Veterinario.Application.Abstracciones;
using SaaS.Veterinario.Application.Errores;

namespace SaaS.Veterinario.Application.Autenticacion;

public sealed record RestablecerPasswordComando(string Correo, string Token, string NuevoPassword);

public sealed class RestablecerPassword(IServicioIdentidad servicioIdentidad)
{
    public async Task EjecutarAsync(RestablecerPasswordComando comando, CancellationToken cancellationToken)
    {
        var usuarioId = await servicioIdentidad.ObtenerUsuarioIdPorCorreoAsync(comando.Correo, cancellationToken);

        if (usuarioId is null)
        {
            throw new ExcepcionAplicacion(CodigosError.TokenInvalido, "El token de recuperación no es válido o expiró.");
        }

        var resultado = await servicioIdentidad.RestablecerPasswordAsync(usuarioId.Value, comando.Token, comando.NuevoPassword, cancellationToken);

        if (!resultado.Exitoso)
        {
            throw new ExcepcionAplicacion(CodigosError.TokenInvalido, "El token de recuperación no es válido o expiró.");
        }
    }
}
