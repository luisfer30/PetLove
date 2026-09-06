using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SaaS.Veterinario.Domain.Permisos;
using SaaS.Veterinario.Infrastructure.Persistencia.Seed;

namespace SaaS.Veterinario.Infrastructure.Persistencia.Configuraciones;

public sealed class PermisoConfiguration : IEntityTypeConfiguration<Permiso>
{
    public void Configure(EntityTypeBuilder<Permiso> builder)
    {
        builder.ToTable("permisos");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(p => p.Codigo)
            .HasColumnName("codigo")
            .HasMaxLength(150)
            .IsRequired();

        builder.HasIndex(p => p.Codigo)
            .IsUnique()
            .HasDatabaseName("ix_permisos_codigo_unico");

        builder.Property(p => p.Nombre)
            .HasColumnName("nombre")
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(p => p.Descripcion)
            .HasColumnName("descripcion")
            .HasMaxLength(500);

        builder.HasData(RolesPermisosSeed.Permisos);
    }
}
