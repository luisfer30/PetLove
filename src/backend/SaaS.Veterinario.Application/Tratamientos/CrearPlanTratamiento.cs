using SaaS.Veterinario.Application.Abstracciones;
using SaaS.Veterinario.Application.Clinico;
using SaaS.Veterinario.Application.Errores;
using SaaS.Veterinario.Domain.Consultas;
using SaaS.Veterinario.Domain.Tratamientos;

namespace SaaS.Veterinario.Application.Tratamientos;

public sealed record CrearPlanTratamientoComando(
    Guid MascotaVeterinariaId,
    Guid? ConsultaId,
    string Nombre,
    string? Descripcion,
    DateTimeOffset FechaInicio,
    DateTimeOffset? FechaFinEstimada);

public sealed record CrearPlanTratamientoResultado(Guid PlanTratamientoId, EstadoPlanTratamiento Estado);

/// <summary>
/// Un plan puede crearse sin Consulta (seccion 30) o asociado a una, incluso mientras esta
/// sigue BORRADOR (seccion 29) -- siempre nace PENDIENTE (R04, seccion 4); solo al ACTIVAR se
/// exige que la Consulta asociada, si existe, este FINALIZADA (ver ActivarPlanTratamiento).
/// </summary>
public sealed class CrearPlanTratamiento(
    IContextoVeterinaria contexto,
    IRepositorioPlanesTratamiento repositorioPlanes,
    IRepositorioMascotasVeterinarias repositorioMascotasVeterinarias,
    IRepositorioConsultas repositorioConsultas)
{
    public async Task<CrearPlanTratamientoResultado> EjecutarAsync(CrearPlanTratamientoComando comando, CancellationToken cancellationToken)
    {
        ValidadorPermisoClinico.ValidarUsuarioActual(contexto);

        var veterinariaId = contexto.VeterinariaId
            ?? throw new InvalidOperationException("Se invocó CrearPlanTratamiento sin contexto veterinario resuelto.");

        _ = await repositorioMascotasVeterinarias.ObtenerPorIdAsync(comando.MascotaVeterinariaId, veterinariaId, cancellationToken)
            ?? throw new ExcepcionAplicacion(CodigosError.MascotaVeterinariaNoEncontrada, "La ficha de la mascota no existe en esta veterinaria.");

        if (comando.ConsultaId is { } consultaId)
        {
            var consulta = await repositorioConsultas.ObtenerPorIdAsync(consultaId, veterinariaId, cancellationToken)
                ?? throw new ExcepcionAplicacion(CodigosError.TratamientoConsultaInvalida, "La consulta indicada no existe en esta veterinaria.");

            if (consulta.MascotaVeterinariaId != comando.MascotaVeterinariaId)
            {
                throw new ExcepcionAplicacion(CodigosError.TratamientoConsultaInvalida, "La consulta indicada no corresponde a esta mascota.");
            }

            if (consulta.Estado == EstadoConsulta.Anulada)
            {
                throw new ExcepcionAplicacion(CodigosError.TratamientoConsultaInvalida, "No se puede crear un tratamiento sobre una consulta anulada.");
            }
        }

        var plan = PlanTratamiento.Crear(
            veterinariaId,
            comando.MascotaVeterinariaId,
            comando.ConsultaId,
            comando.Nombre,
            comando.Descripcion,
            comando.FechaInicio,
            comando.FechaFinEstimada,
            contexto.UsuarioId);

        await repositorioPlanes.AgregarAsync(plan, cancellationToken);

        return new CrearPlanTratamientoResultado(plan.Id, plan.Estado);
    }
}
