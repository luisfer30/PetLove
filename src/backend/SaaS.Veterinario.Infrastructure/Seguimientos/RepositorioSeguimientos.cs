using Microsoft.EntityFrameworkCore;
using SaaS.Veterinario.Application.Abstracciones;
using SaaS.Veterinario.Domain.Seguimientos;
using SaaS.Veterinario.Infrastructure.Persistencia;

namespace SaaS.Veterinario.Infrastructure.Seguimientos;

public sealed class RepositorioSeguimientos(SaaSVeterinarioDbContext contexto) : IRepositorioSeguimientos
{
    public Task<SeguimientoClinico?> ObtenerPorIdAsync(Guid id, Guid veterinariaId, CancellationToken cancellationToken)
        => contexto.SeguimientosClinicos.FirstOrDefaultAsync(s => s.Id == id && s.VeterinariaId == veterinariaId, cancellationToken);

    public async Task<IReadOnlyCollection<SeguimientoClinico>> ListarPorMascotaVeterinariaAsync(Guid mascotaVeterinariaId, Guid veterinariaId, CancellationToken cancellationToken)
        => await contexto.SeguimientosClinicos
            .Where(s => s.MascotaVeterinariaId == mascotaVeterinariaId && s.VeterinariaId == veterinariaId)
            .OrderBy(s => s.FechaObjetivo)
            .ToListAsync(cancellationToken);

    public Task<bool> ExisteAutoGeneradoParaConsultaAsync(Guid consultaOrigenId, CancellationToken cancellationToken)
        => contexto.SeguimientosClinicos.AnyAsync(
            s => s.ConsultaOrigenId == consultaOrigenId && s.EsGeneradoDesdeProximoControl, cancellationToken);

    public async Task AgregarAsync(SeguimientoClinico seguimiento, CancellationToken cancellationToken)
    {
        contexto.SeguimientosClinicos.Add(seguimiento);
        await contexto.SaveChangesAsync(cancellationToken);
    }

    public Task GuardarCambiosAsync(CancellationToken cancellationToken)
        => contexto.SaveChangesAsync(cancellationToken);
}
