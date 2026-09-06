using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SaaS.Veterinario.Domain.Clientes;
using SaaS.Veterinario.Domain.Mascotas;
using SaaS.Veterinario.Domain.Usuarios;
using SaaS.Veterinario.Domain.Veterinarias;
using SaaS.Veterinario.Infrastructure.Persistencia.Conversiones;

namespace SaaS.Veterinario.Infrastructure.Persistencia.Configuraciones;

/// <summary>
/// Pertenece al contexto de UNA veterinaria y UNA MascotaVeterinaria; nunca es global (R11).
/// Solo puede existir un responsable principal activo por MascotaVeterinaria.
/// </summary>
public sealed class ResponsableMascotaConfiguration : IEntityTypeConfiguration<ResponsableMascota>
{
    public void Configure(EntityTypeBuilder<ResponsableMascota> builder)
    {
        builder.ToTable("responsables_mascota");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(r => r.VeterinariaId)
            .HasColumnName("veterinaria_id")
            .IsRequired();

        builder.Property(r => r.MascotaVeterinariaId)
            .HasColumnName("mascota_veterinaria_id")
            .IsRequired();

        builder.Property(r => r.ClienteVeterinariaId)
            .HasColumnName("cliente_veterinaria_id");

        builder.Property(r => r.UsuarioId)
            .HasColumnName("usuario_id");

        builder.Property(r => r.Nombre)
            .HasColumnName("nombre")
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(r => r.Correo)
            .HasColumnName("correo")
            .HasConversion(ConversionesValorObjetos.ConversorCorreoNullable, ConversionesValorObjetos.ComparadorCorreoNullable)
            .HasMaxLength(320);

        builder.Property(r => r.Telefono)
            .HasColumnName("telefono")
            .HasMaxLength(30);

        builder.Property(r => r.TipoRelacion)
            .HasColumnName("tipo_relacion")
            .HasConversion(ConversionesEstadosMascotas.ConversorTipoRelacionResponsable)
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(r => r.EsPrincipal)
            .HasColumnName("es_principal")
            .IsRequired();

        builder.Property(r => r.PuedeVerHistorial)
            .HasColumnName("puede_ver_historial")
            .IsRequired();

        builder.Property(r => r.PuedeGestionarTratamientos)
            .HasColumnName("puede_gestionar_tratamientos")
            .IsRequired();

        builder.Property(r => r.EstadoVinculacion)
            .HasColumnName("estado_vinculacion")
            .HasConversion(ConversionesEstadosMascotas.ConversorEstadoVinculacionResponsable)
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(r => r.Estado)
            .HasColumnName("estado")
            .HasConversion(ConversionesEstadosMascotas.ConversorEstadoResponsableMascota)
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(r => r.FechaCreacion)
            .HasColumnName("fecha_creacion")
            .IsRequired();

        builder.Property(r => r.FechaActualizacion)
            .HasColumnName("fecha_actualizacion")
            .IsRequired();

        builder.HasIndex(r => r.VeterinariaId)
            .HasDatabaseName("ix_responsables_mascota_veterinaria_id");

        builder.HasIndex(r => r.MascotaVeterinariaId)
            .HasDatabaseName("ix_responsables_mascota_mascota_veterinaria_id");

        builder.HasIndex(r => r.MascotaVeterinariaId)
            .IsUnique()
            .HasDatabaseName("ux_responsables_mascota_principal_activo")
            .HasFilter("es_principal AND estado = 'ACTIVO'");

        builder.HasOne<Veterinaria>()
            .WithMany()
            .HasForeignKey(r => r.VeterinariaId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<MascotaVeterinaria>()
            .WithMany()
            .HasForeignKey(r => r.MascotaVeterinariaId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<ClienteVeterinaria>()
            .WithMany()
            .HasForeignKey(r => r.ClienteVeterinariaId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Usuario>()
            .WithMany()
            .HasForeignKey(r => r.UsuarioId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
