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
}
