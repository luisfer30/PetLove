using Microsoft.EntityFrameworkCore;
using SaaS.Veterinario.Application.Abstracciones;
using SaaS.Veterinario.Domain.Mascotas;
using SaaS.Veterinario.Domain.ObjetosValor;
using SaaS.Veterinario.Infrastructure.Persistencia;

namespace SaaS.Veterinario.Infrastructure.Mascotas;

public sealed class RepositorioMascotasVeterinarias(SaaSVeterinarioDbContext contexto) : IRepositorioMascotasVeterinarias
{
    public Task<bool> ExisteVinculoAsync(Guid veterinariaId, Guid mascotaId, CancellationToken cancellationToken)
        => contexto.MascotasVeterinaria.AnyAsync(mv => mv.VeterinariaId == veterinariaId && mv.MascotaId == mascotaId, cancellationToken);

    public Task<MascotaVeterinaria?> ObtenerPorIdAsync(Guid id, Guid veterinariaId, CancellationToken cancellationToken)
        => contexto.MascotasVeterinaria.FirstOrDefaultAsync(mv => mv.Id == id && mv.VeterinariaId == veterinariaId, cancellationToken);

    public async Task<InfoMascotaFicha?> ObtenerFichaPorIdAsync(Guid id, Guid veterinariaId, CancellationToken cancellationToken)
    {
        // El filtro va sobre mv.Id/mv.VeterinariaId ANTES del Select: EF Core no puede
        // traducir un Where posterior que reconstruye el record FichaCruda solo para leer
        // una de sus propiedades (ver ConsultaFichasCrudas).
        var cruda = await ConsultaFichasCrudas(mv => mv.Id == id && mv.VeterinariaId == veterinariaId)
            .FirstOrDefaultAsync(cancellationToken);

        return cruda is null ? null : AProyeccion(cruda);
    }

    public async Task<IReadOnlyCollection<InfoMascotaFicha>> ListarPorVeterinariaAsync(Guid veterinariaId, CancellationToken cancellationToken)
    {
        var crudas = await ConsultaFichasCrudas(mv => mv.VeterinariaId == veterinariaId)
            .ToListAsync(cancellationToken);

        return crudas.Select(AProyeccion).ToList();
    }

    public async Task AgregarAsync(MascotaVeterinaria mascotaVeterinaria, CancellationToken cancellationToken)
    {
        contexto.MascotasVeterinaria.Add(mascotaVeterinaria);
        await contexto.SaveChangesAsync(cancellationToken);
    }

    // El filtro (predicado sobre MascotaVeterinaria) se aplica ANTES del Select: EF Core no
    // logra traducir un Where posterior que reconstruye el record FichaCruda solo para leer
    // una de sus propiedades.
    private IQueryable<FichaCruda> ConsultaFichasCrudas(System.Linq.Expressions.Expression<Func<MascotaVeterinaria, bool>> filtro)
        => from mv in contexto.MascotasVeterinaria.Where(filtro)
           join m in contexto.Mascotas on mv.MascotaId equals m.Id
           join e in contexto.Especies on m.EspecieId equals e.Id
           join r in contexto.Razas on m.RazaId equals r.Id into razas
           from r in razas.DefaultIfEmpty()
           select new FichaCruda(
               mv.Id,
               mv.VeterinariaId,
               m.Id,
               m.CodigoPublico,
               m.Nombre,
               e.Nombre,
               r == null ? null : r.Nombre,
               m.Sexo,
               mv.NumeroHistoria,
               mv.Estado);

    private static InfoMascotaFicha AProyeccion(FichaCruda f)
        => new(f.MascotaVeterinariaId, f.MascotaId, f.CodigoPublico.Valor, f.Nombre, f.Especie, f.Raza, f.Sexo.ToString(), f.NumeroHistoria, f.EstadoLocal.ToString());

    private sealed record FichaCruda(
        Guid MascotaVeterinariaId,
        Guid VeterinariaId,
        Guid MascotaId,
        CodigoPublico CodigoPublico,
        string Nombre,
        string Especie,
        string? Raza,
        SexoMascota Sexo,
        string? NumeroHistoria,
        EstadoMascotaVeterinaria EstadoLocal);
}
