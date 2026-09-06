using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SaaS.Veterinario.Domain.Mascotas;
using SaaS.Veterinario.Domain.Tratamientos;
using SaaS.Veterinario.Domain.Usuarios;
using SaaS.Veterinario.Domain.Veterinarias;
using SaaS.Veterinario.Infrastructure.Persistencia.Conversiones;

namespace SaaS.Veterinario.Infrastructure.Persistencia.Configuraciones;

public sealed class ProgramacionTratamientoConfiguration : IEntityTypeConfiguration<ProgramacionTratamiento>
{
    public void Configure(EntityTypeBuilder<ProgramacionTratamiento> builder)
    {
        builder.ToTable("programaciones_tratamiento");

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

        builder.Property(p => p.PlanTratamientoId)
            .HasColumnName("plan_tratamiento_id")
            .IsRequired();

        builder.Property(p => p.ItemTratamientoId)
            .HasColumnName("item_tratamiento_id")
            .IsRequired();

        builder.Property(p => p.FechaHoraProgramada)
            .HasColumnName("fecha_hora_programada")
            .IsRequired();

        builder.Property(p => p.Estado)
            .HasColumnName("estado")
            .HasConversion(ConversionesEstadosTratamiento.ConversorEstadoProgramacionTratamiento)
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(p => p.FechaRealizacion)
            .HasColumnName("fecha_realizacion");

        builder.Property(p => p.RegistradoPorUsuarioId)
            .HasColumnName("registrado_por_usuario_id");

        builder.Property(p => p.ObservacionRealizacion)
            .HasColumnName("observacion_realizacion")
            .HasMaxLength(500);

        builder.Property(p => p.FechaCreacion)
            .HasColumnName("fecha_creacion")
            .IsRequired();

        builder.Property(p => p.FechaActualizacion)
            .HasColumnName("fecha_actualizacion")
            .IsRequired();

        // Protege la generacion idempotente (seccion 52): nunca dos programaciones del mismo
        // item en la misma fecha/hora exacta.
        builder.HasIndex(p => new { p.ItemTratamientoId, p.FechaHoraProgramada })
            .IsUnique()
            .HasDatabaseName("ux_programaciones_tratamiento_item_fecha");

        builder.HasIndex(p => new { p.VeterinariaId, p.FechaHoraProgramada })
            .HasDatabaseName("ix_programaciones_tratamiento_veterinaria_fecha");

        builder.HasIndex(p => new { p.PlanTratamientoId, p.Estado })
            .HasDatabaseName("ix_programaciones_tratamiento_plan_estado");

        builder.HasOne<Veterinaria>().WithMany().HasForeignKey(p => p.VeterinariaId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<MascotaVeterinaria>().WithMany().HasForeignKey(p => p.MascotaVeterinariaId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<PlanTratamiento>().WithMany().HasForeignKey(p => p.PlanTratamientoId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<ItemTratamiento>().WithMany().HasForeignKey(p => p.ItemTratamientoId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Usuario>().WithMany().HasForeignKey(p => p.RegistradoPorUsuarioId).OnDelete(DeleteBehavior.Restrict);
    }
}
