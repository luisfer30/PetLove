using SaaS.Veterinario.Application.Abstracciones;
using SaaS.Veterinario.Application.Errores;
using SaaS.Veterinario.Domain.Citas;
using SaaS.Veterinario.Domain.Errores;

namespace SaaS.Veterinario.Application.Citas;

/// <summary>
/// Agrupa las 5 transiciones de estado de Cita: son casos de uso triviales (cargar, invocar el
/// metodo de dominio correspondiente, guardar) que comparten exactamente las mismas
/// dependencias, por eso viven en una sola clase en vez de 5 casi identicas.
/// </summary>
public sealed class GestionarEstadoCita(IContextoVeterinaria contexto, IRepositorioCitas repositorioCitas)
{
    public Task ConfirmarAsync(Guid citaId, CancellationToken cancellationToken)
        => CambiarEstadoAsync(citaId, cita => cita.Confirmar, cancellationToken);

    public Task MarcarEnEsperaAsync(Guid citaId, CancellationToken cancellationToken)
        => CambiarEstadoAsync(citaId, cita => cita.MarcarEnEspera, cancellationToken);

    public Task MarcarAtendidaAsync(Guid citaId, CancellationToken cancellationToken)
        => CambiarEstadoAsync(citaId, cita => cita.MarcarAtendida, cancellationToken);

    public Task CancelarAsync(Guid citaId, CancellationToken cancellationToken)
        => CambiarEstadoAsync(citaId, cita => cita.Cancelar, cancellationToken);

    public Task MarcarNoAsistioAsync(Guid citaId, CancellationToken cancellationToken)
        => CambiarEstadoAsync(citaId, cita => cita.MarcarNoAsistio, cancellationToken);

    private async Task CambiarEstadoAsync(Guid citaId, Func<Cita, Action<Guid, DateTimeOffset>> transicion, CancellationToken cancellationToken)
    {
        var veterinariaId = contexto.VeterinariaId
            ?? throw new InvalidOperationException("Se invocó GestionarEstadoCita sin contexto veterinario resuelto.");

        var cita = await repositorioCitas.ObtenerPorIdAsync(citaId, veterinariaId, cancellationToken)
            ?? throw new ExcepcionAplicacion(CodigosError.CitaNoEncontrada, "La cita no existe en esta veterinaria.");

        try
        {
            transicion(cita)(contexto.UsuarioId, DateTimeOffset.UtcNow);
        }
        catch (ExcepcionDominio excepcion)
        {
            throw new ExcepcionAplicacion(CodigosError.CitaEstadoInvalido, excepcion.Message);
        }

        await repositorioCitas.GuardarCambiosAsync(cancellationToken);
    }
}
