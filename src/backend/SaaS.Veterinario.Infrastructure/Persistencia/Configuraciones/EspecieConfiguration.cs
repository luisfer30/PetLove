using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SaaS.Veterinario.Domain.Mascotas;
using SaaS.Veterinario.Infrastructure.Persistencia.Conversiones;
using SaaS.Veterinario.Infrastructure.Persistencia.Seed;

namespace SaaS.Veterinario.Infrastructure.Persistencia.Configuraciones;

public sealed class EspecieConfiguration : IEntityTypeConfiguration<Especie>
{
    public void Configure(EntityTypeBuilder<Especie> builder)
    {
        builder.ToTable("especies");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(e => e.Codigo)
            .HasColumnName("codigo")
            .HasMaxLength(50)
            .IsRequired();

        builder.HasIndex(e => e.Codigo)
            .IsUnique()
            .HasDatabaseName("ix_especies_codigo_unico");

        builder.Property(e => e.Nombre)
            .HasColumnName("nombre")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(e => e.Estado)
            .HasColumnName("estado")
            .HasConversion(ConversionesEstadosMascotas.ConversorEstadoEspecie)
            .HasMaxLength(20)
            .IsRequired();

        builder.HasData(EspeciesSeed.Especies);
    }
}
