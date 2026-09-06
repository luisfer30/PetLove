using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SaaS.Veterinario.Domain.Tratamientos;
using SaaS.Veterinario.Infrastructure.Persistencia.Conversiones;

namespace SaaS.Veterinario.Infrastructure.Persistencia.Configuraciones;

public sealed class ItemTratamientoConfiguration : IEntityTypeConfiguration<ItemTratamiento>
{
    public void Configure(EntityTypeBuilder<ItemTratamiento> builder)
    {
        builder.ToTable("items_tratamiento");

        builder.HasKey(i => i.Id);

        builder.Property(i => i.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(i => i.PlanTratamientoId)
            .HasColumnName("plan_tratamiento_id")
            .IsRequired();

        builder.Property(i => i.Tipo)
            .HasColumnName("tipo")
            .HasConversion(ConversionesEstadosTratamiento.ConversorTipoItemTratamiento)
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(i => i.Nombre)
            .HasColumnName("nombre")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(i => i.Descripcion)
            .HasColumnName("descripcion")
            .HasMaxLength(1000);

        builder.Property(i => i.DosisCantidad)
            .HasColumnName("dosis_cantidad")
            .HasPrecision(8, 3);

        builder.Property(i => i.DosisUnidad)
            .HasColumnName("dosis_unidad")
            .HasMaxLength(50);

        builder.Property(i => i.ViaAdministracion)
            .HasColumnName("via_administracion")
            .HasConversion(ConversionesEstadosTratamiento.ConversorViaAdministracion)
            .HasMaxLength(20);

        builder.Property(i => i.FrecuenciaTipo)
            .HasColumnName("frecuencia_tipo")
            .HasConversion(ConversionesEstadosTratamiento.ConversorTipoFrecuenciaTratamiento)
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(i => i.IntervaloHoras)
            .HasColumnName("intervalo_horas");

        builder.Property(i => i.VecesPorDia)
            .HasColumnName("veces_por_dia");

        builder.Property(i => i.FechaInicio)
            .HasColumnName("fecha_inicio")
            .IsRequired();

        builder.Property(i => i.FechaFin)
            .HasColumnName("fecha_fin");

        builder.Property(i => i.DuracionDias)
            .HasColumnName("duracion_dias");

        builder.Property(i => i.Instrucciones)
            .HasColumnName("instrucciones")
            .HasMaxLength(1000);

        builder.Ignore(i => i.EsSegunNecesidad);

        builder.Property(i => i.Estado)
            .HasColumnName("estado")
            .HasConversion(ConversionesEstadosTratamiento.ConversorEstadoItemTratamiento)
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(i => i.Orden)
            .HasColumnName("orden")
            .IsRequired();

        builder.Property(i => i.FechaCreacion)
            .HasColumnName("fecha_creacion")
            .IsRequired();

        builder.Property(i => i.FechaActualizacion)
            .HasColumnName("fecha_actualizacion")
            .IsRequired();

        builder.HasIndex(i => i.PlanTratamientoId)
            .HasDatabaseName("ix_items_tratamiento_plan_tratamiento_id");

        builder.HasOne<PlanTratamiento>()
            .WithMany()
            .HasForeignKey(i => i.PlanTratamientoId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(i => i.Horarios)
            .WithOne()
            .HasForeignKey(h => h.ItemTratamientoId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(nameof(ItemTratamiento.Horarios))
            .HasField("_horarios")
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
