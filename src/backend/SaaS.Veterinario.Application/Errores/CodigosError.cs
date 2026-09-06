namespace SaaS.Veterinario.Application.Errores;

/// <summary>
/// Codigos funcionales estables para ExcepcionAplicacion. Evita strings magicos repetidos
/// entre casos de uso y su mapeo a ProblemDetails en la Api.
/// </summary>
public static class CodigosError
{
    public const string CredencialesInvalidas = "AUTENTICACION_CREDENCIALES_INVALIDAS";
    public const string CorreoNoVerificado = "AUTENTICACION_CORREO_NO_VERIFICADO";
    public const string UsuarioBloqueado = "AUTENTICACION_USUARIO_BLOQUEADO";
    public const string RefreshTokenInvalido = "AUTENTICACION_REFRESH_TOKEN_INVALIDO";
    public const string RefreshTokenExpirado = "AUTENTICACION_REFRESH_TOKEN_EXPIRADO";
    public const string RefreshTokenRevocado = "AUTENTICACION_REFRESH_TOKEN_REVOCADO";
    public const string CorreoExistente = "USUARIO_CORREO_EXISTENTE";

    // No enumerados explicitamente en el pedido original; se agregan para no dejar sin
    // codigo los fallos de token de confirmacion/recuperacion (ambos comparten taxonomia
    // porque son el mismo tipo de fallo: token invalido, expirado o ya usado).
    public const string TokenInvalido = "AUTENTICACION_TOKEN_INVALIDO";
    public const string RegistroInvalido = "REGISTRO_INVALIDO";

    // Etapa 5: ContextoVeterinaria + autorizacion por permisos.
    public const string ContextoVeterinarioRequerido = "CONTEXTO_VETERINARIA_REQUERIDO";
    public const string VeterinariaNoDisponible = "VETERINARIA_NO_DISPONIBLE";
    public const string AutorizacionPermisoRequerido = "AUTORIZACION_PERMISO_REQUERIDO";
    public const string PersonalRolInvalido = "PERSONAL_ROL_INVALIDO";
    public const string PersonalInvitacionPendienteExistente = "PERSONAL_INVITACION_PENDIENTE_EXISTENTE";

    // Reservado para el flujo de aceptacion de invitaciones (no implementado todavia en esta
    // etapa): se activa cuando alguien intenta activar una membresia y ya tiene otra ACTIVA.
    public const string PersonalMembresiaActivaExistente = "PERSONAL_MEMBRESIA_ACTIVA_EXISTENTE";

    // Etapa 6: Clientes + Mascotas.
    public const string MascotaEspecieInvalida = "MASCOTA_ESPECIE_INVALIDA";
    public const string MascotaRazaInvalida = "MASCOTA_RAZA_INVALIDA";
    public const string MascotaSexoInvalido = "MASCOTA_SEXO_INVALIDO";
    public const string MascotaMicrochipDuplicado = "MASCOTA_MICROCHIP_DUPLICADO";
    public const string MascotaYaRegistradaEnVeterinaria = "MASCOTA_YA_REGISTRADA_EN_VETERINARIA";
    public const string MascotaNoEncontrada = "MASCOTA_NO_ENCONTRADA";
    public const string ClienteInvalido = "CLIENTE_INVALIDO";
    public const string ResponsableTipoRelacionInvalido = "RESPONSABLE_TIPO_RELACION_INVALIDO";
    public const string ResponsablePrincipalYaExiste = "RESPONSABLE_PRINCIPAL_YA_EXISTE";

    // Etapa 7: Agenda + Consultas + Diagnosticos.
    public const string CitaNoEncontrada = "CITA_NO_ENCONTRADA";
    public const string CitaEstadoInvalido = "CITA_ESTADO_INVALIDO";
    public const string CitaHorarioInvalido = "CITA_HORARIO_INVALIDO";
    public const string CitaHorarioNoDisponible = "CITA_HORARIO_NO_DISPONIBLE";
    public const string CitaVeterinarioInvalido = "CITA_VETERINARIO_INVALIDO";

    public const string ConsultaNoEncontrada = "CONSULTA_NO_ENCONTRADA";
    public const string ConsultaEstadoInvalido = "CONSULTA_ESTADO_INVALIDO";
    public const string ConsultaFinalizadaNoEditable = "CONSULTA_FINALIZADA_NO_EDITABLE";
    public const string ConsultaVeterinarioInvalido = "CONSULTA_VETERINARIO_INVALIDO";
    public const string ConsultaCitaInvalida = "CONSULTA_CITA_INVALIDA";

    public const string DiagnosticoNoEncontrado = "DIAGNOSTICO_NO_ENCONTRADO";
    public const string DiagnosticoPrincipalExistente = "DIAGNOSTICO_PRINCIPAL_EXISTENTE";
    public const string DiagnosticoConsultaNoEditable = "DIAGNOSTICO_CONSULTA_NO_EDITABLE";

    // No enumerado explicitamente en el pedido original; se agrega siguiendo el mismo patron
    // que MascotaSexoInvalido (Etapa 6) para no dejar sin codigo un Tipo de enum invalido.
    public const string DiagnosticoTipoInvalido = "DIAGNOSTICO_TIPO_INVALIDO";

    public const string MascotaVeterinariaNoEncontrada = "MASCOTA_VETERINARIA_NO_ENCONTRADA";

    // Etapa 8: Tratamientos + Items + Programaciones + Seguimiento clinico.
    public const string TratamientoNoEncontrado = "TRATAMIENTO_NO_ENCONTRADO";
    public const string TratamientoEstadoInvalido = "TRATAMIENTO_ESTADO_INVALIDO";
    public const string TratamientoConsultaInvalida = "TRATAMIENTO_CONSULTA_INVALIDA";
    public const string TratamientoRequiereConsultaFinalizada = "TRATAMIENTO_REQUIERE_CONSULTA_FINALIZADA";
    public const string TratamientoItemNoEncontrado = "TRATAMIENTO_ITEM_NO_ENCONTRADO";
    public const string TratamientoItemEstadoInvalido = "TRATAMIENTO_ITEM_ESTADO_INVALIDO";
    public const string TratamientoFrecuenciaInvalida = "TRATAMIENTO_FRECUENCIA_INVALIDA";
    public const string TratamientoHorarioInvalido = "TRATAMIENTO_HORARIO_INVALIDO";

    public const string ProgramacionNoEncontrada = "PROGRAMACION_NO_ENCONTRADA";
    public const string ProgramacionEstadoInvalido = "PROGRAMACION_ESTADO_INVALIDO";

    public const string SeguimientoNoEncontrado = "SEGUIMIENTO_NO_ENCONTRADO";
    public const string SeguimientoEstadoInvalido = "SEGUIMIENTO_ESTADO_INVALIDO";
    public const string SeguimientoCitaInvalida = "SEGUIMIENTO_CITA_INVALIDA";
}
