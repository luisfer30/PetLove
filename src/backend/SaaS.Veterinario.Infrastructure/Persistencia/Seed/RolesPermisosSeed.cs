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

    // Etapa 7: Agenda + Consultas + Diagnosticos. IDs nuevos que continuan la misma serie --
    // las filas anteriores (000001-000015) no se tocan.
    public static readonly Guid PermisoCitasVerId = new("22222222-0000-0000-0000-000000000016");
    public static readonly Guid PermisoCitasCrearId = new("22222222-0000-0000-0000-000000000017");
    public static readonly Guid PermisoCitasEditarId = new("22222222-0000-0000-0000-000000000018");
    public static readonly Guid PermisoCitasCancelarId = new("22222222-0000-0000-0000-000000000019");
    public static readonly Guid PermisoCitasCambiarEstadoId = new("22222222-0000-0000-0000-000000000020");
    public static readonly Guid PermisoConsultasVerId = new("22222222-0000-0000-0000-000000000021");
    public static readonly Guid PermisoConsultasCrearId = new("22222222-0000-0000-0000-000000000022");
    public static readonly Guid PermisoConsultasEditarBorradorId = new("22222222-0000-0000-0000-000000000023");
    public static readonly Guid PermisoConsultasFinalizarId = new("22222222-0000-0000-0000-000000000024");
    public static readonly Guid PermisoConsultasAnularId = new("22222222-0000-0000-0000-000000000025");
    public static readonly Guid PermisoDiagnosticosVerId = new("22222222-0000-0000-0000-000000000026");
    public static readonly Guid PermisoDiagnosticosCrearId = new("22222222-0000-0000-0000-000000000027");
    public static readonly Guid PermisoDiagnosticosEditarId = new("22222222-0000-0000-0000-000000000028");
    public static readonly Guid PermisoHistorialVerId = new("22222222-0000-0000-0000-000000000029");

    // Ajuste post-Etapa 7: permiso dedicado exclusivamente a "puede ser seleccionado como
    // profesional veterinario responsable de una cita/consulta" -- separado de consultas.crear
    // porque significan cosas distintas (uno es la capacidad de operar el modulo, este es la
    // elegibilidad para ser asignado como el veterinario responsable). Continua la misma serie.
    public static readonly Guid PermisoClinicaEjercerVeterinarioId = new("22222222-0000-0000-0000-000000000030");

    // Etapa 8: Tratamientos + Programaciones + Seguimiento clinico. IDs nuevos que continuan la
    // misma serie -- las filas anteriores (000001-000030) no se tocan.
    public static readonly Guid PermisoTratamientosVerId = new("22222222-0000-0000-0000-000000000031");
    public static readonly Guid PermisoTratamientosCrearId = new("22222222-0000-0000-0000-000000000032");
    public static readonly Guid PermisoTratamientosEditarId = new("22222222-0000-0000-0000-000000000033");
    public static readonly Guid PermisoTratamientosActivarId = new("22222222-0000-0000-0000-000000000034");
    public static readonly Guid PermisoTratamientosSuspenderId = new("22222222-0000-0000-0000-000000000035");
    public static readonly Guid PermisoTratamientosCompletarId = new("22222222-0000-0000-0000-000000000036");
    public static readonly Guid PermisoTratamientosCancelarId = new("22222222-0000-0000-0000-000000000037");
    public static readonly Guid PermisoProgramacionesVerId = new("22222222-0000-0000-0000-000000000038");
    public static readonly Guid PermisoProgramacionesRegistrarRealizacionId = new("22222222-0000-0000-0000-000000000039");
    public static readonly Guid PermisoProgramacionesRegistrarOmisionId = new("22222222-0000-0000-0000-000000000040");
    public static readonly Guid PermisoSeguimientosVerId = new("22222222-0000-0000-0000-000000000041");
    public static readonly Guid PermisoSeguimientosCrearId = new("22222222-0000-0000-0000-000000000042");
    public static readonly Guid PermisoSeguimientosEditarId = new("22222222-0000-0000-0000-000000000043");
    public static readonly Guid PermisoSeguimientosRealizarId = new("22222222-0000-0000-0000-000000000044");
    public static readonly Guid PermisoSeguimientosCancelarId = new("22222222-0000-0000-0000-000000000045");

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
        new { Id = PermisoCitasVerId, Codigo = "citas.ver", Nombre = "Ver citas", Descripcion = (string?)null },
        new { Id = PermisoCitasCrearId, Codigo = "citas.crear", Nombre = "Crear citas", Descripcion = (string?)null },
        new { Id = PermisoCitasEditarId, Codigo = "citas.editar", Nombre = "Editar citas", Descripcion = (string?)null },
        new { Id = PermisoCitasCancelarId, Codigo = "citas.cancelar", Nombre = "Cancelar citas", Descripcion = (string?)null },
        new { Id = PermisoCitasCambiarEstadoId, Codigo = "citas.cambiar_estado", Nombre = "Cambiar estado de citas", Descripcion = (string?)null },
        new { Id = PermisoConsultasVerId, Codigo = "consultas.ver", Nombre = "Ver consultas", Descripcion = (string?)null },
        new { Id = PermisoConsultasCrearId, Codigo = "consultas.crear", Nombre = "Crear consultas", Descripcion = (string?)null },
        new { Id = PermisoConsultasEditarBorradorId, Codigo = "consultas.editar_borrador", Nombre = "Editar consultas en borrador", Descripcion = (string?)null },
        new { Id = PermisoConsultasFinalizarId, Codigo = "consultas.finalizar", Nombre = "Finalizar consultas", Descripcion = (string?)null },
        new { Id = PermisoConsultasAnularId, Codigo = "consultas.anular", Nombre = "Anular consultas", Descripcion = (string?)null },
        new { Id = PermisoDiagnosticosVerId, Codigo = "diagnosticos.ver", Nombre = "Ver diagnósticos", Descripcion = (string?)null },
        new { Id = PermisoDiagnosticosCrearId, Codigo = "diagnosticos.crear", Nombre = "Crear diagnósticos", Descripcion = (string?)null },
        new { Id = PermisoDiagnosticosEditarId, Codigo = "diagnosticos.editar", Nombre = "Editar diagnósticos", Descripcion = (string?)null },
        new { Id = PermisoHistorialVerId, Codigo = "historial.ver", Nombre = "Ver historial clínico", Descripcion = (string?)null },
        new { Id = PermisoClinicaEjercerVeterinarioId, Codigo = "clinica.ejercer_veterinario", Nombre = "Ejercer como veterinario responsable", Descripcion = (string?)null },
        new { Id = PermisoTratamientosVerId, Codigo = "tratamientos.ver", Nombre = "Ver tratamientos", Descripcion = (string?)null },
        new { Id = PermisoTratamientosCrearId, Codigo = "tratamientos.crear", Nombre = "Crear tratamientos", Descripcion = (string?)null },
        new { Id = PermisoTratamientosEditarId, Codigo = "tratamientos.editar", Nombre = "Editar tratamientos", Descripcion = (string?)null },
        new { Id = PermisoTratamientosActivarId, Codigo = "tratamientos.activar", Nombre = "Activar tratamientos", Descripcion = (string?)null },
        new { Id = PermisoTratamientosSuspenderId, Codigo = "tratamientos.suspender", Nombre = "Suspender tratamientos", Descripcion = (string?)null },
        new { Id = PermisoTratamientosCompletarId, Codigo = "tratamientos.completar", Nombre = "Completar tratamientos", Descripcion = (string?)null },
        new { Id = PermisoTratamientosCancelarId, Codigo = "tratamientos.cancelar", Nombre = "Cancelar tratamientos", Descripcion = (string?)null },
        new { Id = PermisoProgramacionesVerId, Codigo = "programaciones.ver", Nombre = "Ver programaciones", Descripcion = (string?)null },
        new { Id = PermisoProgramacionesRegistrarRealizacionId, Codigo = "programaciones.registrar_realizacion", Nombre = "Registrar realización de programaciones", Descripcion = (string?)null },
        new { Id = PermisoProgramacionesRegistrarOmisionId, Codigo = "programaciones.registrar_omision", Nombre = "Registrar omisión de programaciones", Descripcion = (string?)null },
        new { Id = PermisoSeguimientosVerId, Codigo = "seguimientos.ver", Nombre = "Ver seguimientos", Descripcion = (string?)null },
        new { Id = PermisoSeguimientosCrearId, Codigo = "seguimientos.crear", Nombre = "Crear seguimientos", Descripcion = (string?)null },
        new { Id = PermisoSeguimientosEditarId, Codigo = "seguimientos.editar", Nombre = "Editar seguimientos", Descripcion = (string?)null },
        new { Id = PermisoSeguimientosRealizarId, Codigo = "seguimientos.realizar", Nombre = "Realizar seguimientos", Descripcion = (string?)null },
        new { Id = PermisoSeguimientosCancelarId, Codigo = "seguimientos.cancelar", Nombre = "Cancelar seguimientos", Descripcion = (string?)null },
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

        // Etapa 7 -- ADMINISTRADOR_VETERINARIA: toda la agenda + lectura de lo clinico. NO se
        // asume permiso clinico de escritura solo por ser administrador (seccion 31): si tambien
        // ejerce como veterinario, necesita ademas el rol VETERINARIO.
        new { RolId = RolAdministradorVeterinariaId, PermisoId = PermisoCitasVerId },
        new { RolId = RolAdministradorVeterinariaId, PermisoId = PermisoCitasCrearId },
        new { RolId = RolAdministradorVeterinariaId, PermisoId = PermisoCitasEditarId },
        new { RolId = RolAdministradorVeterinariaId, PermisoId = PermisoCitasCancelarId },
        new { RolId = RolAdministradorVeterinariaId, PermisoId = PermisoCitasCambiarEstadoId },
        new { RolId = RolAdministradorVeterinariaId, PermisoId = PermisoHistorialVerId },
        new { RolId = RolAdministradorVeterinariaId, PermisoId = PermisoConsultasVerId },
        new { RolId = RolAdministradorVeterinariaId, PermisoId = PermisoDiagnosticosVerId },

        // Etapa 7 -- VETERINARIO: agenda de solo lectura + todo lo clinico.
        new { RolId = RolVeterinarioId, PermisoId = PermisoCitasVerId },
        new { RolId = RolVeterinarioId, PermisoId = PermisoConsultasVerId },
        new { RolId = RolVeterinarioId, PermisoId = PermisoConsultasCrearId },
        new { RolId = RolVeterinarioId, PermisoId = PermisoConsultasEditarBorradorId },
        new { RolId = RolVeterinarioId, PermisoId = PermisoConsultasFinalizarId },
        new { RolId = RolVeterinarioId, PermisoId = PermisoConsultasAnularId },
        new { RolId = RolVeterinarioId, PermisoId = PermisoDiagnosticosVerId },
        new { RolId = RolVeterinarioId, PermisoId = PermisoDiagnosticosCrearId },
        new { RolId = RolVeterinarioId, PermisoId = PermisoDiagnosticosEditarId },
        new { RolId = RolVeterinarioId, PermisoId = PermisoHistorialVerId },

        // Etapa 7 -- ASISTENTE: gestiona la agenda pero NUNCA crea consultas ni diagnosticos.
        new { RolId = RolAsistenteId, PermisoId = PermisoCitasVerId },
        new { RolId = RolAsistenteId, PermisoId = PermisoCitasCrearId },
        new { RolId = RolAsistenteId, PermisoId = PermisoCitasEditarId },
        new { RolId = RolAsistenteId, PermisoId = PermisoCitasCancelarId },
        new { RolId = RolAsistenteId, PermisoId = PermisoCitasCambiarEstadoId },
        new { RolId = RolAsistenteId, PermisoId = PermisoHistorialVerId },
        new { RolId = RolAsistenteId, PermisoId = PermisoConsultasVerId },

        // Ajuste post-Etapa 7: clinica.ejercer_veterinario se asigna UNICAMENTE a VETERINARIO
        // -- ni ADMINISTRADOR_VETERINARIA ni ASISTENTE lo reciben por defecto. Un administrador
        // que tambien ejerce como veterinario necesita ademas el rol VETERINARIO.
        new { RolId = RolVeterinarioId, PermisoId = PermisoClinicaEjercerVeterinarioId },

        // Etapa 8 -- ADMINISTRADOR_VETERINARIA: solo lectura, nunca escritura clinica (seccion 45).
        new { RolId = RolAdministradorVeterinariaId, PermisoId = PermisoTratamientosVerId },
        new { RolId = RolAdministradorVeterinariaId, PermisoId = PermisoProgramacionesVerId },
        new { RolId = RolAdministradorVeterinariaId, PermisoId = PermisoSeguimientosVerId },

        // Etapa 8 -- VETERINARIO: control clinico completo de tratamientos/programaciones/seguimientos.
        new { RolId = RolVeterinarioId, PermisoId = PermisoTratamientosVerId },
        new { RolId = RolVeterinarioId, PermisoId = PermisoTratamientosCrearId },
        new { RolId = RolVeterinarioId, PermisoId = PermisoTratamientosEditarId },
        new { RolId = RolVeterinarioId, PermisoId = PermisoTratamientosActivarId },
        new { RolId = RolVeterinarioId, PermisoId = PermisoTratamientosSuspenderId },
        new { RolId = RolVeterinarioId, PermisoId = PermisoTratamientosCompletarId },
        new { RolId = RolVeterinarioId, PermisoId = PermisoTratamientosCancelarId },
        new { RolId = RolVeterinarioId, PermisoId = PermisoProgramacionesVerId },
        new { RolId = RolVeterinarioId, PermisoId = PermisoProgramacionesRegistrarRealizacionId },
        new { RolId = RolVeterinarioId, PermisoId = PermisoProgramacionesRegistrarOmisionId },
        new { RolId = RolVeterinarioId, PermisoId = PermisoSeguimientosVerId },
        new { RolId = RolVeterinarioId, PermisoId = PermisoSeguimientosCrearId },
        new { RolId = RolVeterinarioId, PermisoId = PermisoSeguimientosEditarId },
        new { RolId = RolVeterinarioId, PermisoId = PermisoSeguimientosRealizarId },
        new { RolId = RolVeterinarioId, PermisoId = PermisoSeguimientosCancelarId },

        // Etapa 8 -- ASISTENTE: ve tratamientos y registra ejecucion/omision de programaciones,
        // pero NUNCA crea/modifica el tratamiento clinico en si (seccion 45).
        new { RolId = RolAsistenteId, PermisoId = PermisoTratamientosVerId },
        new { RolId = RolAsistenteId, PermisoId = PermisoProgramacionesVerId },
        new { RolId = RolAsistenteId, PermisoId = PermisoProgramacionesRegistrarRealizacionId },
        new { RolId = RolAsistenteId, PermisoId = PermisoProgramacionesRegistrarOmisionId },
        new { RolId = RolAsistenteId, PermisoId = PermisoSeguimientosVerId },
    ];
}
