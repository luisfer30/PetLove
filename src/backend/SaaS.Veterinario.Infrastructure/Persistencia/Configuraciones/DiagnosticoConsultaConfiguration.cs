using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SaaS.Veterinario.Domain.Consultas;
using SaaS.Veterinario.Infrastructure.Persistencia.Conversiones;

namespace SaaS.Veterinario.Infrastructure.Persistencia.Configuraciones;

public sealed class DiagnosticoConsultaConfiguration : IEntityTypeConfiguration<DiagnosticoConsulta>
{
    public void Configure(EntityTypeBuilder<DiagnosticoConsulta> builder)
    {
        builder.ToTable("diagnosticos_consulta");

        builder.HasKey(d => d.Id);

        builder.Property(d => d.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(d => d.ConsultaId)
            .HasColumnName("consulta_id")
            .IsRequired();

        builder.Property(d => d.Descripcion)
            .HasColumnName("descripcion")
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(d => d.Tipo)
            .HasColumnName("tipo")
            .HasConversion(ConversionesEstadosClinico.ConversorTipoDiagnostico)
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(d => d.EsPrincipal)
            .HasColumnName("es_principal")
            .IsRequired();

        builder.Property(d => d.FechaCreacion)
            .HasColumnName("fecha_creacion")
            .IsRequired();

        builder.HasIndex(d => d.ConsultaId)
            .HasDatabaseName("ix_diagnosticos_consulta_consulta_id");

        // Segunda barrera de "maximo un diagnostico principal por consulta" (seccion 21); la
        // primera y principal es la comprobacion explicita en Application, mismo patron que el
        // responsable principal de la Etapa 6.
        builder.HasIndex(d => d.ConsultaId)
            .IsUnique()
            .HasDatabaseName("ux_diagnosticos_consulta_principal")
            .HasFilter("es_principal = true");

        builder.HasOne<ConsultaVeterinaria>()
            .WithMany()
            .HasForeignKey(d => d.ConsultaId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
