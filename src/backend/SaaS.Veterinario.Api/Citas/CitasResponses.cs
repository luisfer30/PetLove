using SaaS.Veterinario.Domain.Citas;

namespace SaaS.Veterinario.Api.Citas;

public sealed record CitaResponse(
    Guid Id,
    Guid MascotaVeterinariaId,
    Guid? ClienteVeterinariaId,
    Guid? VeterinarioUsuarioId,
    DateTimeOffset FechaHoraInicio,
    DateTimeOffset FechaHoraFin,
    string Motivo,
    string? Observaciones,
    string Estado,
    DateTimeOffset FechaCreacion,
    Guid CreadoPorUsuarioId,
    DateTimeOffset FechaActualizacion,
    Guid? ActualizadoPorUsuarioId)
{
    public static CitaResponse DeDominio(Cita cita)
        => new(
            cita.Id,
            cita.MascotaVeterinariaId,
            cita.ClienteVeterinariaId,
            cita.VeterinarioUsuarioId,
            cita.FechaHoraInicio,
            cita.FechaHoraFin,
            cita.Motivo,
            cita.Observaciones,
            cita.Estado.ToString(),
            cita.FechaCreacion,
            cita.CreadoPorUsuarioId,
            cita.FechaActualizacion,
            cita.ActualizadoPorUsuarioId);
}

public sealed record CrearCitaRequest(
    Guid MascotaVeterinariaId,
    Guid? ClienteVeterinariaId,
    Guid? VeterinarioUsuarioId,
    DateTimeOffset FechaHoraInicio,
    DateTimeOffset FechaHoraFin,
    string Motivo,
    string? Observaciones);

public sealed record CrearCitaResponse(Guid CitaId, string Estado);

public sealed record ActualizarCitaRequest(
    DateTimeOffset FechaHoraInicio,
    DateTimeOffset FechaHoraFin,
    string Motivo,
    string? Observaciones,
    Guid? VeterinarioUsuarioId);
