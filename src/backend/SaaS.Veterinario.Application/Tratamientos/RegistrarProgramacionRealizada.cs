using SaaS.Veterinario.Application.Abstracciones;
using SaaS.Veterinario.Application.Errores;
using SaaS.Veterinario.Domain.Errores;

namespace SaaS.Veterinario.Application.Tratamientos;

public sealed record RegistrarProgramacionRealizadaComando(Guid ProgramacionId, string? Observacion);

/// <summary>
/// Puede registrarlo el personal veterinario (seccion 18) -- el portal propietario llegara en
/// una etapa futura; por ahora el endpoint siempre exige contexto veterinario.
/// </summary>
public sealed class RegistrarProgramacionRealizada(IContextoVeterinaria contexto, IRepositorioProgramacionesTratamiento repositorioProgramaciones)
{
    public async Task EjecutarAsync(RegistrarProgramacionRealizadaComando comando, CancellationToken cancellationToken)
    {
        var veterinariaId = contexto.VeterinariaId
            ?? throw new InvalidOperationException("Se invocó RegistrarProgramacionRealizada sin contexto veterinario resuelto.");

        var programacion = await repositorioProgramaciones.ObtenerPorIdAsync(comando.ProgramacionId, veterinariaId, cancellationToken)
            ?? throw new ExcepcionAplicacion(CodigosError.ProgramacionNoEncontrada, "La programación no existe en esta veterinaria.");

        try
        {
            programacion.MarcarRealizada(contexto.UsuarioId, comando.Observacion, DateTimeOffset.UtcNow);
        }
        catch (ExcepcionDominio excepcion)
        {
            throw new ExcepcionAplicacion(CodigosError.ProgramacionEstadoInvalido, excepcion.Message);
        }

        await repositorioProgramaciones.GuardarCambiosAsync(cancellationToken);
    }
}
