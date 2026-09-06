using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SaaS.Veterinario.Domain.Mascotas;
using SaaS.Veterinario.Infrastructure.Persistencia.Conversiones;

namespace SaaS.Veterinario.Infrastructure.Persistencia.Configuraciones;

/// <summary>
/// Identidad global de plataforma (R01/R02): esta tabla no contiene veterinaria_id,
/// cliente_id ni responsable_id. La relacion con una veterinaria vive en MascotaVeterinaria.
/// </summary>
public sealed class MascotaConfiguration : IEntityTypeConfiguration<Mascota>
{
    public void Configure(EntityTypeBuilder<Mascota> builder)
    {
        builder.ToTable("mascotas");

        builder.HasKey(m => m.Id);

        builder.Property(m => m.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        var conversorCodigoPublico = ConversionesValorObjetos.CrearConversorCodigoPublico(Mascota.PrefijoCodigoPublico);

        builder.Property(m => m.CodigoPublico)
            .HasColumnName("codigo_publico")
            .HasConversion(conversorCodigoPublico, ConversionesValorObjetos.ComparadorCodigoPublico)
            .HasMaxLength(20)
            .IsRequired();

        builder.HasIndex(m => m.CodigoPublico)
            .IsUnique()
            .HasDatabaseName("ix_mascotas_codigo_publico_unico");

        builder.Property(m => m.Nombre)
            .HasColumnName("nombre")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(m => m.EspecieId)
            .HasColumnName("especie_id")
            .IsRequired();

        builder.Property(m => m.RazaId)
            .HasColumnName("raza_id");

        builder.Property(m => m.Sexo)
            .HasColumnName("sexo")
            .HasConversion(ConversionesEstadosMascotas.ConversorSexoMascota)
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(m => m.FechaNacimiento)
            .HasColumnName("fecha_nacimiento");

        builder.Property(m => m.FechaNacimientoAproximada)
            .HasColumnName("fecha_nacimiento_aproximada")
            .IsRequired();

        builder.Property(m => m.Color)
            .HasColumnName("color")
            .HasMaxLength(50);

        builder.Property(m => m.NumeroMicrochip)
            .HasColumnName("numero_microchip")
            .HasMaxLength(50);

        builder.HasIndex(m => m.NumeroMicrochip)
            .IsUnique()
            .HasDatabaseName("ux_mascotas_numero_microchip")
            .HasFilter("numero_microchip IS NOT NULL");

        builder.Property(m => m.Esterilizado)
            .HasColumnName("esterilizado");

        builder.Property(m => m.Estado)
            .HasColumnName("estado")
            .HasConversion(ConversionesEstadosMascotas.ConversorEstadoMascota)
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(m => m.FechaCreacion)
            .HasColumnName("fecha_creacion")
            .IsRequired();

        builder.Property(m => m.FechaActualizacion)
            .HasColumnName("fecha_actualizacion")
            .IsRequired();

        builder.Property(m => m.FechaEliminacion)
            .HasColumnName("fecha_eliminacion");

        builder.HasOne<Especie>()
            .WithMany()
            .HasForeignKey(m => m.EspecieId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Raza>()
            .WithMany()
            .HasForeignKey(m => m.RazaId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
