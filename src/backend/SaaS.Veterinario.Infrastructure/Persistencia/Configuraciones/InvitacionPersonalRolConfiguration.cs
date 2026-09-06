using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SaaS.Veterinario.Domain.Personal;
using SaaS.Veterinario.Domain.Roles;

namespace SaaS.Veterinario.Infrastructure.Persistencia.Configuraciones;

public sealed class InvitacionPersonalRolConfiguration : IEntityTypeConfiguration<InvitacionPersonalRol>
{
    public void Configure(EntityTypeBuilder<InvitacionPersonalRol> builder)
    {
        builder.ToTable("invitaciones_personal_roles");

        // PK compuesta: por definicion impide duplicar el mismo rol en la misma invitacion.
        builder.HasKey(ir => new { ir.InvitacionId, ir.RolId });

        builder.Property(ir => ir.InvitacionId)
            .HasColumnName("invitacion_id");

        builder.Property(ir => ir.RolId)
            .HasColumnName("rol_id");

        // La relacion con InvitacionPersonal (incluyendo el DeleteBehavior) se configura desde
        // InvitacionPersonalConfiguration, que es el agregado raiz de esta tabla puente. Aqui
        // solo se configura el lado hacia Rol, dato de referencia que no debe eliminarse en
        // cascada mientras este asignado a alguna invitacion.
        builder.HasOne<Rol>()
            .WithMany()
            .HasForeignKey(ir => ir.RolId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
