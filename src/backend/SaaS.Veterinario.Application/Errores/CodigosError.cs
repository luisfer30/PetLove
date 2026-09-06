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
}
