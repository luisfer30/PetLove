using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SaaS.Veterinario.Domain.Citas;
using SaaS.Veterinario.Domain.Clientes;
using SaaS.Veterinario.Domain.Consultas;
using SaaS.Veterinario.Domain.Mascotas;
using SaaS.Veterinario.Domain.Membresias;
using SaaS.Veterinario.Domain.Permisos;
using SaaS.Veterinario.Domain.Personal;
using SaaS.Veterinario.Domain.Roles;
using SaaS.Veterinario.Domain.Seguimientos;
using SaaS.Veterinario.Domain.Tratamientos;
using SaaS.Veterinario.Domain.Usuarios;
using SaaS.Veterinario.Domain.Veterinarias;
using SaaS.Veterinario.Infrastructure.Identidad;

namespace SaaS.Veterinario.Infrastructure.Persistencia;

/// <summary>
/// Hereda de IdentityUserContext (no IdentityDbContext) a proposito: la autorizacion de
/// esta plataforma se basa en Rol/Permiso de dominio (via MembresiaRol/RolPermiso), nunca
/// en el sistema de Roles propio de ASP.NET Identity, asi que no tiene sentido crear las
/// tablas AspNetRoles/AspNetUserRoles/AspNetRoleClaims que nadie va a usar.
/// </summary>
public sealed class SaaSVeterinarioDbContext(DbContextOptions<SaaSVeterinarioDbContext> options)
    : IdentityUserContext<UsuarioIdentidad, Guid>(options)
{
    public DbSet<Usuario> Usuarios => Set<Usuario>();

    public DbSet<Veterinaria> Veterinarias => Set<Veterinaria>();

    public DbSet<MembresiaVeterinaria> MembresiasVeterinaria => Set<MembresiaVeterinaria>();

    public DbSet<Rol> Roles => Set<Rol>();

    public DbSet<Permiso> Permisos => Set<Permiso>();

    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    public DbSet<InvitacionPersonal> InvitacionesPersonal => Set<InvitacionPersonal>();

    public DbSet<Especie> Especies => Set<Especie>();

    public DbSet<Raza> Razas => Set<Raza>();

    public DbSet<Mascota> Mascotas => Set<Mascota>();

    public DbSet<ClienteVeterinaria> ClientesVeterinaria => Set<ClienteVeterinaria>();

    public DbSet<MascotaVeterinaria> MascotasVeterinaria => Set<MascotaVeterinaria>();

    public DbSet<ResponsableMascota> ResponsablesMascota => Set<ResponsableMascota>();

    public DbSet<Cita> Citas => Set<Cita>();

    public DbSet<ConsultaVeterinaria> Consultas => Set<ConsultaVeterinaria>();

    public DbSet<DiagnosticoConsulta> DiagnosticosConsulta => Set<DiagnosticoConsulta>();

    public DbSet<PlanTratamiento> PlanesTratamiento => Set<PlanTratamiento>();

    public DbSet<ItemTratamiento> ItemsTratamiento => Set<ItemTratamiento>();

    public DbSet<ProgramacionTratamiento> ProgramacionesTratamiento => Set<ProgramacionTratamiento>();

    public DbSet<SeguimientoClinico> SeguimientosClinicos => Set<SeguimientoClinico>();

    // HorarioItemTratamiento no se expone como DbSet propio: es hijo de ItemTratamiento (mismo
    // patron que Roles en InvitacionPersonal), creado solo dentro de ItemTratamiento.Crear().

    // MembresiaRol, RolPermiso e InvitacionPersonalRol no se exponen como DbSet propio a
    // proposito: son entidades hijas de sus respectivos agregados raiz (MembresiaVeterinaria,
    // Rol, InvitacionPersonal) y se manipulan solo a traves de ellos (AsignarRol/AsignarPermiso).
    // Igual quedan mapeadas en el modelo mediante las relaciones configuradas en sus
    // IEntityTypeConfiguration.

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Simples renombres a snake_case de las tablas hijas que Identity crea por
        // convencion (AspNetUserClaims/Logins/Tokens); no ameritan su propio
        // IEntityTypeConfiguration porque no hay nada mas que configurar en ellas.
        modelBuilder.Entity<Microsoft.AspNetCore.Identity.IdentityUserClaim<Guid>>().ToTable("usuarios_identidad_claims");
        modelBuilder.Entity<Microsoft.AspNetCore.Identity.IdentityUserLogin<Guid>>().ToTable("usuarios_identidad_logins");
        modelBuilder.Entity<Microsoft.AspNetCore.Identity.IdentityUserToken<Guid>>().ToTable("usuarios_identidad_tokens");

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(SaaSVeterinarioDbContext).Assembly);
    }
}
