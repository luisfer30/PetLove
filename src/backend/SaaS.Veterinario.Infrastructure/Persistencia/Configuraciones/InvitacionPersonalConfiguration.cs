using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SaaS.Veterinario.Domain.Personal;
using SaaS.Veterinario.Domain.Usuarios;
using SaaS.Veterinario.Domain.Veterinarias;
using SaaS.Veterinario.Infrastructure.Persistencia.Conversiones;

namespace SaaS.Veterinario.Infrastructure.Persistencia.Configuraciones;

public sealed class InvitacionPersonalConfiguration : IEntityTypeConfiguration<InvitacionPersonal>
{
    public void Configure(EntityTypeBuilder<InvitacionPersonal> builder)
    {
        builder.ToTable("invitaciones_personal");

        builder.HasKey(i => i.Id);

        builder.Property(i => i.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(i => i.VeterinariaId)
            .HasColumnName("veterinaria_id")
            .IsRequired();

        builder.Property(i => i.Correo)
            .HasColumnName("correo")
            .HasConversion(ConversionesValorObjetos.ConversorCorreo, ConversionesValorObjetos.ComparadorCorreo)
            .HasMaxLength(320)
            .IsRequired();

        builder.Property(i => i.TokenHash)
            .HasColumnName("token_hash")
            .HasMaxLength(128)
            .IsRequired();

        builder.HasIndex(i => i.TokenHash)
            .IsUnique()
            .HasDatabaseName("ix_invitaciones_personal_token_hash_unico");

        builder.Property(i => i.Estado)
            .HasColumnName("estado")
            .HasConversion(ConversionesEstados.ConversorEstadoInvitacionPersonal)
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(i => i.FechaExpiracion)
            .HasColumnName("fecha_expiracion")
            .IsRequired();

        builder.Property(i => i.FechaAceptacion)
            .HasColumnName("fecha_aceptacion");

        builder.Property(i => i.CreadoPorUsuarioId)
            .HasColumnName("creado_por_usuario_id")
            .IsRequired();

        builder.Property(i => i.FechaCreacion)
            .HasColumnName("fecha_creacion")
            .IsRequired();

        // Solo una invitacion PENDIENTE por correo dentro de una misma veterinaria; historicas
        // (aceptadas/expiradas/canceladas) si pueden repetirse -- mismo patron que la membresia
        // activa unica de la Etapa 3.
        builder.HasIndex(i => new { i.VeterinariaId, i.Correo })
            .IsUnique()
            .HasFilter("estado = 'PENDIENTE'")
            .HasDatabaseName("ux_invitaciones_personal_veterinaria_correo_pendiente");

        builder.HasIndex(i => i.VeterinariaId)
            .HasDatabaseName("ix_invitaciones_personal_veterinaria_id");

        builder.HasOne<Veterinaria>()
            .WithMany()
            .HasForeignKey(i => i.VeterinariaId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Usuario>()
            .WithMany()
            .HasForeignKey(i => i.CreadoPorUsuarioId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(i => i.Roles)
            .WithOne()
            .HasForeignKey(ir => ir.InvitacionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(nameof(InvitacionPersonal.Roles))
            .HasField("_roles")
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
