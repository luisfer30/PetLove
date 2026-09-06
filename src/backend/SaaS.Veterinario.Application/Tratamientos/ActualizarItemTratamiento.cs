using SaaS.Veterinario.Application.Abstracciones;
using SaaS.Veterinario.Application.Clinico;
using SaaS.Veterinario.Application.Errores;
using SaaS.Veterinario.Domain.Errores;
using SaaS.Veterinario.Domain.Tratamientos;

namespace SaaS.Veterinario.Application.Tratamientos;

public sealed record ActualizarItemTratamientoComando(Guid PlanTratamientoId, Guid ItemTratamientoId, string? Descripcion, string? Instrucciones);

/// <summary>Solo cambios no estructurales (seccion 32); nunca dosis/frecuencia/horarios en un item existente.</summary>
public sealed class ActualizarItemTratamiento(
    IContextoVeterinaria contexto, IRepositorioPlanesTratamiento repositorioPlanes, IRepositorioItemsTratamiento repositorioItems)
{
    public async Task EjecutarAsync(ActualizarItemTratamientoComando comando, CancellationToken cancellationToken)
    {
        ValidadorPermisoClinico.ValidarUsuarioActual(contexto);

        var veterinariaId = contexto.VeterinariaId
            ?? throw new InvalidOperationException("Se invocó ActualizarItemTratamiento sin contexto veterinario resuelto.");

        var plan = await repositorioPlanes.ObtenerPorIdAsync(comando.PlanTratamientoId, veterinariaId, cancellationToken)
            ?? throw new ExcepcionAplicacion(CodigosError.TratamientoNoEncontrado, "El plan de tratamiento no existe en esta veterinaria.");

        if (plan.Estado is EstadoPlanTratamiento.Completado or EstadoPlanTratamiento.Cancelado)
        {
            throw new ExcepcionAplicacion(CodigosError.TratamientoEstadoInvalido, $"No se puede modificar un item de un plan en estado '{plan.Estado}'.");
        }

        var item = await repositorioItems.ObtenerPorIdAsync(comando.ItemTratamientoId, plan.Id, cancellationToken)
            ?? throw new ExcepcionAplicacion(CodigosError.TratamientoItemNoEncontrado, "El item de tratamiento no existe en este plan.");

        try
        {
            item.ActualizarInstrucciones(comando.Descripcion, comando.Instrucciones, DateTimeOffset.UtcNow);
        }
        catch (ExcepcionDominio excepcion)
        {
            throw new ExcepcionAplicacion(CodigosError.TratamientoItemEstadoInvalido, excepcion.Message);
        }

        await repositorioItems.GuardarCambiosAsync(cancellationToken);
    }
}
