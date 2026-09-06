using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SaaS.Veterinario.Domain.Roles;
using SaaS.Veterinario.Infrastructure.Persistencia.Conversiones;
using SaaS.Veterinario.Infrastructure.Persistencia.Seed;

namespace SaaS.Veterinario.Infrastructure.Persistencia.Configuraciones;

public sealed class RolConfiguration : IEntityTypeConfiguration<Rol>
{
    public void Configure(EntityTypeBuilder<Rol> builder)
    {
        builder.ToTable("roles");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(r => r.Codigo)
            .HasColumnName("codigo")
            .HasMaxLength(100)
            .IsRequired();

        builder.HasIndex(r => r.Codigo)
            .IsUnique()
            .HasDatabaseName("ix_roles_codigo_unico");

        builder.Property(r => r.Nombre)
            .HasColumnName("nombre")
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(r => r.Descripcion)
            .HasColumnName("descripcion")
            .HasMaxLength(500);

        builder.Property(r => r.Estado)
            .HasColumnName("estado")
            .HasConversion(ConversionesEstados.ConversorEstadoRol)
            .HasMaxLength(20)
            .IsRequired();

        // Rol es el agregado raiz de RolPermiso: cascade es seguro aqui porque solo
        // limpia las filas hijas de ESTE rol, nunca las de otro agregado.
        builder.HasMany(r => r.Permisos)
            .WithOne()
            .HasForeignKey(rp => rp.RolId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(nameof(Rol.Permisos))
            .HasField("_permisos")
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasData(RolesPermisosSeed.Roles);
    }
}
