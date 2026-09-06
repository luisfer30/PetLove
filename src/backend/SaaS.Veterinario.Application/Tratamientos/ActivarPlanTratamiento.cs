using SaaS.Veterinario.Application.Abstracciones;
using SaaS.Veterinario.Application.Clinico;
using SaaS.Veterinario.Application.Errores;
using SaaS.Veterinario.Domain.Consultas;
using SaaS.Veterinario.Domain.Tratamientos;

namespace SaaS.Veterinario.Application.Tratamientos;

/// <summary>
/// Activar (PENDIENTE -> ACTIVO) exige que la Consulta asociada, si existe, este FINALIZADA
/// (seccion 29) y genera las Programaciones de todos los items activos del plan (seccion 54):
/// Plan.Activar() + generacion + persistencia son atomicos -- si algo falla, el plan permanece
/// PENDIENTE y no quedan programaciones parciales.
/// </summary>
public sealed class ActivarPlanTratamiento(
    IContextoVeterinaria contexto,
    IRepositorioPlanesTratamiento repositorioPlanes,
    IRepositorioItemsTratamiento repositorioItems,
    IRepositorioProgramacionesTratamiento repositorioProgramaciones,
    IRepositorioConsultas repositorioConsultas,
    IRepositorioVeterinarias repositorioVeterinarias,
    IGeneradorProgramacionesTratamiento generadorProgramaciones,
    IUnidadDeTrabajo unidadDeTrabajo)
{
    public async Task EjecutarAsync(Guid planTratamientoId, CancellationToken cancellationToken)
    {
        ValidadorPermisoClinico.ValidarUsuarioActual(contexto);

        var veterinariaId = contexto.VeterinariaId
            ?? throw new InvalidOperationException("Se invocó ActivarPlanTratamiento sin contexto veterinario resuelto.");

        var plan = await repositorioPlanes.ObtenerPorIdAsync(planTratamientoId, veterinariaId, cancellationToken)
            ?? throw new ExcepcionAplicacion(CodigosError.TratamientoNoEncontrado, "El plan de tratamiento no existe en esta veterinaria.");

        if (plan.Estado != EstadoPlanTratamiento.Pendiente)
        {
            throw new ExcepcionAplicacion(CodigosError.TratamientoEstadoInvalido, "Solo se puede activar un plan pendiente.");
        }

        if (plan.ConsultaId is { } consultaId)
        {
            var consulta = await repositorioConsultas.ObtenerPorIdAsync(consultaId, veterinariaId, cancellationToken)
                ?? throw new ExcepcionAplicacion(CodigosError.TratamientoConsultaInvalida, "La consulta asociada ya no existe en esta veterinaria.");

            if (consulta.Estado != EstadoConsulta.Finalizada)
            {
                throw new ExcepcionAplicacion(
                    CodigosError.TratamientoRequiereConsultaFinalizada, "La consulta asociada debe estar finalizada para activar el plan.");
            }
        }

        var veterinaria = await repositorioVeterinarias.ObtenerPorIdAsync(veterinariaId, cancellationToken)
            ?? throw new InvalidOperationException("La veterinaria del contexto no existe.");

        var items = await repositorioItems.ListarPorPlanAsync(planTratamientoId, cancellationToken);

        await unidadDeTrabajo.EjecutarEnTransaccionAsync(async token =>
        {
            var momento = DateTimeOffset.UtcNow;
            plan.Activar(contexto.UsuarioId, momento);
            await repositorioPlanes.GuardarCambiosAsync(token);

            var nuevasProgramaciones = new List<Domain.Tratamientos.ProgramacionTratamiento>();

            foreach (var item in items.Where(i => i.Estado == EstadoItemTratamiento.Activo && !i.EsSegunNecesidad))
            {
                var instantes = generadorProgramaciones.Generar(new ParametrosGeneracionProgramaciones(
                    item.FrecuenciaTipo,
                    item.FechaInicio,
                    item.FechaFin,
                    item.DuracionDias,
                    item.IntervaloHoras,
                    item.Horarios.OrderBy(h => h.Orden).Select(h => h.Hora).ToList(),
                    veterinaria.ZonaHoraria));

                foreach (var instante in instantes)
                {
                    if (await repositorioProgramaciones.ExisteEnFechaAsync(item.Id, instante, token))
                    {
                        continue;
                    }

                    nuevasProgramaciones.Add(Domain.Tratamientos.ProgramacionTratamiento.Crear(
                        veterinariaId, plan.MascotaVeterinariaId, plan.Id, item.Id, instante));
                }
            }

            if (nuevasProgramaciones.Count > 0)
            {
                await repositorioProgramaciones.AgregarRangoAsync(nuevasProgramaciones, token);
            }
        }, cancellationToken);
    }
}
