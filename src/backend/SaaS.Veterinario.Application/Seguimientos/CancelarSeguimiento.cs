using SaaS.Veterinario.Application.Abstracciones;
using SaaS.Veterinario.Application.Errores;
using SaaS.Veterinario.Domain.Errores;

namespace SaaS.Veterinario.Application.Seguimientos;

public sealed class CancelarSeguimiento(IContextoVeterinaria contexto, IRepositorioSeguimientos repositorioSeguimientos)
{
    public async Task EjecutarAsync(Guid seguimientoId, CancellationToken cancellationToken)
    {
        var veterinariaId = contexto.VeterinariaId
            ?? throw new InvalidOperationException("Se invocó CancelarSeguimiento sin contexto veterinario resuelto.");

        var seguimiento = await repositorioSeguimientos.ObtenerPorIdAsync(seguimientoId, veterinariaId, cancellationToken)
            ?? throw new ExcepcionAplicacion(CodigosError.SeguimientoNoEncontrado, "El seguimiento no existe en esta veterinaria.");

        try
        {
            seguimiento.Cancelar(contexto.UsuarioId, DateTimeOffset.UtcNow);
        }
        catch (ExcepcionDominio excepcion)
        {
            throw new ExcepcionAplicacion(CodigosError.SeguimientoEstadoInvalido, excepcion.Message);
        }

        await repositorioSeguimientos.GuardarCambiosAsync(cancellationToken);
    }
}
