using Microsoft.EntityFrameworkCore;
using SaaS.Veterinario.Application.Abstracciones;
using SaaS.Veterinario.Domain.Citas;
using SaaS.Veterinario.Infrastructure.Persistencia;

namespace SaaS.Veterinario.Infrastructure.Citas;

public sealed class RepositorioCitas(SaaSVeterinarioDbContext contexto) : IRepositorioCitas
{
    public Task<Cita?> ObtenerPorIdAsync(Guid id, Guid veterinariaId, CancellationToken cancellationToken)
        => contexto.Citas.FirstOrDefaultAsync(c => c.Id == id && c.VeterinariaId == veterinariaId, cancellationToken);

    public async Task<IReadOnlyCollection<Cita>> ListarAsync(
        Guid veterinariaId,
        DateTimeOffset? fechaDesde,
        DateTimeOffset? fechaHasta,
        Guid? veterinarioUsuarioId,
        EstadoCita? estado,
        CancellationToken cancellationToken)
    {
        var consulta = contexto.Citas.Where(c => c.VeterinariaId == veterinariaId);

        if (fechaDesde is { } desde)
        {
            consulta = consulta.Where(c => c.FechaHoraInicio >= desde);
        }

        if (fechaHasta is { } hasta)
        {
            consulta = consulta.Where(c => c.FechaHoraInicio <= hasta);
        }

        if (veterinarioUsuarioId is { } veterinarioId)
        {
            consulta = consulta.Where(c => c.VeterinarioUsuarioId == veterinarioId);
        }

        if (estado is { } estadoFiltro)
        {
            consulta = consulta.Where(c => c.Estado == estadoFiltro);
        }

        return await consulta.OrderBy(c => c.FechaHoraInicio).ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<Cita>> ListarPorMascotaVeterinariaAsync(Guid mascotaVeterinariaId, Guid veterinariaId, CancellationToken cancellationToken)
        => await contexto.Citas
            .Where(c => c.MascotaVeterinariaId == mascotaVeterinariaId && c.VeterinariaId == veterinariaId)
            .ToListAsync(cancellationToken);

    public Task<bool> ExisteSolapamientoAsync(
        Guid veterinariaId,
        Guid veterinarioUsuarioId,
        DateTimeOffset fechaHoraInicio,
        DateTimeOffset fechaHoraFin,
        Guid? excluirCitaId,
        CancellationToken cancellationToken)
        => contexto.Citas.AnyAsync(
            c => c.VeterinariaId == veterinariaId
                && c.VeterinarioUsuarioId == veterinarioUsuarioId
                && c.Id != excluirCitaId
                && c.Estado != EstadoCita.Cancelada
                && c.Estado != EstadoCita.NoAsistio
                && c.FechaHoraInicio < fechaHoraFin
                && c.FechaHoraFin > fechaHoraInicio,
            cancellationToken);

    public async Task AgregarAsync(Cita cita, CancellationToken cancellationToken)
    {
        contexto.Citas.Add(cita);
        await contexto.SaveChangesAsync(cancellationToken);
    }

    public Task GuardarCambiosAsync(CancellationToken cancellationToken)
        => contexto.SaveChangesAsync(cancellationToken);
}
