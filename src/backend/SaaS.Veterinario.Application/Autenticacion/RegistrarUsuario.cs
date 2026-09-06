using SaaS.Veterinario.Application.Abstracciones;
using SaaS.Veterinario.Application.Errores;
using SaaS.Veterinario.Domain.Usuarios;

namespace SaaS.Veterinario.Application.Autenticacion;

public sealed record RegistrarUsuarioComando(string Nombre, string Correo, string? Telefono, string Password);

public sealed record RegistrarUsuarioResultado(Guid UsuarioId, string Correo);

/// <summary>
/// Alta de cuenta: crea el Usuario de dominio (PENDIENTE_VERIFICACION) y su identidad de
/// autenticacion dentro de UNA transaccion. Si cualquiera de los dos pasos falla, no debe
/// quedar ni Usuario ni identidad huerfanos -- ver IUnidadDeTrabajo. Al terminar, genera el
/// token de confirmacion de correo y lo envia (en Development, IServicioCorreo solo lo
/// registra en logs) -- eso queda fuera de la transaccion porque enviar el correo nunca
/// debe revertir el alta si falla.
/// </summary>
public sealed class RegistrarUsuario(
    IRepositorioUsuarios repositorioUsuarios,
    IServicioIdentidad servicioIdentidad,
    IServicioCorreo servicioCorreo,
    IUnidadDeTrabajo unidadDeTrabajo)
{
    public async Task<RegistrarUsuarioResultado> EjecutarAsync(RegistrarUsuarioComando comando, CancellationToken cancellationToken)
    {
        // Usuario.Crear ya valida y normaliza el correo; se reutiliza para comparar contra
        // el correo global antes de tocar Identity.
        var usuario = Usuario.Crear(comando.Nombre, comando.Correo, comando.Telefono);

        if (await repositorioUsuarios.ExisteCorreoAsync(usuario.Correo.Valor, cancellationToken))
        {
            throw new ExcepcionAplicacion(CodigosError.CorreoExistente, "Ya existe una cuenta asociada a este correo.");
        }

        if (await servicioIdentidad.ExisteCorreoAsync(usuario.Correo.Valor, cancellationToken))
        {
            throw new ExcepcionAplicacion(CodigosError.CorreoExistente, "Ya existe una cuenta asociada a este correo.");
        }

        await unidadDeTrabajo.EjecutarEnTransaccionAsync(async ct =>
        {
            await repositorioUsuarios.AgregarAsync(usuario, ct);

            var resultadoIdentidad = await servicioIdentidad.CrearAsync(usuario.Id, usuario.Correo.Valor, comando.Password, ct);

            if (!resultadoIdentidad.Exitoso)
            {
                throw new ExcepcionAplicacion(
                    CodigosError.RegistroInvalido,
                    string.Join(" ", resultadoIdentidad.Errores));
            }
        }, cancellationToken);

        var tokenConfirmacion = await servicioIdentidad.GenerarTokenConfirmacionCorreoAsync(usuario.Id, cancellationToken);
        await servicioCorreo.EnviarConfirmacionCorreoAsync(usuario.Correo.Valor, tokenConfirmacion, cancellationToken);

        return new RegistrarUsuarioResultado(usuario.Id, usuario.Correo.Valor);
    }
}
