using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SaaS.Veterinario.Domain.Mascotas;
using SaaS.Veterinario.Domain.Veterinarias;
using SaaS.Veterinario.Infrastructure.Persistencia.Conversiones;

namespace SaaS.Veterinario.Infrastructure.Persistencia.Configuraciones;

/// <summary>
/// Vinculo privado de una Veterinaria con una Mascota global (R04). Una Mascota puede tener
/// muchas filas (una por veterinaria distinta) pero nunca dos en la misma veterinaria.
/// </summary>
public sealed class MascotaVeterinariaConfiguration : IEntityTypeConfiguration<MascotaVeterinaria>
{
    public void Configure(EntityTypeBuilder<MascotaVeterinaria> builder)
    {
        builder.ToTable("mascotas_veterinaria");

        builder.HasKey(m => m.Id);

        builder.Property(m => m.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(m => m.VeterinariaId)
            .HasColumnName("veterinaria_id")
            .IsRequired();

        builder.Property(m => m.MascotaId)
            .HasColumnName("mascota_id")
            .IsRequired();

        builder.Property(m => m.NumeroHistoria)
            .HasColumnName("numero_historia")
            .HasMaxLength(50);

        builder.Property(m => m.Estado)
            .HasColumnName("estado")
            .HasConversion(ConversionesEstadosMascotas.ConversorEstadoMascotaVeterinaria)
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(m => m.FechaRegistro)
            .HasColumnName("fecha_registro")
            .IsRequired();

        builder.Property(m => m.ObservacionesInternas)
            .HasColumnName("observaciones_internas")
            .HasMaxLength(500);

        builder.Property(m => m.FechaCreacion)
            .HasColumnName("fecha_creacion")
            .IsRequired();

        builder.Property(m => m.FechaActualizacion)
            .HasColumnName("fecha_actualizacion")
            .IsRequired();

        builder.Property(m => m.FechaEliminacion)
            .HasColumnName("fecha_eliminacion");

        builder.HasIndex(m => new { m.VeterinariaId, m.MascotaId })
            .IsUnique()
            .HasDatabaseName("ux_mascotas_veterinaria_veterinaria_mascota");

        builder.HasOne<Veterinaria>()
            .WithMany()
            .HasForeignKey(m => m.VeterinariaId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Mascota>()
            .WithMany()
            .HasForeignKey(m => m.MascotaId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
