using SaaS.Veterinario.Application.Abstracciones;
using SaaS.Veterinario.Application.Clinico;
using SaaS.Veterinario.Application.Errores;
using SaaS.Veterinario.Domain.Tratamientos;

namespace SaaS.Veterinario.Application.Tratamientos;

public sealed record ActualizarPlanTratamientoComando(
    Guid PlanTratamientoId, string Nombre, string? Descripcion, DateTimeOffset FechaInicio, DateTimeOffset? FechaFinEstimada);

/// <summary>
/// Mientras PENDIENTE permite edicion amplia; mientras ACTIVO solo ajustes no estructurales
/// (descripcion/fecha fin estimada -- seccion 32), ignorando cambios de nombre/fecha inicio en
/// ese caso en vez de fallar, para no obligar al cliente a distinguir dos formularios distintos.
/// </summary>
public sealed class ActualizarPlanTratamiento(IContextoVeterinaria contexto, IRepositorioPlanesTratamiento repositorioPlanes)
{
    public async Task EjecutarAsync(ActualizarPlanTratamientoComando comando, CancellationToken cancellationToken)
    {
        ValidadorPermisoClinico.ValidarUsuarioActual(contexto);

        var veterinariaId = contexto.VeterinariaId
            ?? throw new InvalidOperationException("Se invocó ActualizarPlanTratamiento sin contexto veterinario resuelto.");

        var plan = await repositorioPlanes.ObtenerPorIdAsync(comando.PlanTratamientoId, veterinariaId, cancellationToken)
            ?? throw new ExcepcionAplicacion(CodigosError.TratamientoNoEncontrado, "El plan de tratamiento no existe en esta veterinaria.");

        var momento = DateTimeOffset.UtcNow;

        switch (plan.Estado)
        {
            case EstadoPlanTratamiento.Pendiente:
                plan.ActualizarDatosPendiente(comando.Nombre, comando.Descripcion, comando.FechaInicio, comando.FechaFinEstimada, contexto.UsuarioId, momento);
                break;

            case EstadoPlanTratamiento.Activo:
                plan.ActualizarDatosActivo(comando.Descripcion, comando.FechaFinEstimada, contexto.UsuarioId, momento);
                break;

            default:
                throw new ExcepcionAplicacion(CodigosError.TratamientoEstadoInvalido, $"No se puede editar un plan en estado '{plan.Estado}'.");
        }

        await repositorioPlanes.GuardarCambiosAsync(cancellationToken);
    }
}
