using SaaS.Veterinario.Domain.Tratamientos;

namespace SaaS.Veterinario.Application.Abstracciones;

public interface IRepositorioProgramacionesTratamiento
{
    Task<ProgramacionTratamiento?> ObtenerPorIdAsync(Guid id, Guid veterinariaId, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<ProgramacionTratamiento>> ListarPorPlanAsync(Guid planTratamientoId, Guid veterinariaId, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<ProgramacionTratamiento>> ListarPorMascotaVeterinariaAsync(Guid mascotaVeterinariaId, Guid veterinariaId, CancellationToken cancellationToken);

    /// <summary>Programaciones PENDIENTE de un plan completo -- usado al suspender/completar/cancelar (seccion 21).</summary>
    Task<IReadOnlyCollection<ProgramacionTratamiento>> ListarPendientesPorPlanAsync(Guid planTratamientoId, CancellationToken cancellationToken);

    /// <summary>
    /// Sin filtro de veterinaria a proposito: pensado para un futuro job/motor de recordatorios
    /// (Etapa 9) que procesa vencimientos de todas las veterinarias, no para un usuario
    /// autenticado en un tenant especifico (seccion 20 -- logica reusable, no ejecutada aun).
    /// </summary>
    Task<IReadOnlyCollection<ProgramacionTratamiento>> ListarPendientesVencidasAsync(DateTimeOffset fechaCorte, CancellationToken cancellationToken);

    Task<bool> ExisteEnFechaAsync(Guid itemTratamientoId, DateTimeOffset fechaHoraProgramada, CancellationToken cancellationToken);

    /// <summary>Conteo por estado evaluable para adherencia (seccion 38) -- se calcula bajo demanda, nunca se persiste el porcentaje.</summary>
    Task<ConteoProgramacionesPlan> ContarPorEstadoAsync(Guid planTratamientoId, Guid veterinariaId, CancellationToken cancellationToken);

    Task AgregarRangoAsync(IReadOnlyCollection<ProgramacionTratamiento> programaciones, CancellationToken cancellationToken);

    Task GuardarCambiosAsync(CancellationToken cancellationToken);
}

public sealed record ConteoProgramacionesPlan(int Realizadas, int Omitidas, int Vencidas, int Pendientes, int Canceladas);
