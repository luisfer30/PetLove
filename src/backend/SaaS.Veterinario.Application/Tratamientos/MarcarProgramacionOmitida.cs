using SaaS.Veterinario.Application.Abstracciones;
using SaaS.Veterinario.Application.Errores;
using SaaS.Veterinario.Domain.Errores;

namespace SaaS.Veterinario.Application.Tratamientos;

public sealed record MarcarProgramacionOmitidaComando(Guid ProgramacionId, string? Observacion);

/// <summary>No elimina la fila (seccion 19); solo cambia el estado y registra quien la marco.</summary>
public sealed class MarcarProgramacionOmitida(IContextoVeterinaria contexto, IRepositorioProgramacionesTratamiento repositorioProgramaciones)
{
    public async Task EjecutarAsync(MarcarProgramacionOmitidaComando comando, CancellationToken cancellationToken)
    {
        var veterinariaId = contexto.VeterinariaId
            ?? throw new InvalidOperationException("Se invocó MarcarProgramacionOmitida sin contexto veterinario resuelto.");

        var programacion = await repositorioProgramaciones.ObtenerPorIdAsync(comando.ProgramacionId, veterinariaId, cancellationToken)
            ?? throw new ExcepcionAplicacion(CodigosError.ProgramacionNoEncontrada, "La programación no existe en esta veterinaria.");

        try
        {
            programacion.MarcarOmitida(contexto.UsuarioId, comando.Observacion, DateTimeOffset.UtcNow);
        }
        catch (ExcepcionDominio excepcion)
        {
            throw new ExcepcionAplicacion(CodigosError.ProgramacionEstadoInvalido, excepcion.Message);
        }

        await repositorioProgramaciones.GuardarCambiosAsync(cancellationToken);
    }
}
