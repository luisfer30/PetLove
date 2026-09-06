using SaaS.Veterinario.Application.Abstracciones;
using SaaS.Veterinario.Application.Clinico;
using SaaS.Veterinario.Application.Errores;
using SaaS.Veterinario.Domain.Errores;
using SaaS.Veterinario.Domain.Tratamientos;

namespace SaaS.Veterinario.Application.Tratamientos;

public sealed record AgregarItemTratamientoComando(
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
    IReadOnlyCollection<TimeOnly> Horarios,
    DateTimeOffset FechaInicio,
    DateTimeOffset? FechaFin,
    int? DuracionDias,
    string? Instrucciones,
    int Orden);

public sealed record AgregarItemTratamientoResultado(Guid ItemTratamientoId);

/// <summary>
/// Permitido mientras el plan este PENDIENTE o ACTIVO (seccion 32: agregar un item nuevo es la
/// via sancionada para introducir un cambio de dosis/frecuencia en un tratamiento activo). Si
/// el plan ya esta ACTIVO, el item nuevo genera sus propias programaciones de inmediato, en la
/// misma transaccion; si esta PENDIENTE, la generacion queda diferida a ActivarPlanTratamiento.
/// </summary>
public sealed class AgregarItemTratamiento(
    IContextoVeterinaria contexto,
    IRepositorioPlanesTratamiento repositorioPlanes,
    IRepositorioItemsTratamiento repositorioItems,
    IRepositorioProgramacionesTratamiento repositorioProgramaciones,
    IRepositorioVeterinarias repositorioVeterinarias,
    IGeneradorProgramacionesTratamiento generadorProgramaciones,
    IUnidadDeTrabajo unidadDeTrabajo)
{
    public async Task<AgregarItemTratamientoResultado> EjecutarAsync(AgregarItemTratamientoComando comando, CancellationToken cancellationToken)
    {
        ValidadorPermisoClinico.ValidarUsuarioActual(contexto);

        var veterinariaId = contexto.VeterinariaId
            ?? throw new InvalidOperationException("Se invocó AgregarItemTratamiento sin contexto veterinario resuelto.");

        var plan = await repositorioPlanes.ObtenerPorIdAsync(comando.PlanTratamientoId, veterinariaId, cancellationToken)
            ?? throw new ExcepcionAplicacion(CodigosError.TratamientoNoEncontrado, "El plan de tratamiento no existe en esta veterinaria.");

        if (plan.Estado is not (EstadoPlanTratamiento.Pendiente or EstadoPlanTratamiento.Activo))
        {
            throw new ExcepcionAplicacion(CodigosError.TratamientoEstadoInvalido, "Solo se pueden agregar items a un plan pendiente o activo.");
        }

        if (!Enum.TryParse<TipoItemTratamiento>(comando.Tipo, ignoreCase: true, out var tipo))
        {
            throw new ExcepcionAplicacion(CodigosError.TratamientoItemEstadoInvalido, "El tipo de item indicado no es válido.");
        }

        if (!Enum.TryParse<TipoFrecuenciaTratamiento>(comando.FrecuenciaTipo, ignoreCase: true, out var frecuenciaTipo))
        {
            throw new ExcepcionAplicacion(CodigosError.TratamientoFrecuenciaInvalida, "El tipo de frecuencia indicado no es válido.");
        }

        Domain.Tratamientos.ViaAdministracion? viaAdministracion = null;
        if (!string.IsNullOrWhiteSpace(comando.ViaAdministracion))
        {
            if (!Enum.TryParse<Domain.Tratamientos.ViaAdministracion>(comando.ViaAdministracion, ignoreCase: true, out var via))
            {
                throw new ExcepcionAplicacion(CodigosError.TratamientoHorarioInvalido, "La vía de administración indicada no es válida.");
            }

            viaAdministracion = via;
        }

        ItemTratamiento item;
        try
        {
            item = ItemTratamiento.Crear(
                plan.Id, tipo, comando.Nombre, comando.Descripcion, comando.DosisCantidad, comando.DosisUnidad, viaAdministracion,
                frecuenciaTipo, comando.IntervaloHoras, comando.VecesPorDia, comando.Horarios,
                comando.FechaInicio, comando.FechaFin, comando.DuracionDias, comando.Instrucciones, comando.Orden);
        }
        catch (ExcepcionDominio excepcion)
        {
            var codigo = frecuenciaTipo is TipoFrecuenciaTratamiento.HoraFija or TipoFrecuenciaTratamiento.VecesPorDia
                ? CodigosError.TratamientoHorarioInvalido
                : CodigosError.TratamientoFrecuenciaInvalida;
            throw new ExcepcionAplicacion(codigo, excepcion.Message);
        }

        if (plan.Estado == EstadoPlanTratamiento.Pendiente)
        {
            await repositorioItems.AgregarAsync(item, cancellationToken);
            return new AgregarItemTratamientoResultado(item.Id);
        }

        var veterinaria = await repositorioVeterinarias.ObtenerPorIdAsync(veterinariaId, cancellationToken)
            ?? throw new InvalidOperationException("La veterinaria del contexto no existe.");

        await unidadDeTrabajo.EjecutarEnTransaccionAsync(async token =>
        {
            await repositorioItems.AgregarAsync(item, token);

            if (item.EsSegunNecesidad)
            {
                return;
            }

            var instantes = generadorProgramaciones.Generar(new ParametrosGeneracionProgramaciones(
                item.FrecuenciaTipo, item.FechaInicio, item.FechaFin, item.DuracionDias, item.IntervaloHoras,
                item.Horarios.OrderBy(h => h.Orden).Select(h => h.Hora).ToList(), veterinaria.ZonaHoraria));

            var nuevasProgramaciones = new List<Domain.Tratamientos.ProgramacionTratamiento>();
            foreach (var instante in instantes)
            {
                if (!await repositorioProgramaciones.ExisteEnFechaAsync(item.Id, instante, token))
                {
                    nuevasProgramaciones.Add(Domain.Tratamientos.ProgramacionTratamiento.Crear(veterinariaId, plan.MascotaVeterinariaId, plan.Id, item.Id, instante));
                }
            }

            if (nuevasProgramaciones.Count > 0)
            {
                await repositorioProgramaciones.AgregarRangoAsync(nuevasProgramaciones, token);
            }
        }, cancellationToken);

        return new AgregarItemTratamientoResultado(item.Id);
    }
}
