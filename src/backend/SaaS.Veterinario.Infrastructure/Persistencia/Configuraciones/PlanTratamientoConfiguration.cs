using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SaaS.Veterinario.Domain.Consultas;
using SaaS.Veterinario.Domain.Mascotas;
using SaaS.Veterinario.Domain.Tratamientos;
using SaaS.Veterinario.Domain.Usuarios;
using SaaS.Veterinario.Domain.Veterinarias;
using SaaS.Veterinario.Infrastructure.Persistencia.Conversiones;

namespace SaaS.Veterinario.Infrastructure.Persistencia.Configuraciones;

public sealed class PlanTratamientoConfiguration : IEntityTypeConfiguration<PlanTratamiento>
{
    public void Configure(EntityTypeBuilder<PlanTratamiento> builder)
    {
        builder.ToTable("planes_tratamiento");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(p => p.VeterinariaId)
            .HasColumnName("veterinaria_id")
            .IsRequired();

        builder.Property(p => p.MascotaVeterinariaId)
            .HasColumnName("mascota_veterinaria_id")
            .IsRequired();

        builder.Property(p => p.ConsultaId)
            .HasColumnName("consulta_id");

        builder.Property(p => p.Nombre)
            .HasColumnName("nombre")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(p => p.Descripcion)
            .HasColumnName("descripcion")
            .HasMaxLength(1000);

        builder.Property(p => p.FechaInicio)
            .HasColumnName("fecha_inicio")
            .IsRequired();

        builder.Property(p => p.FechaFinEstimada)
            .HasColumnName("fecha_fin_estimada");

        builder.Property(p => p.Estado)
            .HasColumnName("estado")
            .HasConversion(ConversionesEstadosTratamiento.ConversorEstadoPlanTratamiento)
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(p => p.CreadoPorUsuarioId)
            .HasColumnName("creado_por_usuario_id")
            .IsRequired();

        builder.Property(p => p.FechaCreacion)
            .HasColumnName("fecha_creacion")
            .IsRequired();

        builder.Property(p => p.ActualizadoPorUsuarioId)
            .HasColumnName("actualizado_por_usuario_id");

        builder.Property(p => p.FechaActualizacion)
            .HasColumnName("fecha_actualizacion")
            .IsRequired();

        builder.Property(p => p.SuspendidoPorUsuarioId)
            .HasColumnName("suspendido_por_usuario_id");

        builder.Property(p => p.FechaSuspension)
            .HasColumnName("fecha_suspension");

        builder.Property(p => p.MotivoSuspension)
            .HasColumnName("motivo_suspension")
            .HasMaxLength(500);

        builder.Property(p => p.CompletadoPorUsuarioId)
            .HasColumnName("completado_por_usuario_id");

        builder.Property(p => p.FechaCompletado)
            .HasColumnName("fecha_completado");

        builder.Property(p => p.CanceladoPorUsuarioId)
            .HasColumnName("cancelado_por_usuario_id");

        builder.Property(p => p.FechaCancelacion)
            .HasColumnName("fecha_cancelacion");

        builder.Property(p => p.MotivoCancelacion)
            .HasColumnName("motivo_cancelacion")
            .HasMaxLength(500);

        builder.HasIndex(p => new { p.VeterinariaId, p.MascotaVeterinariaId })
            .HasDatabaseName("ix_planes_tratamiento_veterinaria_mascota_veterinaria");

        builder.HasIndex(p => new { p.VeterinariaId, p.Estado })
            .HasDatabaseName("ix_planes_tratamiento_veterinaria_estado");

        builder.HasOne<Veterinaria>().WithMany().HasForeignKey(p => p.VeterinariaId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<MascotaVeterinaria>().WithMany().HasForeignKey(p => p.MascotaVeterinariaId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<ConsultaVeterinaria>().WithMany().HasForeignKey(p => p.ConsultaId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Usuario>().WithMany().HasForeignKey(p => p.CreadoPorUsuarioId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Usuario>().WithMany().HasForeignKey(p => p.ActualizadoPorUsuarioId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Usuario>().WithMany().HasForeignKey(p => p.SuspendidoPorUsuarioId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Usuario>().WithMany().HasForeignKey(p => p.CompletadoPorUsuarioId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Usuario>().WithMany().HasForeignKey(p => p.CanceladoPorUsuarioId).OnDelete(DeleteBehavior.Restrict);
    }
}
