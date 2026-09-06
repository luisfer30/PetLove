using SaaS.Veterinario.Application.Abstracciones;
using SaaS.Veterinario.Application.Clinico;
using SaaS.Veterinario.Application.Errores;
using SaaS.Veterinario.Domain.Citas;

namespace SaaS.Veterinario.Application.Citas;

public sealed record ActualizarCitaComando(
    Guid CitaId,
    DateTimeOffset FechaHoraInicio,
    DateTimeOffset FechaHoraFin,
    string Motivo,
    string? Observaciones,
    Guid? VeterinarioUsuarioId);

/// <summary>
/// Solo permite modificar mientras la cita este PROGRAMADA o CONFIRMADA (seccion 8); nunca
/// permite cambiar VeterinariaId, MascotaVeterinariaId ni el Estado de forma arbitraria -- eso
/// requiere los endpoints de transicion dedicados.
/// </summary>
public sealed class ActualizarCita(
    IContextoVeterinaria contexto,
    IRepositorioCitas repositorioCitas,
    IRepositorioMembresias repositorioMembresias)
{
    public async Task EjecutarAsync(ActualizarCitaComando comando, CancellationToken cancellationToken)
    {
        var veterinariaId = contexto.VeterinariaId
            ?? throw new InvalidOperationException("Se invocó ActualizarCita sin contexto veterinario resuelto.");

        var cita = await repositorioCitas.ObtenerPorIdAsync(comando.CitaId, veterinariaId, cancellationToken)
            ?? throw new ExcepcionAplicacion(CodigosError.CitaNoEncontrada, "La cita no existe en esta veterinaria.");

        if (cita.Estado is not (EstadoCita.Programada or EstadoCita.Confirmada))
        {
            throw new ExcepcionAplicacion(CodigosError.CitaEstadoInvalido, "Solo se pueden modificar citas programadas o confirmadas.");
        }

        if (comando.VeterinarioUsuarioId is { } veterinarioId)
        {
            await ValidadorVeterinarioClinico.ValidarAsync(
                repositorioMembresias, veterinarioId, veterinariaId, CodigosError.CitaVeterinarioInvalido, cancellationToken);

            if (await repositorioCitas.ExisteSolapamientoAsync(
                    veterinariaId, veterinarioId, comando.FechaHoraInicio, comando.FechaHoraFin, cita.Id, cancellationToken))
            {
                throw new ExcepcionAplicacion(CodigosError.CitaHorarioNoDisponible, "El veterinario ya tiene una cita en ese horario.");
            }
        }

        cita.ActualizarDatos(
            comando.FechaHoraInicio,
            comando.FechaHoraFin,
            comando.Motivo,
            comando.Observaciones,
            comando.VeterinarioUsuarioId,
            contexto.UsuarioId,
            DateTimeOffset.UtcNow);

        await repositorioCitas.GuardarCambiosAsync(cancellationToken);
    }
}
