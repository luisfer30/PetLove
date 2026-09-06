using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SaaS.Veterinario.Domain.Citas;
using SaaS.Veterinario.Domain.Clientes;
using SaaS.Veterinario.Domain.Mascotas;
using SaaS.Veterinario.Domain.Usuarios;
using SaaS.Veterinario.Domain.Veterinarias;
using SaaS.Veterinario.Infrastructure.Persistencia.Conversiones;

namespace SaaS.Veterinario.Infrastructure.Persistencia.Configuraciones;

public sealed class CitaConfiguration : IEntityTypeConfiguration<Cita>
{
    public void Configure(EntityTypeBuilder<Cita> builder)
    {
        builder.ToTable("citas");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(c => c.VeterinariaId)
            .HasColumnName("veterinaria_id")
            .IsRequired();

        builder.Property(c => c.MascotaVeterinariaId)
            .HasColumnName("mascota_veterinaria_id")
            .IsRequired();

        builder.Property(c => c.ClienteVeterinariaId)
            .HasColumnName("cliente_veterinaria_id");

        builder.Property(c => c.VeterinarioUsuarioId)
            .HasColumnName("veterinario_usuario_id");

        builder.Property(c => c.FechaHoraInicio)
            .HasColumnName("fecha_hora_inicio")
            .IsRequired();

        builder.Property(c => c.FechaHoraFin)
            .HasColumnName("fecha_hora_fin")
            .IsRequired();

        builder.Property(c => c.Motivo)
            .HasColumnName("motivo")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(c => c.Observaciones)
            .HasColumnName("observaciones")
            .HasMaxLength(500);

        builder.Property(c => c.Estado)
            .HasColumnName("estado")
            .HasConversion(ConversionesEstadosClinico.ConversorEstadoCita)
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(c => c.FechaCreacion)
            .HasColumnName("fecha_creacion")
            .IsRequired();

        builder.Property(c => c.CreadoPorUsuarioId)
            .HasColumnName("creado_por_usuario_id")
            .IsRequired();

        builder.Property(c => c.FechaActualizacion)
            .HasColumnName("fecha_actualizacion")
            .IsRequired();

        builder.Property(c => c.ActualizadoPorUsuarioId)
            .HasColumnName("actualizado_por_usuario_id");

        builder.HasIndex(c => new { c.VeterinariaId, c.FechaHoraInicio })
            .HasDatabaseName("ix_citas_veterinaria_fecha_inicio");

        builder.HasIndex(c => new { c.VeterinariaId, c.VeterinarioUsuarioId, c.FechaHoraInicio })
            .HasDatabaseName("ix_citas_veterinaria_veterinario_fecha_inicio");

        builder.HasIndex(c => new { c.MascotaVeterinariaId, c.FechaHoraInicio })
            .HasDatabaseName("ix_citas_mascota_veterinaria_fecha_inicio");

        builder.HasOne<Veterinaria>()
            .WithMany()
            .HasForeignKey(c => c.VeterinariaId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<MascotaVeterinaria>()
            .WithMany()
            .HasForeignKey(c => c.MascotaVeterinariaId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<ClienteVeterinaria>()
            .WithMany()
            .HasForeignKey(c => c.ClienteVeterinariaId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Usuario>()
            .WithMany()
            .HasForeignKey(c => c.VeterinarioUsuarioId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Usuario>()
            .WithMany()
            .HasForeignKey(c => c.CreadoPorUsuarioId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Usuario>()
            .WithMany()
            .HasForeignKey(c => c.ActualizadoPorUsuarioId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
