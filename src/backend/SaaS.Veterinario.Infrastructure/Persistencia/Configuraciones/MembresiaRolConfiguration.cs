using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SaaS.Veterinario.Domain.Membresias;
using SaaS.Veterinario.Domain.Roles;

namespace SaaS.Veterinario.Infrastructure.Persistencia.Configuraciones;

public sealed class MembresiaRolConfiguration : IEntityTypeConfiguration<MembresiaRol>
{
    public void Configure(EntityTypeBuilder<MembresiaRol> builder)
    {
        builder.ToTable("membresias_roles");

        // PK compuesta: por definicion impide duplicar el mismo rol en la misma membresia.
        builder.HasKey(mr => new { mr.MembresiaId, mr.RolId });

        builder.Property(mr => mr.MembresiaId)
            .HasColumnName("membresia_id");

        builder.Property(mr => mr.RolId)
            .HasColumnName("rol_id");

        // La relacion con MembresiaVeterinaria (incluyendo el DeleteBehavior) se configura
        // desde MembresiaVeterinariaConfiguration, que es el agregado raiz de esta tabla puente.
        // Aqui solo se configura el lado hacia Rol, que es dato de referencia y no debe
        // eliminarse en cascada mientras este asignado a alguna membresia.
        builder.HasOne<Rol>()
            .WithMany()
            .HasForeignKey(mr => mr.RolId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
