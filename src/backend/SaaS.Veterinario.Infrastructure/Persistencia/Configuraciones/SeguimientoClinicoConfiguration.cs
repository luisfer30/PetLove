using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SaaS.Veterinario.Domain.Citas;
using SaaS.Veterinario.Domain.Consultas;
using SaaS.Veterinario.Domain.Mascotas;
using SaaS.Veterinario.Domain.Seguimientos;
using SaaS.Veterinario.Domain.Tratamientos;
using SaaS.Veterinario.Domain.Usuarios;
using SaaS.Veterinario.Domain.Veterinarias;
using SaaS.Veterinario.Infrastructure.Persistencia.Conversiones;

namespace SaaS.Veterinario.Infrastructure.Persistencia.Configuraciones;

public sealed class SeguimientoClinicoConfiguration : IEntityTypeConfiguration<SeguimientoClinico>
{
    public void Configure(EntityTypeBuilder<SeguimientoClinico> builder)
    {
        builder.ToTable("seguimientos_clinicos");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(s => s.VeterinariaId)
            .HasColumnName("veterinaria_id")
            .IsRequired();

        builder.Property(s => s.MascotaVeterinariaId)
            .HasColumnName("mascota_veterinaria_id")
            .IsRequired();

        builder.Property(s => s.ConsultaOrigenId)
            .HasColumnName("consulta_origen_id");

        builder.Property(s => s.PlanTratamientoId)
            .HasColumnName("plan_tratamiento_id");

        builder.Property(s => s.Tipo)
            .HasColumnName("tipo")
            .HasConversion(ConversionesEstadosTratamiento.ConversorTipoSeguimientoClinico)
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(s => s.FechaObjetivo)
            .HasColumnName("fecha_objetivo")
            .IsRequired();

        builder.Property(s => s.Estado)
            .HasColumnName("estado")
            .HasConversion(ConversionesEstadosTratamiento.ConversorEstadoSeguimientoClinico)
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(s => s.Motivo)
            .HasColumnName("motivo")
            .HasMaxLength(300)
            .IsRequired();

        builder.Property(s => s.Notas)
            .HasColumnName("notas")
            .HasMaxLength(1000);

        builder.Property(s => s.FechaAgendada)
            .HasColumnName("fecha_agendada");

        builder.Property(s => s.CitaId)
            .HasColumnName("cita_id");

        builder.Property(s => s.FechaRealizacion)
            .HasColumnName("fecha_realizacion");

        builder.Property(s => s.EsGeneradoDesdeProximoControl)
            .HasColumnName("es_generado_desde_proximo_control")
            .IsRequired();

        builder.Property(s => s.CreadoPorUsuarioId)
            .HasColumnName("creado_por_usuario_id")
            .IsRequired();

        builder.Property(s => s.FechaCreacion)
            .HasColumnName("fecha_creacion")
            .IsRequired();

        builder.Property(s => s.ActualizadoPorUsuarioId)
            .HasColumnName("actualizado_por_usuario_id");

        builder.Property(s => s.FechaActualizacion)
            .HasColumnName("fecha_actualizacion")
            .IsRequired();

        builder.HasIndex(s => new { s.VeterinariaId, s.MascotaVeterinariaId, s.FechaObjetivo })
            .HasDatabaseName("ix_seguimientos_clinicos_veterinaria_mascota_veterinaria_fecha");

        // Idempotencia del seguimiento automatico desde ProximaFechaControl (seccion 36/53):
        // maximo uno por Consulta origen marcado como generado automaticamente; no impide que
        // existan multiples seguimientos MANUALES asociados a la misma consulta.
        builder.HasIndex(s => s.ConsultaOrigenId)
            .IsUnique()
            .HasDatabaseName("ux_seguimientos_clinicos_consulta_origen_auto_generado")
            .HasFilter("es_generado_desde_proximo_control = true");

        builder.HasOne<Veterinaria>().WithMany().HasForeignKey(s => s.VeterinariaId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<MascotaVeterinaria>().WithMany().HasForeignKey(s => s.MascotaVeterinariaId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<ConsultaVeterinaria>().WithMany().HasForeignKey(s => s.ConsultaOrigenId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<PlanTratamiento>().WithMany().HasForeignKey(s => s.PlanTratamientoId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Cita>().WithMany().HasForeignKey(s => s.CitaId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Usuario>().WithMany().HasForeignKey(s => s.CreadoPorUsuarioId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Usuario>().WithMany().HasForeignKey(s => s.ActualizadoPorUsuarioId).OnDelete(DeleteBehavior.Restrict);
    }
}
