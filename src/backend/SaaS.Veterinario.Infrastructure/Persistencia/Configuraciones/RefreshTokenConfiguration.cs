using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SaaS.Veterinario.Domain.Usuarios;
using SaaS.Veterinario.Infrastructure.Identidad;

namespace SaaS.Veterinario.Infrastructure.Persistencia.Configuraciones;

public sealed class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("refresh_tokens");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(t => t.UsuarioId)
            .HasColumnName("usuario_id")
            .IsRequired();

        builder.Property(t => t.TokenHash)
            .HasColumnName("token_hash")
            .HasMaxLength(128)
            .IsRequired();

        builder.HasIndex(t => t.TokenHash)
            .IsUnique()
            .HasDatabaseName("ix_refresh_tokens_token_hash_unico");

        builder.Property(t => t.FechaCreacion)
            .HasColumnName("fecha_creacion")
            .IsRequired();

        builder.Property(t => t.FechaExpiracion)
            .HasColumnName("fecha_expiracion")
            .IsRequired();

        builder.Property(t => t.FechaRevocacion)
            .HasColumnName("fecha_revocacion");

        builder.Property(t => t.Dispositivo)
            .HasColumnName("dispositivo")
            .HasMaxLength(250);

        builder.Property(t => t.DireccionIp)
            .HasColumnName("direccion_ip")
            .HasMaxLength(45);

        builder.HasIndex(t => t.UsuarioId)
            .HasDatabaseName("ix_refresh_tokens_usuario_id");

        // Restrict, no Cascade: se documenta a proposito (punto 27). Un RefreshToken sin
        // Usuario detras no tiene ningun sentido funcional, pero mantenemos el mismo
        // criterio del resto del modelo -- nunca cascada automatica sobre Usuario -- para
        // no crear una excepcion silenciosa a esa regla. Si en el futuro se necesita poder
        // eliminar fisicamente un Usuario, la limpieza de sus refresh tokens debe ser un
        // paso explicito, no un efecto secundario de la FK.
        builder.HasOne<Usuario>()
            .WithMany()
            .HasForeignKey(t => t.UsuarioId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
