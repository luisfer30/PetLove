using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SaaS.Veterinario.Domain.Tratamientos;

namespace SaaS.Veterinario.Infrastructure.Persistencia.Configuraciones;

public sealed class HorarioItemTratamientoConfiguration : IEntityTypeConfiguration<HorarioItemTratamiento>
{
    public void Configure(EntityTypeBuilder<HorarioItemTratamiento> builder)
    {
        builder.ToTable("horarios_items_tratamiento");

        builder.HasKey(h => h.Id);

        builder.Property(h => h.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(h => h.ItemTratamientoId)
            .HasColumnName("item_tratamiento_id")
            .IsRequired();

        builder.Property(h => h.Hora)
            .HasColumnName("hora")
            .IsRequired();

        builder.Property(h => h.Orden)
            .HasColumnName("orden")
            .IsRequired();

        builder.HasIndex(h => new { h.ItemTratamientoId, h.Hora })
            .IsUnique()
            .HasDatabaseName("ux_horarios_items_tratamiento_item_hora");
    }
}
