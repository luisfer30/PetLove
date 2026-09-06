using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SaaS.Veterinario.Domain.Usuarios;
using SaaS.Veterinario.Infrastructure.Identidad;

namespace SaaS.Veterinario.Infrastructure.Persistencia.Configuraciones;

/// <summary>
/// Renombra las tablas de Identity a snake_case y agrega la relacion 1:1 hacia el Usuario
/// de dominio. El resto del modelo de UsuarioIdentidad (columnas de Identity, indices de
/// NormalizedEmail/NormalizedUserName, etc.) ya lo configura IdentityUserContext en su
/// propio OnModelCreating; esta clase solo complementa lo que Identity no sabe.
/// </summary>
public sealed class UsuarioIdentidadConfiguration : IEntityTypeConfiguration<UsuarioIdentidad>
{
    public void Configure(EntityTypeBuilder<UsuarioIdentidad> builder)
    {
        builder.ToTable("usuarios_identidad");

        builder.Property(u => u.UsuarioId)
            .HasColumnName("usuario_id")
            .IsRequired();

        // 1:1 real: un Usuario de dominio tiene a lo sumo una identidad de autenticacion.
        builder.HasIndex(u => u.UsuarioId)
            .IsUnique()
            .HasDatabaseName("ix_usuarios_identidad_usuario_id_unico");

        // Restrict, no Cascade: borrar la identidad de autenticacion nunca debe arrastrar
        // al Usuario de dominio (ni viceversa) -- ver decision documentada en la Etapa 4.
        builder.HasOne<Usuario>()
            .WithMany()
            .HasForeignKey(u => u.UsuarioId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
