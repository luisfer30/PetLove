using SaaS.Veterinario.Domain.Roles;

namespace SaaS.Veterinario.Infrastructure.Persistencia.Seed;

/// <summary>
/// Datos de siembra deterministas para roles y permisos iniciales. Los Guid son fijos
/// (no Guid.NewGuid()) porque HasData los graba literalmente en la migracion y deben
/// ser estables entre entornos y entre regeneraciones de la migracion.
/// </summary>
internal static class RolesPermisosSeed
{
    public static readonly Guid RolAdministradorVeterinariaId = new("11111111-0000-0000-0000-000000000001");
    public static readonly Guid RolVeterinarioId = new("11111111-0000-0000-0000-000000000002");
    public static readonly Guid RolAsistenteId = new("11111111-0000-0000-0000-000000000003");

    public static readonly Guid PermisoVeterinariaVerId = new("22222222-0000-0000-0000-000000000001");
    public static readonly Guid PermisoVeterinariaEditarId = new("22222222-0000-0000-0000-000000000002");
    public static readonly Guid PermisoPersonalVerId = new("22222222-0000-0000-0000-000000000003");
    public static readonly Guid PermisoPersonalInvitarId = new("22222222-0000-0000-0000-000000000004");
    public static readonly Guid PermisoPersonalEditarId = new("22222222-0000-0000-0000-000000000005");
    public static readonly Guid PermisoPersonalFinalizarId = new("22222222-0000-0000-0000-000000000006");

    public static object[] Roles { get; } =
    [
        new
        {
            Id = RolAdministradorVeterinariaId,
            Codigo = "ADMINISTRADOR_VETERINARIA",
            Nombre = "Administrador de veterinaria",
            Descripcion = (string?)null,
            Estado = EstadoRol.Activo,
        },
        new
        {
            Id = RolVeterinarioId,
            Codigo = "VETERINARIO",
            Nombre = "Veterinario",
            Descripcion = (string?)null,
            Estado = EstadoRol.Activo,
        },
        new
        {
            Id = RolAsistenteId,
            Codigo = "ASISTENTE",
            Nombre = "Asistente",
            Descripcion = (string?)null,
            Estado = EstadoRol.Activo,
        },
    ];

    public static object[] Permisos { get; } =
    [
        new { Id = PermisoVeterinariaVerId, Codigo = "veterinaria.ver", Nombre = "Ver veterinaria", Descripcion = (string?)null },
        new { Id = PermisoVeterinariaEditarId, Codigo = "veterinaria.editar", Nombre = "Editar veterinaria", Descripcion = (string?)null },
        new { Id = PermisoPersonalVerId, Codigo = "personal.ver", Nombre = "Ver personal", Descripcion = (string?)null },
        new { Id = PermisoPersonalInvitarId, Codigo = "personal.invitar", Nombre = "Invitar personal", Descripcion = (string?)null },
        new { Id = PermisoPersonalEditarId, Codigo = "personal.editar", Nombre = "Editar personal", Descripcion = (string?)null },
        new { Id = PermisoPersonalFinalizarId, Codigo = "personal.finalizar", Nombre = "Finalizar personal", Descripcion = (string?)null },
    ];

    public static object[] RolesPermisos { get; } =
    [
        // ADMINISTRADOR_VETERINARIA: todos los permisos iniciales.
        new { RolId = RolAdministradorVeterinariaId, PermisoId = PermisoVeterinariaVerId },
        new { RolId = RolAdministradorVeterinariaId, PermisoId = PermisoVeterinariaEditarId },
        new { RolId = RolAdministradorVeterinariaId, PermisoId = PermisoPersonalVerId },
        new { RolId = RolAdministradorVeterinariaId, PermisoId = PermisoPersonalInvitarId },
        new { RolId = RolAdministradorVeterinariaId, PermisoId = PermisoPersonalEditarId },
        new { RolId = RolAdministradorVeterinariaId, PermisoId = PermisoPersonalFinalizarId },

        // VETERINARIO: solo lectura de la veterinaria por ahora.
        new { RolId = RolVeterinarioId, PermisoId = PermisoVeterinariaVerId },

        // ASISTENTE: solo lectura de la veterinaria por ahora.
        new { RolId = RolAsistenteId, PermisoId = PermisoVeterinariaVerId },
    ];
}
