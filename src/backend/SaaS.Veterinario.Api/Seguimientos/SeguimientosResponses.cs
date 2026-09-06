using SaaS.Veterinario.Domain.Seguimientos;

namespace SaaS.Veterinario.Api.Seguimientos;

public sealed record SeguimientoClinicoResponse(
    Guid Id,
    Guid MascotaVeterinariaId,
    Guid? ConsultaOrigenId,
    Guid? PlanTratamientoId,
    string Tipo,
    DateTimeOffset FechaObjetivo,
    string Estado,
    string Motivo,
    string? Notas,
    DateTimeOffset? FechaAgendada,
    Guid? CitaId,
    DateTimeOffset? FechaRealizacion,
    bool EsGeneradoDesdeProximoControl)
{
    public static SeguimientoClinicoResponse DeDominio(SeguimientoClinico seguimiento)
        => new(
            seguimiento.Id, seguimiento.MascotaVeterinariaId, seguimiento.ConsultaOrigenId, seguimiento.PlanTratamientoId,
            seguimiento.Tipo.ToString(), seguimiento.FechaObjetivo, seguimiento.Estado.ToString(), seguimiento.Motivo, seguimiento.Notas,
            seguimiento.FechaAgendada, seguimiento.CitaId, seguimiento.FechaRealizacion, seguimiento.EsGeneradoDesdeProximoControl);
}

public sealed record CrearSeguimientoRequest(Guid? ConsultaOrigenId, Guid? PlanTratamientoId, string Tipo, DateTimeOffset FechaObjetivo, string Motivo, string? Notas);

public sealed record CrearSeguimientoResponse(Guid SeguimientoId);

public sealed record MarcarSeguimientoAgendadoRequest(Guid CitaId);
