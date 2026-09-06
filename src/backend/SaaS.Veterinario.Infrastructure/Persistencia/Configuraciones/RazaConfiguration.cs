using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SaaS.Veterinario.Domain.Mascotas;
using SaaS.Veterinario.Infrastructure.Persistencia.Conversiones;

namespace SaaS.Veterinario.Infrastructure.Persistencia.Configuraciones;

public sealed class RazaConfiguration : IEntityTypeConfiguration<Raza>
{
    public void Configure(EntityTypeBuilder<Raza> builder)
    {
        builder.ToTable("razas");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(r => r.EspecieId)
            .HasColumnName("especie_id")
            .IsRequired();

        builder.Property(r => r.Nombre)
            .HasColumnName("nombre")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(r => r.Estado)
            .HasColumnName("estado")
            .HasConversion(ConversionesEstadosMascotas.ConversorEstadoRaza)
            .HasMaxLength(20)
            .IsRequired();

        builder.HasIndex(r => new { r.EspecieId, r.Nombre })
            .IsUnique()
            .HasDatabaseName("ux_razas_especie_nombre");

        builder.HasOne<Especie>()
            .WithMany()
            .HasForeignKey(r => r.EspecieId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
