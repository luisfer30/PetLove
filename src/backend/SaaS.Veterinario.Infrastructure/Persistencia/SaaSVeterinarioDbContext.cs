using Microsoft.EntityFrameworkCore;
using SaaS.Veterinario.Domain.Membresias;
using SaaS.Veterinario.Domain.Permisos;
using SaaS.Veterinario.Domain.Roles;
using SaaS.Veterinario.Domain.Usuarios;
using SaaS.Veterinario.Domain.Veterinarias;

namespace SaaS.Veterinario.Infrastructure.Persistencia;

public sealed class SaaSVeterinarioDbContext(DbContextOptions<SaaSVeterinarioDbContext> options)
    : DbContext(options)
{
    public DbSet<Usuario> Usuarios => Set<Usuario>();

    public DbSet<Veterinaria> Veterinarias => Set<Veterinaria>();

    public DbSet<MembresiaVeterinaria> MembresiasVeterinaria => Set<MembresiaVeterinaria>();

    public DbSet<Rol> Roles => Set<Rol>();

    public DbSet<Permiso> Permisos => Set<Permiso>();

    // MembresiaRol y RolPermiso no se exponen como DbSet propio a proposito: son entidades
    // hijas de MembresiaVeterinaria y Rol respectivamente, y se manipulan solo a traves de
    // esos agregados (MembresiaVeterinaria.AsignarRol / Rol.AsignarPermiso). Igual quedan
    // mapeadas en el modelo mediante las relaciones configuradas en sus IEntityTypeConfiguration.

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(SaaSVeterinarioDbContext).Assembly);
    }
}
