using Microsoft.EntityFrameworkCore;
using SaaS.Veterinario.Application.Abstracciones;
using SaaS.Veterinario.Domain.Consultas;
using SaaS.Veterinario.Infrastructure.Persistencia;

namespace SaaS.Veterinario.Infrastructure.Consultas;

public sealed class RepositorioConsultas(SaaSVeterinarioDbContext contexto) : IRepositorioConsultas
{
    public Task<ConsultaVeterinaria?> ObtenerPorIdAsync(Guid id, Guid veterinariaId, CancellationToken cancellationToken)
        => contexto.Consultas.FirstOrDefaultAsync(c => c.Id == id && c.VeterinariaId == veterinariaId, cancellationToken);

    public async Task<IReadOnlyCollection<ConsultaVeterinaria>> ListarPorMascotaVeterinariaAsync(
        Guid mascotaVeterinariaId, Guid veterinariaId, CancellationToken cancellationToken)
        => await contexto.Consultas
            .Where(c => c.MascotaVeterinariaId == mascotaVeterinariaId && c.VeterinariaId == veterinariaId)
            .ToListAsync(cancellationToken);

    public async Task AgregarAsync(ConsultaVeterinaria consulta, CancellationToken cancellationToken)
    {
        contexto.Consultas.Add(consulta);
        await contexto.SaveChangesAsync(cancellationToken);
    }

    public Task GuardarCambiosAsync(CancellationToken cancellationToken)
        => contexto.SaveChangesAsync(cancellationToken);
}
