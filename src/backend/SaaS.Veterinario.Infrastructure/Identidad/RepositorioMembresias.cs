using Microsoft.EntityFrameworkCore;
using SaaS.Veterinario.Application.Abstracciones;
using SaaS.Veterinario.Domain.Membresias;
using SaaS.Veterinario.Domain.Roles;
using SaaS.Veterinario.Infrastructure.Persistencia;

namespace SaaS.Veterinario.Infrastructure.Identidad;

public sealed class RepositorioMembresias(SaaSVeterinarioDbContext contexto) : IRepositorioMembresias
{
    public async Task<InfoContextoMembresia?> ObtenerContextoActivoAsync(Guid usuarioId, CancellationToken cancellationToken)
    {
        var membresia = await contexto.MembresiasVeterinaria
            .FirstOrDefaultAsync(m => m.UsuarioId == usuarioId && m.Estado == EstadoMembresiaVeterinaria.Activa, cancellationToken);

        if (membresia is null)
        {
            return null;
        }

        var rolIds = await contexto.Set<MembresiaRol>()
            .Where(mr => mr.MembresiaId == membresia.Id)
            .Select(mr => mr.RolId)
            .ToListAsync(cancellationToken);

        if (rolIds.Count == 0)
        {
            return new InfoContextoMembresia(membresia.Id, membresia.VeterinariaId, [], []);
        }

        var roles = await contexto.Roles
            .Where(r => rolIds.Contains(r.Id))
            .Select(r => r.Codigo)
            .ToListAsync(cancellationToken);

        // Union de permisos de todos los roles, sin duplicados (Distinct sobre el codigo).
        var permisos = await contexto.Set<RolPermiso>()
            .Where(rp => rolIds.Contains(rp.RolId))
            .Join(contexto.Permisos, rp => rp.PermisoId, p => p.Id, (rp, p) => p.Codigo)
            .Distinct()
            .ToListAsync(cancellationToken);

        return new InfoContextoMembresia(membresia.Id, membresia.VeterinariaId, roles, permisos);
    }

    public async Task<IReadOnlyCollection<InfoMiembroPersonal>> ObtenerPersonalActivoAsync(Guid veterinariaId, CancellationToken cancellationToken)
    {
        var membresias = await contexto.MembresiasVeterinaria
            .Where(m => m.VeterinariaId == veterinariaId && m.Estado == EstadoMembresiaVeterinaria.Activa)
            .ToListAsync(cancellationToken);

        if (membresias.Count == 0)
        {
            return [];
        }

        var membresiaIds = membresias.Select(m => m.Id).ToList();
        var usuarioIds = membresias.Select(m => m.UsuarioId).ToList();

        var usuarios = await contexto.Usuarios
            .Where(u => usuarioIds.Contains(u.Id))
            .ToListAsync(cancellationToken);

        var rolesPorMembresia = await contexto.Set<MembresiaRol>()
            .Where(mr => membresiaIds.Contains(mr.MembresiaId))
            .Join(contexto.Roles, mr => mr.RolId, r => r.Id, (mr, r) => new { mr.MembresiaId, r.Codigo })
            .ToListAsync(cancellationToken);

        return membresias
            .Select(m =>
            {
                var usuario = usuarios.First(u => u.Id == m.UsuarioId);
                var roles = rolesPorMembresia
                    .Where(x => x.MembresiaId == m.Id)
                    .Select(x => x.Codigo)
                    .ToArray();

                return new InfoMiembroPersonal(m.Id, usuario.Id, usuario.Nombre, usuario.Correo.Valor, roles);
            })
            .ToList();
    }
}
