using SaaS.Veterinario.Application.Tratamientos;
using SaaS.Veterinario.Domain.Tratamientos;

namespace SaaS.Veterinario.Api.Tratamientos;

public sealed record PlanTratamientoResponse(
    Guid Id,
    Guid MascotaVeterinariaId,
    Guid? ConsultaId,
    string Nombre,
    string? Descripcion,
    DateTimeOffset FechaInicio,
    DateTimeOffset? FechaFinEstimada,
    string Estado,
    DateTimeOffset FechaCreacion,
    Guid CreadoPorUsuarioId,
    DateTimeOffset FechaActualizacion,
    Guid? ActualizadoPorUsuarioId,
    Guid? SuspendidoPorUsuarioId,
    DateTimeOffset? FechaSuspension,
    string? MotivoSuspension,
    Guid? CompletadoPorUsuarioId,
    DateTimeOffset? FechaCompletado,
    Guid? CanceladoPorUsuarioId,
    DateTimeOffset? FechaCancelacion,
    string? MotivoCancelacion)
{
    public static PlanTratamientoResponse DeDominio(PlanTratamiento plan)
        => new(
            plan.Id, plan.MascotaVeterinariaId, plan.ConsultaId, plan.Nombre, plan.Descripcion, plan.FechaInicio, plan.FechaFinEstimada,
            plan.Estado.ToString(), plan.FechaCreacion, plan.CreadoPorUsuarioId, plan.FechaActualizacion, plan.ActualizadoPorUsuarioId,
            plan.SuspendidoPorUsuarioId, plan.FechaSuspension, plan.MotivoSuspension, plan.CompletadoPorUsuarioId, plan.FechaCompletado,
            plan.CanceladoPorUsuarioId, plan.FechaCancelacion, plan.MotivoCancelacion);
}

public sealed record CrearPlanTratamientoRequest(
    Guid MascotaVeterinariaId, Guid? ConsultaId, string Nombre, string? Descripcion, DateTimeOffset FechaInicio, DateTimeOffset? FechaFinEstimada);

public sealed record CrearPlanTratamientoResponse(Guid PlanTratamientoId, string Estado);

public sealed record ActualizarPlanTratamientoRequest(string Nombre, string? Descripcion, DateTimeOffset FechaInicio, DateTimeOffset? FechaFinEstimada);

public sealed record SuspenderPlanTratamientoRequest(string Motivo);

public sealed record CancelarPlanTratamientoRequest(string Motivo);

public sealed record HorarioItemResponse(TimeOnly Hora, int Orden)
{
    public static HorarioItemResponse DeDominio(HorarioItemTratamiento horario) => new(horario.Hora, horario.Orden);
}

public sealed record ItemTratamientoResponse(
    Guid Id,
    Guid PlanTratamientoId,
    string Tipo,
    string Nombre,
    string? Descripcion,
    decimal? DosisCantidad,
    string? DosisUnidad,
    string? ViaAdministracion,
    string FrecuenciaTipo,
    int? IntervaloHoras,
    int? VecesPorDia,
    IReadOnlyCollection<HorarioItemResponse> Horarios,
    DateTimeOffset FechaInicio,
    DateTimeOffset? FechaFin,
    int? DuracionDias,
    string? Instrucciones,
    bool EsSegunNecesidad,
    string Estado,
    int Orden)
{
    public static ItemTratamientoResponse DeDominio(ItemTratamiento item)
        => new(
            item.Id, item.PlanTratamientoId, item.Tipo.ToString(), item.Nombre, item.Descripcion, item.DosisCantidad, item.DosisUnidad,
            item.ViaAdministracion?.ToString(), item.FrecuenciaTipo.ToString(), item.IntervaloHoras, item.VecesPorDia,
            item.Horarios.OrderBy(h => h.Orden).Select(HorarioItemResponse.DeDominio).ToList(),
            item.FechaInicio, item.FechaFin, item.DuracionDias, item.Instrucciones, item.EsSegunNecesidad, item.Estado.ToString(), item.Orden);
}

public sealed record AgregarItemTratamientoRequest(
    string Tipo,
    string Nombre,
    string? Descripcion,
    decimal? DosisCantidad,
    string? DosisUnidad,
    string? ViaAdministracion,
    string FrecuenciaTipo,
    int? IntervaloHoras,
    int? VecesPorDia,
    IReadOnlyCollection<TimeOnly> Horarios,
    DateTimeOffset FechaInicio,
    DateTimeOffset? FechaFin,
    int? DuracionDias,
    string? Instrucciones,
    int Orden);

public sealed record AgregarItemTratamientoResponse(Guid ItemTratamientoId);

public sealed record ActualizarItemTratamientoRequest(string? Descripcion, string? Instrucciones);

public sealed record ProgramacionTratamientoResponse(
    Guid Id,
    Guid MascotaVeterinariaId,
    Guid PlanTratamientoId,
    Guid ItemTratamientoId,
    DateTimeOffset FechaHoraProgramada,
    string Estado,
    DateTimeOffset? FechaRealizacion,
    Guid? RegistradoPorUsuarioId,
    string? ObservacionRealizacion)
{
    public static ProgramacionTratamientoResponse DeDominio(ProgramacionTratamiento programacion)
        => new(
            programacion.Id, programacion.MascotaVeterinariaId, programacion.PlanTratamientoId, programacion.ItemTratamientoId,
            programacion.FechaHoraProgramada, programacion.Estado.ToString(), programacion.FechaRealizacion,
            programacion.RegistradoPorUsuarioId, programacion.ObservacionRealizacion);
}

public sealed record RegistrarProgramacionRealizadaRequest(string? Observacion);

public sealed record MarcarProgramacionOmitidaRequest(string? Observacion);

public sealed record AdherenciaResponse(Guid PlanTratamientoId, int Realizadas, int Omitidas, int Vencidas, int Pendientes, int Canceladas, decimal? PorcentajeAdherencia)
{
    public static AdherenciaResponse DeAplicacion(AdherenciaResultado resultado)
        => new(resultado.PlanTratamientoId, resultado.Realizadas, resultado.Omitidas, resultado.Vencidas, resultado.Pendientes, resultado.Canceladas, resultado.PorcentajeAdherencia);
}
