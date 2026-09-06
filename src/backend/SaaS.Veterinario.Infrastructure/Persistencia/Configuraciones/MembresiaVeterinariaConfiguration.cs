using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SaaS.Veterinario.Domain.Membresias;
using SaaS.Veterinario.Domain.Usuarios;
using SaaS.Veterinario.Domain.Veterinarias;
using SaaS.Veterinario.Infrastructure.Persistencia.Conversiones;

namespace SaaS.Veterinario.Infrastructure.Persistencia.Configuraciones;

public sealed class MembresiaVeterinariaConfiguration : IEntityTypeConfiguration<MembresiaVeterinaria>
{
    /// <summary>
    /// Segunda barrera (la primera es la maquina de estados del dominio) para la regla V1:
    /// un usuario nunca puede tener mas de una MembresiaVeterinaria con estado ACTIVA al
    /// mismo tiempo, aunque si puede tener muchas historicas en otros estados.
    /// </summary>
    public const string IndiceUnicoMembresiaActivaPorUsuario = "ux_membresias_veterinaria_usuario_activa";

    public void Configure(EntityTypeBuilder<MembresiaVeterinaria> builder)
    {
        builder.ToTable("membresias_veterinaria");

        builder.HasKey(m => m.Id);

        builder.Property(m => m.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(m => m.UsuarioId)
            .HasColumnName("usuario_id")
            .IsRequired();

        builder.Property(m => m.VeterinariaId)
            .HasColumnName("veterinaria_id")
            .IsRequired();

        builder.Property(m => m.Estado)
            .HasColumnName("estado")
            .HasConversion(ConversionesEstados.ConversorEstadoMembresia)
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(m => m.FechaIngreso)
            .HasColumnName("fecha_ingreso")
            .IsRequired();

        builder.Property(m => m.FechaFinalizacion)
            .HasColumnName("fecha_finalizacion");

        builder.Property(m => m.FechaCreacion)
            .HasColumnName("fecha_creacion")
            .IsRequired();

        builder.Property(m => m.FechaActualizacion)
            .HasColumnName("fecha_actualizacion")
            .IsRequired();

        // Usuario y Veterinaria no tienen navegacion de vuelta a MembresiaVeterinaria en el
        // dominio (es su propio agregado raiz), por eso WithMany() sin argumento.
        builder.HasOne<Usuario>()
            .WithMany()
            .HasForeignKey(m => m.UsuarioId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Veterinaria>()
            .WithMany()
            .HasForeignKey(m => m.VeterinariaId)
            .OnDelete(DeleteBehavior.Restrict);

        // Nota: se usa el overload HasIndex(expression, name) -- no HasDatabaseName() encadenado --
        // porque EF Core identifica un indice por su conjunto de propiedades salvo que se le de
        // un nombre explicito al momento de crearlo. Con HasDatabaseName() encadenado, la segunda
        // llamada a HasIndex(m => m.UsuarioId) reconfiguraba el MISMO indice en vez de crear uno
        // nuevo, y el indice de busqueda general se perdia silenciosamente.
        builder.HasIndex(m => m.UsuarioId, "ix_membresias_veterinaria_usuario_id");

        builder.HasIndex(m => m.VeterinariaId, "ix_membresias_veterinaria_veterinaria_id");

        // Indice UNIQUE parcial: UNIQUE(usuario_id) WHERE estado = 'ACTIVA'.
        builder.HasIndex(m => m.UsuarioId, IndiceUnicoMembresiaActivaPorUsuario)
            .IsUnique()
            .HasFilter("estado = 'ACTIVA'");

        // MembresiaVeterinaria es el agregado raiz de MembresiaRol: cascade es seguro aqui
        // porque solo limpia las filas hijas de ESTE agregado, nunca las de otro.
        builder.HasMany(m => m.Roles)
            .WithOne()
            .HasForeignKey(mr => mr.MembresiaId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(nameof(MembresiaVeterinaria.Roles))
            .HasField("_roles")
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
