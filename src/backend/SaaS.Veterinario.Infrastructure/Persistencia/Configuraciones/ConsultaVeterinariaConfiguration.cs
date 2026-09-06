using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SaaS.Veterinario.Domain.Citas;
using SaaS.Veterinario.Domain.Consultas;
using SaaS.Veterinario.Domain.Mascotas;
using SaaS.Veterinario.Domain.Usuarios;
using SaaS.Veterinario.Domain.Veterinarias;
using SaaS.Veterinario.Infrastructure.Persistencia.Conversiones;

namespace SaaS.Veterinario.Infrastructure.Persistencia.Configuraciones;

public sealed class ConsultaVeterinariaConfiguration : IEntityTypeConfiguration<ConsultaVeterinaria>
{
    public void Configure(EntityTypeBuilder<ConsultaVeterinaria> builder)
    {
        builder.ToTable("consultas");

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

        builder.Property(c => c.CitaId)
            .HasColumnName("cita_id");

        builder.Property(c => c.VeterinarioUsuarioId)
            .HasColumnName("veterinario_usuario_id")
            .IsRequired();

        builder.Property(c => c.FechaHora)
            .HasColumnName("fecha_hora")
            .IsRequired();

        builder.Property(c => c.MotivoConsulta)
            .HasColumnName("motivo_consulta")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(c => c.Peso)
            .HasColumnName("peso")
            .HasPrecision(6, 2);

        builder.Property(c => c.Temperatura)
            .HasColumnName("temperatura")
            .HasPrecision(4, 1);

        builder.Property(c => c.FrecuenciaCardiaca)
            .HasColumnName("frecuencia_cardiaca");

        builder.Property(c => c.ObservacionesClinicas)
            .HasColumnName("observaciones_clinicas")
            .HasMaxLength(2000);

        builder.Property(c => c.IndicacionesPropietario)
            .HasColumnName("indicaciones_propietario")
            .HasMaxLength(2000);

        builder.Property(c => c.ProximaFechaControl)
            .HasColumnName("proxima_fecha_control");

        builder.Property(c => c.MotivoProximoControl)
            .HasColumnName("motivo_proximo_control")
            .HasMaxLength(500);

        builder.Property(c => c.Estado)
            .HasColumnName("estado")
            .HasConversion(ConversionesEstadosClinico.ConversorEstadoConsulta)
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

        builder.Property(c => c.FechaFinalizacion)
            .HasColumnName("fecha_finalizacion");

        builder.Property(c => c.FinalizadoPorUsuarioId)
            .HasColumnName("finalizado_por_usuario_id");

        builder.Property(c => c.FechaAnulacion)
            .HasColumnName("fecha_anulacion");

        builder.Property(c => c.AnuladoPorUsuarioId)
            .HasColumnName("anulado_por_usuario_id");

        builder.Property(c => c.MotivoAnulacion)
            .HasColumnName("motivo_anulacion")
            .HasMaxLength(500);

        builder.HasIndex(c => new { c.VeterinariaId, c.MascotaVeterinariaId, c.FechaHora })
            .HasDatabaseName("ix_consultas_veterinaria_mascota_veterinaria_fecha");

        builder.HasIndex(c => new { c.VeterinariaId, c.VeterinarioUsuarioId, c.FechaHora })
            .HasDatabaseName("ix_consultas_veterinaria_veterinario_fecha");

        builder.HasOne<Veterinaria>()
            .WithMany()
            .HasForeignKey(c => c.VeterinariaId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<MascotaVeterinaria>()
            .WithMany()
            .HasForeignKey(c => c.MascotaVeterinariaId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Cita>()
            .WithMany()
            .HasForeignKey(c => c.CitaId)
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

        builder.HasOne<Usuario>()
            .WithMany()
            .HasForeignKey(c => c.FinalizadoPorUsuarioId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Usuario>()
            .WithMany()
            .HasForeignKey(c => c.AnuladoPorUsuarioId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
