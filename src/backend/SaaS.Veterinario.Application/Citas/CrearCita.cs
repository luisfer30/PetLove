using SaaS.Veterinario.Application.Abstracciones;
using SaaS.Veterinario.Application.Clinico;
using SaaS.Veterinario.Application.Errores;
using SaaS.Veterinario.Domain.Citas;

namespace SaaS.Veterinario.Application.Citas;

public sealed record CrearCitaComando(
    Guid MascotaVeterinariaId,
    Guid? ClienteVeterinariaId,
    Guid? VeterinarioUsuarioId,
    DateTimeOffset FechaHoraInicio,
    DateTimeOffset FechaHoraFin,
    string Motivo,
    string? Observaciones);

public sealed record CrearCitaResultado(Guid CitaId, EstadoCita Estado);

public sealed class CrearCita(
    IContextoVeterinaria contexto,
    IRepositorioCitas repositorioCitas,
    IRepositorioMascotasVeterinarias repositorioMascotasVeterinarias,
    IRepositorioClientes repositorioClientes,
    IRepositorioMembresias repositorioMembresias)
{
    public async Task<CrearCitaResultado> EjecutarAsync(CrearCitaComando comando, CancellationToken cancellationToken)
    {
        var veterinariaId = contexto.VeterinariaId
            ?? throw new InvalidOperationException("Se invocó CrearCita sin contexto veterinario resuelto.");

        _ = await repositorioMascotasVeterinarias.ObtenerPorIdAsync(comando.MascotaVeterinariaId, veterinariaId, cancellationToken)
            ?? throw new ExcepcionAplicacion(CodigosError.MascotaVeterinariaNoEncontrada, "La ficha de la mascota no existe en esta veterinaria.");

        if (comando.ClienteVeterinariaId is { } clienteId)
        {
            _ = await repositorioClientes.ObtenerPorIdAsync(clienteId, veterinariaId, cancellationToken)
                ?? throw new ExcepcionAplicacion(CodigosError.ClienteInvalido, "El cliente indicado no existe en esta veterinaria.");
        }

        if (comando.VeterinarioUsuarioId is { } veterinarioId)
        {
            await ValidadorVeterinarioClinico.ValidarAsync(
                repositorioMembresias, veterinarioId, veterinariaId, CodigosError.CitaVeterinarioInvalido, cancellationToken);

            if (await repositorioCitas.ExisteSolapamientoAsync(
                    veterinariaId, veterinarioId, comando.FechaHoraInicio, comando.FechaHoraFin, excluirCitaId: null, cancellationToken))
            {
                throw new ExcepcionAplicacion(CodigosError.CitaHorarioNoDisponible, "El veterinario ya tiene una cita en ese horario.");
            }
        }

        var cita = Cita.Crear(
            veterinariaId,
            comando.MascotaVeterinariaId,
            comando.ClienteVeterinariaId,
            comando.VeterinarioUsuarioId,
            comando.FechaHoraInicio,
            comando.FechaHoraFin,
            comando.Motivo,
            comando.Observaciones,
            contexto.UsuarioId);

        await repositorioCitas.AgregarAsync(cita, cancellationToken);

        return new CrearCitaResultado(cita.Id, cita.Estado);
    }
}
