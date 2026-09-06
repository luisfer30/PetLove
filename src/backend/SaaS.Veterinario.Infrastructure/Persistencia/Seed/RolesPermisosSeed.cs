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

    // Etapa 6: Clientes + Mascotas. IDs nuevos que continuan la misma serie -- las filas
    // anteriores (000001-000006) no se tocan, esta migracion solo agrega filas nuevas.
    public static readonly Guid PermisoClientesVerId = new("22222222-0000-0000-0000-000000000007");
    public static readonly Guid PermisoClientesCrearId = new("22222222-0000-0000-0000-000000000008");
    public static readonly Guid PermisoClientesEditarId = new("22222222-0000-0000-0000-000000000009");
    public static readonly Guid PermisoMascotasVerId = new("22222222-0000-0000-0000-000000000010");
    public static readonly Guid PermisoMascotasCrearId = new("22222222-0000-0000-0000-000000000011");
    public static readonly Guid PermisoMascotasEditarId = new("22222222-0000-0000-0000-000000000012");
    public static readonly Guid PermisoResponsablesVerId = new("22222222-0000-0000-0000-000000000013");
    public static readonly Guid PermisoResponsablesCrearId = new("22222222-0000-0000-0000-000000000014");
    public static readonly Guid PermisoResponsablesEditarId = new("22222222-0000-0000-0000-000000000015");

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
        new { Id = PermisoClientesVerId, Codigo = "clientes.ver", Nombre = "Ver clientes", Descripcion = (string?)null },
        new { Id = PermisoClientesCrearId, Codigo = "clientes.crear", Nombre = "Crear clientes", Descripcion = (string?)null },
        new { Id = PermisoClientesEditarId, Codigo = "clientes.editar", Nombre = "Editar clientes", Descripcion = (string?)null },
        new { Id = PermisoMascotasVerId, Codigo = "mascotas.ver", Nombre = "Ver mascotas", Descripcion = (string?)null },
        new { Id = PermisoMascotasCrearId, Codigo = "mascotas.crear", Nombre = "Crear mascotas", Descripcion = (string?)null },
        new { Id = PermisoMascotasEditarId, Codigo = "mascotas.editar", Nombre = "Editar mascotas", Descripcion = (string?)null },
        new { Id = PermisoResponsablesVerId, Codigo = "responsables.ver", Nombre = "Ver responsables", Descripcion = (string?)null },
        new { Id = PermisoResponsablesCrearId, Codigo = "responsables.crear", Nombre = "Crear responsables", Descripcion = (string?)null },
        new { Id = PermisoResponsablesEditarId, Codigo = "responsables.editar", Nombre = "Editar responsables", Descripcion = (string?)null },
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

        // Etapa 6 -- ADMINISTRADOR_VETERINARIA: todos los permisos nuevos.
        new { RolId = RolAdministradorVeterinariaId, PermisoId = PermisoClientesVerId },
        new { RolId = RolAdministradorVeterinariaId, PermisoId = PermisoClientesCrearId },
        new { RolId = RolAdministradorVeterinariaId, PermisoId = PermisoClientesEditarId },
        new { RolId = RolAdministradorVeterinariaId, PermisoId = PermisoMascotasVerId },
        new { RolId = RolAdministradorVeterinariaId, PermisoId = PermisoMascotasCrearId },
        new { RolId = RolAdministradorVeterinariaId, PermisoId = PermisoMascotasEditarId },
        new { RolId = RolAdministradorVeterinariaId, PermisoId = PermisoResponsablesVerId },
        new { RolId = RolAdministradorVeterinariaId, PermisoId = PermisoResponsablesCrearId },
        new { RolId = RolAdministradorVeterinariaId, PermisoId = PermisoResponsablesEditarId },

        // Etapa 6 -- VETERINARIO.
        new { RolId = RolVeterinarioId, PermisoId = PermisoClientesVerId },
        new { RolId = RolVeterinarioId, PermisoId = PermisoMascotasVerId },
        new { RolId = RolVeterinarioId, PermisoId = PermisoMascotasCrearId },
        new { RolId = RolVeterinarioId, PermisoId = PermisoMascotasEditarId },
        new { RolId = RolVeterinarioId, PermisoId = PermisoResponsablesVerId },
        new { RolId = RolVeterinarioId, PermisoId = PermisoResponsablesCrearId },

        // Etapa 6 -- ASISTENTE: todos los permisos nuevos (tarea de recepcion/administrativa).
        new { RolId = RolAsistenteId, PermisoId = PermisoClientesVerId },
        new { RolId = RolAsistenteId, PermisoId = PermisoClientesCrearId },
        new { RolId = RolAsistenteId, PermisoId = PermisoClientesEditarId },
        new { RolId = RolAsistenteId, PermisoId = PermisoMascotasVerId },
        new { RolId = RolAsistenteId, PermisoId = PermisoMascotasCrearId },
        new { RolId = RolAsistenteId, PermisoId = PermisoMascotasEditarId },
        new { RolId = RolAsistenteId, PermisoId = PermisoResponsablesVerId },
        new { RolId = RolAsistenteId, PermisoId = PermisoResponsablesCrearId },
        new { RolId = RolAsistenteId, PermisoId = PermisoResponsablesEditarId },
    ];
}
