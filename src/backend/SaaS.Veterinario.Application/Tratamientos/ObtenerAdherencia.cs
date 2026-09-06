using SaaS.Veterinario.Application.Abstracciones;
using SaaS.Veterinario.Application.Errores;

namespace SaaS.Veterinario.Application.Tratamientos;

public sealed record AdherenciaResultado(
    Guid PlanTratamientoId, int Realizadas, int Omitidas, int Vencidas, int Pendientes, int Canceladas, decimal? PorcentajeAdherencia);

/// <summary>
/// adherencia = REALIZADAS / (REALIZADAS + OMITIDAS + VENCIDAS) * 100 (seccion 38) --
/// PENDIENTE y CANCELADA nunca cuentan como evaluables. Se calcula bajo demanda, nunca se
/// persiste el porcentaje.
/// </summary>
public sealed class ObtenerAdherencia(IContextoVeterinaria contexto, IRepositorioPlanesTratamiento repositorioPlanes, IRepositorioProgramacionesTratamiento repositorioProgramaciones)
{
    public async Task<AdherenciaResultado> EjecutarAsync(Guid planTratamientoId, CancellationToken cancellationToken)
    {
        var veterinariaId = contexto.VeterinariaId
            ?? throw new InvalidOperationException("Se invocó ObtenerAdherencia sin contexto veterinario resuelto.");

        _ = await repositorioPlanes.ObtenerPorIdAsync(planTratamientoId, veterinariaId, cancellationToken)
            ?? throw new ExcepcionAplicacion(CodigosError.TratamientoNoEncontrado, "El plan de tratamiento no existe en esta veterinaria.");

        var conteo = await repositorioProgramaciones.ContarPorEstadoAsync(planTratamientoId, veterinariaId, cancellationToken);

        var evaluables = conteo.Realizadas + conteo.Omitidas + conteo.Vencidas;
        decimal? porcentaje = evaluables == 0 ? null : Math.Round(conteo.Realizadas * 100m / evaluables, 2);

        return new AdherenciaResultado(planTratamientoId, conteo.Realizadas, conteo.Omitidas, conteo.Vencidas, conteo.Pendientes, conteo.Canceladas, porcentaje);
    }
}
