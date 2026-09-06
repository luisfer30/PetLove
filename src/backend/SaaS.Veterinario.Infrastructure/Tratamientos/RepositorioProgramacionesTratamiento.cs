using Microsoft.EntityFrameworkCore;
using SaaS.Veterinario.Application.Abstracciones;
using SaaS.Veterinario.Domain.Tratamientos;
using SaaS.Veterinario.Infrastructure.Persistencia;

namespace SaaS.Veterinario.Infrastructure.Tratamientos;

public sealed class RepositorioProgramacionesTratamiento(SaaSVeterinarioDbContext contexto) : IRepositorioProgramacionesTratamiento
{
    public Task<ProgramacionTratamiento?> ObtenerPorIdAsync(Guid id, Guid veterinariaId, CancellationToken cancellationToken)
        => contexto.ProgramacionesTratamiento.FirstOrDefaultAsync(p => p.Id == id && p.VeterinariaId == veterinariaId, cancellationToken);

    public async Task<IReadOnlyCollection<ProgramacionTratamiento>> ListarPorPlanAsync(Guid planTratamientoId, Guid veterinariaId, CancellationToken cancellationToken)
        => await contexto.ProgramacionesTratamiento
            .Where(p => p.PlanTratamientoId == planTratamientoId && p.VeterinariaId == veterinariaId)
            .OrderBy(p => p.FechaHoraProgramada)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyCollection<ProgramacionTratamiento>> ListarPorMascotaVeterinariaAsync(Guid mascotaVeterinariaId, Guid veterinariaId, CancellationToken cancellationToken)
        => await contexto.ProgramacionesTratamiento
            .Where(p => p.MascotaVeterinariaId == mascotaVeterinariaId && p.VeterinariaId == veterinariaId)
            .OrderBy(p => p.FechaHoraProgramada)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyCollection<ProgramacionTratamiento>> ListarPendientesPorPlanAsync(Guid planTratamientoId, CancellationToken cancellationToken)
        => await contexto.ProgramacionesTratamiento
            .Where(p => p.PlanTratamientoId == planTratamientoId && p.Estado == EstadoProgramacionTratamiento.Pendiente)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyCollection<ProgramacionTratamiento>> ListarPendientesVencidasAsync(DateTimeOffset fechaCorte, CancellationToken cancellationToken)
        => await contexto.ProgramacionesTratamiento
            .Where(p => p.Estado == EstadoProgramacionTratamiento.Pendiente && p.FechaHoraProgramada < fechaCorte)
            .ToListAsync(cancellationToken);

    public Task<bool> ExisteEnFechaAsync(Guid itemTratamientoId, DateTimeOffset fechaHoraProgramada, CancellationToken cancellationToken)
        => contexto.ProgramacionesTratamiento.AnyAsync(
            p => p.ItemTratamientoId == itemTratamientoId && p.FechaHoraProgramada == fechaHoraProgramada, cancellationToken);

    public async Task<ConteoProgramacionesPlan> ContarPorEstadoAsync(Guid planTratamientoId, Guid veterinariaId, CancellationToken cancellationToken)
    {
        var conteos = await contexto.ProgramacionesTratamiento
            .Where(p => p.PlanTratamientoId == planTratamientoId && p.VeterinariaId == veterinariaId)
            .GroupBy(p => p.Estado)
            .Select(g => new { Estado = g.Key, Cantidad = g.Count() })
            .ToListAsync(cancellationToken);

        int Obtener(EstadoProgramacionTratamiento estado) => conteos.FirstOrDefault(c => c.Estado == estado)?.Cantidad ?? 0;

        return new ConteoProgramacionesPlan(
            Obtener(EstadoProgramacionTratamiento.Realizada),
            Obtener(EstadoProgramacionTratamiento.Omitida),
            Obtener(EstadoProgramacionTratamiento.Vencida),
            Obtener(EstadoProgramacionTratamiento.Pendiente),
            Obtener(EstadoProgramacionTratamiento.Cancelada));
    }

    public async Task AgregarRangoAsync(IReadOnlyCollection<ProgramacionTratamiento> programaciones, CancellationToken cancellationToken)
    {
        contexto.ProgramacionesTratamiento.AddRange(programaciones);
        await contexto.SaveChangesAsync(cancellationToken);
    }

    public Task GuardarCambiosAsync(CancellationToken cancellationToken)
        => contexto.SaveChangesAsync(cancellationToken);
}
