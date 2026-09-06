using SaaS.Veterinario.Domain.Tratamientos;

namespace SaaS.Veterinario.Application.Abstracciones;

/// <summary>
/// Genera los instantes (DateTimeOffset) en que debe ocurrir cada actividad de un
/// ItemTratamiento, respetando la ZonaHoraria de la Veterinaria (seccion 23/28) -- 08:00 hora
/// clinica local nunca se asume como 08:00 UTC. La implementacion real vive en Infrastructure
/// porque depende de TimeZoneInfo, igual que CodigoPublico/RefreshToken con la aleatoriedad.
/// </summary>
public interface IGeneradorProgramacionesTratamiento
{
    IReadOnlyCollection<DateTimeOffset> Generar(ParametrosGeneracionProgramaciones parametros);
}

public sealed record ParametrosGeneracionProgramaciones(
    TipoFrecuenciaTratamiento FrecuenciaTipo,
    DateTimeOffset FechaInicio,
    DateTimeOffset? FechaFin,
    int? DuracionDias,
    int? IntervaloHoras,
    IReadOnlyCollection<TimeOnly> Horarios,
    string ZonaHorariaVeterinaria);
