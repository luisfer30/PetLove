using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SaaS.Veterinario.Domain.Veterinarias;
using SaaS.Veterinario.Infrastructure.Persistencia.Conversiones;

namespace SaaS.Veterinario.Infrastructure.Persistencia.Configuraciones;

public sealed class VeterinariaConfiguration : IEntityTypeConfiguration<Veterinaria>
{
    public void Configure(EntityTypeBuilder<Veterinaria> builder)
    {
        builder.ToTable("veterinarias");

        builder.HasKey(v => v.Id);

        builder.Property(v => v.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        var conversorCodigoPublico = ConversionesValorObjetos.CrearConversorCodigoPublico(Veterinaria.PrefijoCodigoPublico);

        builder.Property(v => v.CodigoPublico)
            .HasColumnName("codigo_publico")
            .HasConversion(conversorCodigoPublico, ConversionesValorObjetos.ComparadorCodigoPublico)
            .HasMaxLength(20)
            .IsRequired();

        builder.HasIndex(v => v.CodigoPublico)
            .IsUnique()
            .HasDatabaseName("ix_veterinarias_codigo_publico_unico");

        builder.Property(v => v.NombreComercial)
            .HasColumnName("nombre_comercial")
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(v => v.RazonSocial)
            .HasColumnName("razon_social")
            .HasMaxLength(150);

        builder.Property(v => v.Ruc)
            .HasColumnName("ruc")
            .HasMaxLength(30);

        builder.Property(v => v.Correo)
            .HasColumnName("correo")
            .HasConversion(ConversionesValorObjetos.ConversorCorreo, ConversionesValorObjetos.ComparadorCorreo)
            .HasMaxLength(320)
            .IsRequired();

        builder.Property(v => v.Telefono)
            .HasColumnName("telefono")
            .HasMaxLength(30);

        builder.Property(v => v.Direccion)
            .HasColumnName("direccion")
            .HasMaxLength(250);

        builder.Property(v => v.Ciudad)
            .HasColumnName("ciudad")
            .HasMaxLength(100);

        builder.Property(v => v.Pais)
            .HasColumnName("pais")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(v => v.ZonaHoraria)
            .HasColumnName("zona_horaria")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(v => v.Estado)
            .HasColumnName("estado")
            .HasConversion(ConversionesEstados.ConversorEstadoVeterinaria)
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(v => v.FechaCreacion)
            .HasColumnName("fecha_creacion")
            .IsRequired();

        builder.Property(v => v.FechaActualizacion)
            .HasColumnName("fecha_actualizacion")
            .IsRequired();

        builder.Property(v => v.FechaEliminacion)
            .HasColumnName("fecha_eliminacion");
    }
}
