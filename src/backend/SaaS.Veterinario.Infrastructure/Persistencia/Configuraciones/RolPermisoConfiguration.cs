using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SaaS.Veterinario.Domain.Permisos;
using SaaS.Veterinario.Domain.Roles;
using SaaS.Veterinario.Infrastructure.Persistencia.Seed;

namespace SaaS.Veterinario.Infrastructure.Persistencia.Configuraciones;

public sealed class RolPermisoConfiguration : IEntityTypeConfiguration<RolPermiso>
{
    public void Configure(EntityTypeBuilder<RolPermiso> builder)
    {
        builder.ToTable("roles_permisos");

        // PK compuesta: por definicion impide duplicar el mismo permiso en el mismo rol.
        builder.HasKey(rp => new { rp.RolId, rp.PermisoId });

        builder.Property(rp => rp.RolId)
            .HasColumnName("rol_id");

        builder.Property(rp => rp.PermisoId)
            .HasColumnName("permiso_id");

        // La relacion con Rol (incluyendo el DeleteBehavior) se configura desde
        // RolConfiguration, que es el agregado raiz de esta tabla puente. Aqui solo se
        // configura el lado hacia Permiso, que es dato de referencia y no debe eliminarse
        // en cascada mientras este asignado a algun rol.
        builder.HasOne<Permiso>()
            .WithMany()
            .HasForeignKey(rp => rp.PermisoId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasData(RolesPermisosSeed.RolesPermisos);
    }
}
