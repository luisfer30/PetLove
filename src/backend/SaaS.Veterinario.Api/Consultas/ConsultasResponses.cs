using SaaS.Veterinario.Application.Consultas;
using SaaS.Veterinario.Domain.Consultas;

namespace SaaS.Veterinario.Api.Consultas;

public sealed record ConsultaResponse(
    Guid Id,
    Guid MascotaVeterinariaId,
    Guid? CitaId,
    Guid VeterinarioUsuarioId,
    DateTimeOffset FechaHora,
    string MotivoConsulta,
    decimal? Peso,
    decimal? Temperatura,
    int? FrecuenciaCardiaca,
    string? ObservacionesClinicas,
    string? IndicacionesPropietario,
    DateTimeOffset? ProximaFechaControl,
    string? MotivoProximoControl,
    string Estado,
    DateTimeOffset FechaCreacion,
    Guid CreadoPorUsuarioId,
    DateTimeOffset FechaActualizacion,
    Guid? ActualizadoPorUsuarioId,
    DateTimeOffset? FechaFinalizacion,
    Guid? FinalizadoPorUsuarioId,
    DateTimeOffset? FechaAnulacion,
    Guid? AnuladoPorUsuarioId,
    string? MotivoAnulacion)
{
    public static ConsultaResponse DeDominio(ConsultaVeterinaria consulta)
        => new(
            consulta.Id,
            consulta.MascotaVeterinariaId,
            consulta.CitaId,
            consulta.VeterinarioUsuarioId,
            consulta.FechaHora,
            consulta.MotivoConsulta,
            consulta.Peso,
            consulta.Temperatura,
            consulta.FrecuenciaCardiaca,
            consulta.ObservacionesClinicas,
            consulta.IndicacionesPropietario,
            consulta.ProximaFechaControl,
            consulta.MotivoProximoControl,
            consulta.Estado.ToString(),
            consulta.FechaCreacion,
            consulta.CreadoPorUsuarioId,
            consulta.FechaActualizacion,
            consulta.ActualizadoPorUsuarioId,
            consulta.FechaFinalizacion,
            consulta.FinalizadoPorUsuarioId,
            consulta.FechaAnulacion,
            consulta.AnuladoPorUsuarioId,
            consulta.MotivoAnulacion);
}

public sealed record CrearConsultaRequest(
    Guid MascotaVeterinariaId,
    Guid? CitaId,
    Guid VeterinarioUsuarioId,
    DateTimeOffset FechaHora,
    string MotivoConsulta,
    decimal? Peso,
    decimal? Temperatura,
    int? FrecuenciaCardiaca,
    string? ObservacionesClinicas,
    string? IndicacionesPropietario,
    DateTimeOffset? ProximaFechaControl,
    string? MotivoProximoControl);

public sealed record CrearConsultaResponse(Guid ConsultaId, string Estado);

public sealed record ActualizarConsultaRequest(
    Guid VeterinarioUsuarioId,
    DateTimeOffset FechaHora,
    string MotivoConsulta,
    decimal? Peso,
    decimal? Temperatura,
    int? FrecuenciaCardiaca,
    string? ObservacionesClinicas,
    string? IndicacionesPropietario,
    DateTimeOffset? ProximaFechaControl,
    string? MotivoProximoControl);

public sealed record AnularConsultaRequest(string Motivo);

public sealed record DiagnosticoResponse(Guid Id, Guid ConsultaId, string Descripcion, string Tipo, bool EsPrincipal, DateTimeOffset FechaCreacion)
{
    public static DiagnosticoResponse DeDominio(DiagnosticoConsulta diagnostico)
        => new(diagnostico.Id, diagnostico.ConsultaId, diagnostico.Descripcion, diagnostico.Tipo.ToString(), diagnostico.EsPrincipal, diagnostico.FechaCreacion);
}

public sealed record AgregarDiagnosticoRequest(string Descripcion, string Tipo, bool EsPrincipal);

public sealed record AgregarDiagnosticoResponse(Guid DiagnosticoId);

public sealed record ActualizarDiagnosticoRequest(string Descripcion, string Tipo, bool EsPrincipal);

public sealed record HistorialEventoResponse(string Tipo, DateTimeOffset FechaHora, string Titulo, string Estado, string? Resumen)
{
    public static HistorialEventoResponse DeAplicacion(HistorialEvento evento)
        => new(evento.Tipo, evento.FechaHora, evento.Titulo, evento.Estado, evento.Resumen);
}

public sealed record HistorialMascotaResponse(
    Guid MascotaVeterinariaId, string CodigoPublicoMascota, string NombreMascota, IReadOnlyCollection<HistorialEventoResponse> Eventos)
{
    public static HistorialMascotaResponse DeAplicacion(HistorialMascotaResultado resultado)
        => new(
            resultado.MascotaVeterinariaId,
            resultado.CodigoPublicoMascota,
            resultado.NombreMascota,
            resultado.Eventos.Select(HistorialEventoResponse.DeAplicacion).ToList());
}
