using SaaS.Veterinario.Application.Abstracciones;
using SaaS.Veterinario.Application.Clinico;
using SaaS.Veterinario.Application.Errores;
using SaaS.Veterinario.Domain.Citas;
using SaaS.Veterinario.Domain.Consultas;

namespace SaaS.Veterinario.Application.Consultas;

public sealed record CrearConsultaComando(
    Guid MascotaVeterinariaId,
    Guid? CitaId,
    Guid VeterinarioUsuarioId,
    DateTimeOffset FechaHora,
    string MotivoConsulta,
    decimal? Peso,
    decimal? Temperatura,
    int? FrecuenciaCardiaca,
    string? ObservacionesClinicas,
    string? IndicacionesPropietario,
    DateTimeOffset? ProximaFechaControl,
    string? MotivoProximoControl);

public sealed record CrearConsultaResultado(Guid ConsultaId, EstadoConsulta Estado);

/// <summary>
/// Una Consulta puede existir sin Cita previa (R03, ej. emergencia). Si CitaId se especifica,
/// no se fuerza su estado a ATENDIDA aqui todavia (seccion 14) -- eso ocurre automaticamente
/// al FINALIZAR la consulta (ver FinalizarConsulta).
/// </summary>
public sealed class CrearConsulta(
    IContextoVeterinaria contexto,
    IRepositorioConsultas repositorioConsultas,
    IRepositorioMascotasVeterinarias repositorioMascotasVeterinarias,
    IRepositorioCitas repositorioCitas,
    IRepositorioMembresias repositorioMembresias)
{
    public async Task<CrearConsultaResultado> EjecutarAsync(CrearConsultaComando comando, CancellationToken cancellationToken)
    {
        var veterinariaId = contexto.VeterinariaId
            ?? throw new InvalidOperationException("Se invocó CrearConsulta sin contexto veterinario resuelto.");

        _ = await repositorioMascotasVeterinarias.ObtenerPorIdAsync(comando.MascotaVeterinariaId, veterinariaId, cancellationToken)
            ?? throw new ExcepcionAplicacion(CodigosError.MascotaVeterinariaNoEncontrada, "La ficha de la mascota no existe en esta veterinaria.");

        await ValidadorVeterinarioClinico.ValidarAsync(
            repositorioMembresias, comando.VeterinarioUsuarioId, veterinariaId, CodigosError.ConsultaVeterinarioInvalido, cancellationToken);

        if (comando.CitaId is { } citaId)
        {
            var cita = await repositorioCitas.ObtenerPorIdAsync(citaId, veterinariaId, cancellationToken)
                ?? throw new ExcepcionAplicacion(CodigosError.ConsultaCitaInvalida, "La cita indicada no existe en esta veterinaria.");

            if (cita.MascotaVeterinariaId != comando.MascotaVeterinariaId)
            {
                throw new ExcepcionAplicacion(CodigosError.ConsultaCitaInvalida, "La cita indicada no corresponde a esta mascota.");
            }

            if (cita.Estado is EstadoCita.Cancelada or EstadoCita.NoAsistio)
            {
                throw new ExcepcionAplicacion(CodigosError.ConsultaCitaInvalida, "No se puede crear una consulta sobre una cita cancelada o no asistida.");
            }
        }

        var consulta = ConsultaVeterinaria.Crear(
            veterinariaId,
            comando.MascotaVeterinariaId,
            comando.CitaId,
            comando.VeterinarioUsuarioId,
            comando.FechaHora,
            comando.MotivoConsulta,
            comando.Peso,
            comando.Temperatura,
            comando.FrecuenciaCardiaca,
            comando.ObservacionesClinicas,
            comando.IndicacionesPropietario,
            comando.ProximaFechaControl,
            comando.MotivoProximoControl,
            contexto.UsuarioId);

        await repositorioConsultas.AgregarAsync(consulta, cancellationToken);

        return new CrearConsultaResultado(consulta.Id, consulta.Estado);
    }
}
