using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SaaS.Veterinario.Domain.Clientes;
using SaaS.Veterinario.Domain.Usuarios;
using SaaS.Veterinario.Domain.Veterinarias;
using SaaS.Veterinario.Infrastructure.Persistencia.Conversiones;

namespace SaaS.Veterinario.Infrastructure.Persistencia.Configuraciones;

/// <summary>
/// Pertenece a exactamente una Veterinaria (R08) y puede existir sin Usuario global (R09).
/// Decision explicita del usuario: NO se exige unicidad de correo entre distintos
/// ClienteVeterinaria (se permiten correos repetidos dentro de la misma veterinaria).
/// </summary>
public sealed class ClienteVeterinariaConfiguration : IEntityTypeConfiguration<ClienteVeterinaria>
{
    public void Configure(EntityTypeBuilder<ClienteVeterinaria> builder)
    {
        builder.ToTable("clientes_veterinaria");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(c => c.VeterinariaId)
            .HasColumnName("veterinaria_id")
            .IsRequired();

        builder.Property(c => c.UsuarioId)
            .HasColumnName("usuario_id");

        builder.Property(c => c.Nombre)
            .HasColumnName("nombre")
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(c => c.Correo)
            .HasColumnName("correo")
            .HasConversion(ConversionesValorObjetos.ConversorCorreoNullable, ConversionesValorObjetos.ComparadorCorreoNullable)
            .HasMaxLength(320);

        builder.Property(c => c.Telefono)
            .HasColumnName("telefono")
            .HasMaxLength(30);

        builder.Property(c => c.Observaciones)
            .HasColumnName("observaciones")
            .HasMaxLength(500);

        builder.Property(c => c.Estado)
            .HasColumnName("estado")
            .HasConversion(ConversionesEstadosMascotas.ConversorEstadoClienteVeterinaria)
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(c => c.FechaCreacion)
            .HasColumnName("fecha_creacion")
            .IsRequired();

        builder.Property(c => c.FechaActualizacion)
            .HasColumnName("fecha_actualizacion")
            .IsRequired();

        builder.Property(c => c.FechaEliminacion)
            .HasColumnName("fecha_eliminacion");

        builder.HasIndex(c => c.VeterinariaId)
            .HasDatabaseName("ix_clientes_veterinaria_veterinaria_id");

        builder.HasOne<Veterinaria>()
            .WithMany()
            .HasForeignKey(c => c.VeterinariaId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Usuario>()
            .WithMany()
            .HasForeignKey(c => c.UsuarioId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
