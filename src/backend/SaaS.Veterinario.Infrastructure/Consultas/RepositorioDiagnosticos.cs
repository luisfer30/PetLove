using Microsoft.EntityFrameworkCore;
using SaaS.Veterinario.Application.Abstracciones;
using SaaS.Veterinario.Domain.Consultas;
using SaaS.Veterinario.Infrastructure.Persistencia;

namespace SaaS.Veterinario.Infrastructure.Consultas;

public sealed class RepositorioDiagnosticos(SaaSVeterinarioDbContext contexto) : IRepositorioDiagnosticos
{
    public Task<DiagnosticoConsulta?> ObtenerPorIdAsync(Guid id, Guid consultaId, CancellationToken cancellationToken)
        => contexto.DiagnosticosConsulta.FirstOrDefaultAsync(d => d.Id == id && d.ConsultaId == consultaId, cancellationToken);

    public async Task<IReadOnlyCollection<DiagnosticoConsulta>> ListarPorConsultaAsync(Guid consultaId, CancellationToken cancellationToken)
        => await contexto.DiagnosticosConsulta
            .Where(d => d.ConsultaId == consultaId)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyCollection<DiagnosticoConsulta>> ListarPorConsultasAsync(IReadOnlyCollection<Guid> consultaIds, CancellationToken cancellationToken)
    {
        if (consultaIds.Count == 0)
        {
            return [];
        }

        return await contexto.DiagnosticosConsulta
            .Where(d => consultaIds.Contains(d.ConsultaId))
            .ToListAsync(cancellationToken);
    }

    public Task<bool> ExistePrincipalAsync(Guid consultaId, Guid? excluirDiagnosticoId, CancellationToken cancellationToken)
        => contexto.DiagnosticosConsulta.AnyAsync(
            d => d.ConsultaId == consultaId && d.EsPrincipal && d.Id != excluirDiagnosticoId,
            cancellationToken);

    public async Task AgregarAsync(DiagnosticoConsulta diagnostico, CancellationToken cancellationToken)
    {
        contexto.DiagnosticosConsulta.Add(diagnostico);
        await contexto.SaveChangesAsync(cancellationToken);
    }

    public Task GuardarCambiosAsync(CancellationToken cancellationToken)
        => contexto.SaveChangesAsync(cancellationToken);

    public async Task EliminarAsync(DiagnosticoConsulta diagnostico, CancellationToken cancellationToken)
    {
        contexto.DiagnosticosConsulta.Remove(diagnostico);
        await contexto.SaveChangesAsync(cancellationToken);
    }
}
