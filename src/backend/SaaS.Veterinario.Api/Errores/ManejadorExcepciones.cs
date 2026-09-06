using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using SaaS.Veterinario.Application.Errores;
using SaaS.Veterinario.Domain.Errores;

namespace SaaS.Veterinario.Api.Errores;

/// <summary>
/// Traduce ExcepcionAplicacion/ExcepcionDominio a ProblemDetails con codigo funcional y
/// correlationId. Nunca expone stack traces ni detalles tecnicos al cliente; cualquier otra
/// excepcion no prevista se registra como error y se responde como 500 generico.
/// </summary>
public sealed class ManejadorExcepciones(ILogger<ManejadorExcepciones> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var correlationId = httpContext.ObtenerCorrelationId();

        var (status, codigo, mensaje) = exception switch
        {
            ExcepcionAplicacion excepcionAplicacion => (MapearEstadoHttp(excepcionAplicacion.Codigo), excepcionAplicacion.Codigo, excepcionAplicacion.Message),
            ExcepcionDominio excepcionDominio => (StatusCodes.Status400BadRequest, "DOMINIO_INVALIDO", excepcionDominio.Message),
            _ => (StatusCodes.Status500InternalServerError, "ERROR_INTERNO", "Ocurrió un error inesperado."),
        };

        if (status == StatusCodes.Status500InternalServerError)
        {
            logger.LogError(exception, "Error no controlado. CorrelationId={CorrelationId}", correlationId);
        }
        else
        {
            logger.LogWarning("Error de aplicación {Codigo}. CorrelationId={CorrelationId}", codigo, correlationId);
        }

        httpContext.Response.StatusCode = status;

        var problema = new ProblemDetails
        {
            Status = status,
            Title = codigo,
            Detail = mensaje,
        };
        problema.Extensions["codigo"] = codigo;
        problema.Extensions["correlationId"] = correlationId;

        await httpContext.Response.WriteAsJsonAsync(problema, cancellationToken);

        return true;
    }

    private static int MapearEstadoHttp(string codigo) => codigo switch
    {
        CodigosError.CredencialesInvalidas => StatusCodes.Status401Unauthorized,
        CodigosError.CorreoNoVerificado => StatusCodes.Status403Forbidden,
        CodigosError.UsuarioBloqueado => StatusCodes.Status403Forbidden,
        CodigosError.RefreshTokenInvalido => StatusCodes.Status401Unauthorized,
        CodigosError.RefreshTokenExpirado => StatusCodes.Status401Unauthorized,
        CodigosError.RefreshTokenRevocado => StatusCodes.Status401Unauthorized,
        CodigosError.CorreoExistente => StatusCodes.Status409Conflict,
        CodigosError.TokenInvalido => StatusCodes.Status400BadRequest,
        CodigosError.RegistroInvalido => StatusCodes.Status400BadRequest,
        CodigosError.ContextoVeterinarioRequerido => StatusCodes.Status403Forbidden,
        CodigosError.VeterinariaNoDisponible => StatusCodes.Status403Forbidden,
        CodigosError.AutorizacionPermisoRequerido => StatusCodes.Status403Forbidden,
        CodigosError.PersonalRolInvalido => StatusCodes.Status400BadRequest,
        CodigosError.PersonalInvitacionPendienteExistente => StatusCodes.Status409Conflict,
        CodigosError.PersonalMembresiaActivaExistente => StatusCodes.Status409Conflict,
        CodigosError.MascotaEspecieInvalida => StatusCodes.Status400BadRequest,
        CodigosError.MascotaRazaInvalida => StatusCodes.Status400BadRequest,
        CodigosError.MascotaSexoInvalido => StatusCodes.Status400BadRequest,
        CodigosError.MascotaMicrochipDuplicado => StatusCodes.Status409Conflict,
        CodigosError.MascotaYaRegistradaEnVeterinaria => StatusCodes.Status409Conflict,
        CodigosError.MascotaNoEncontrada => StatusCodes.Status404NotFound,
        CodigosError.ClienteInvalido => StatusCodes.Status400BadRequest,
        CodigosError.ResponsableTipoRelacionInvalido => StatusCodes.Status400BadRequest,
        CodigosError.ResponsablePrincipalYaExiste => StatusCodes.Status409Conflict,
        CodigosError.CitaNoEncontrada => StatusCodes.Status404NotFound,
        CodigosError.CitaEstadoInvalido => StatusCodes.Status400BadRequest,
        CodigosError.CitaHorarioInvalido => StatusCodes.Status400BadRequest,
        CodigosError.CitaHorarioNoDisponible => StatusCodes.Status409Conflict,
        CodigosError.CitaVeterinarioInvalido => StatusCodes.Status400BadRequest,
        CodigosError.ConsultaNoEncontrada => StatusCodes.Status404NotFound,
        CodigosError.ConsultaEstadoInvalido => StatusCodes.Status400BadRequest,
        CodigosError.ConsultaFinalizadaNoEditable => StatusCodes.Status400BadRequest,
        CodigosError.ConsultaVeterinarioInvalido => StatusCodes.Status400BadRequest,
        CodigosError.ConsultaCitaInvalida => StatusCodes.Status400BadRequest,
        CodigosError.DiagnosticoNoEncontrado => StatusCodes.Status404NotFound,
        CodigosError.DiagnosticoPrincipalExistente => StatusCodes.Status409Conflict,
        CodigosError.DiagnosticoConsultaNoEditable => StatusCodes.Status400BadRequest,
        CodigosError.DiagnosticoTipoInvalido => StatusCodes.Status400BadRequest,
        CodigosError.MascotaVeterinariaNoEncontrada => StatusCodes.Status404NotFound,
        _ => StatusCodes.Status400BadRequest,
    };
}
