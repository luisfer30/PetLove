using SaaS.Veterinario.Application.Abstracciones;
using SaaS.Veterinario.Application.Errores;
using SaaS.Veterinario.Domain.Errores;

namespace SaaS.Veterinario.Application.Seguimientos;

public sealed record MarcarSeguimientoAgendadoComando(Guid SeguimientoId, Guid CitaId);

/// <summary>Solo si la cita pertenece al mismo tenant y a la misma MascotaVeterinaria (seccion 37).</summary>
public sealed class MarcarSeguimientoAgendado(
    IContextoVeterinaria contexto, IRepositorioSeguimientos repositorioSeguimientos, IRepositorioCitas repositorioCitas)
{
    public async Task EjecutarAsync(MarcarSeguimientoAgendadoComando comando, CancellationToken cancellationToken)
    {
        var veterinariaId = contexto.VeterinariaId
            ?? throw new InvalidOperationException("Se invocó MarcarSeguimientoAgendado sin contexto veterinario resuelto.");

        var seguimiento = await repositorioSeguimientos.ObtenerPorIdAsync(comando.SeguimientoId, veterinariaId, cancellationToken)
            ?? throw new ExcepcionAplicacion(CodigosError.SeguimientoNoEncontrado, "El seguimiento no existe en esta veterinaria.");

        var cita = await repositorioCitas.ObtenerPorIdAsync(comando.CitaId, veterinariaId, cancellationToken)
            ?? throw new ExcepcionAplicacion(CodigosError.SeguimientoCitaInvalida, "La cita indicada no existe en esta veterinaria.");

        if (cita.MascotaVeterinariaId != seguimiento.MascotaVeterinariaId)
        {
            throw new ExcepcionAplicacion(CodigosError.SeguimientoCitaInvalida, "La cita indicada no corresponde a la mascota del seguimiento.");
        }

        try
        {
            seguimiento.MarcarAgendado(comando.CitaId, cita.FechaHoraInicio, contexto.UsuarioId, DateTimeOffset.UtcNow);
        }
        catch (ExcepcionDominio excepcion)
        {
            throw new ExcepcionAplicacion(CodigosError.SeguimientoEstadoInvalido, excepcion.Message);
        }

        await repositorioSeguimientos.GuardarCambiosAsync(cancellationToken);
    }
}
