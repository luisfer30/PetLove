using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SaaS.Veterinario.Domain.Usuarios;
using SaaS.Veterinario.Infrastructure.Persistencia.Conversiones;

namespace SaaS.Veterinario.Infrastructure.Persistencia.Configuraciones;

public sealed class UsuarioConfiguration : IEntityTypeConfiguration<Usuario>
{
    public void Configure(EntityTypeBuilder<Usuario> builder)
    {
        builder.ToTable("usuarios");

        builder.HasKey(u => u.Id);

        builder.Property(u => u.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(u => u.Nombre)
            .HasColumnName("nombre")
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(u => u.Correo)
            .HasColumnName("correo")
            .HasConversion(ConversionesValorObjetos.ConversorCorreo, ConversionesValorObjetos.ComparadorCorreo)
            .HasMaxLength(320)
            .IsRequired();

        // El dominio (Correo.Crear) normaliza siempre a minusculas antes de llegar aqui,
        // por lo que un indice UNIQUE comun ya garantiza unicidad case-insensitive real.
        builder.HasIndex(u => u.Correo)
            .IsUnique()
            .HasDatabaseName("ix_usuarios_correo_unico");

        builder.Property(u => u.Telefono)
            .HasColumnName("telefono")
            .HasMaxLength(30);

        builder.Property(u => u.CorreoVerificado)
            .HasColumnName("correo_verificado")
            .IsRequired();

        builder.Property(u => u.TelefonoVerificado)
            .HasColumnName("telefono_verificado")
            .IsRequired();

        builder.Property(u => u.Estado)
            .HasColumnName("estado")
            .HasConversion(ConversionesEstados.ConversorEstadoUsuario)
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(u => u.FechaCreacion)
            .HasColumnName("fecha_creacion")
            .IsRequired();

        builder.Property(u => u.FechaActualizacion)
            .HasColumnName("fecha_actualizacion")
            .IsRequired();
    }
}
