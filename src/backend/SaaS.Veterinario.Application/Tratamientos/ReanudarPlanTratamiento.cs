using SaaS.Veterinario.Application.Abstracciones;
using SaaS.Veterinario.Application.Clinico;
using SaaS.Veterinario.Application.Errores;
using SaaS.Veterinario.Domain.Errores;
using SaaS.Veterinario.Domain.Tratamientos;

namespace SaaS.Veterinario.Application.Tratamientos;

/// <summary>
/// SUSPENDIDO -> ACTIVO. A diferencia de ActivarPlanTratamiento, NUNCA regenera las
/// programaciones canceladas al suspender (seccion 6: "si luego se reanuda, NO regenerarlas
/// automaticamente sin una accion explicita") -- por eso es un caso de uso separado y no una
/// simple llamada a Activar().
/// </summary>
public sealed class ReanudarPlanTratamiento(IContextoVeterinaria contexto, IRepositorioPlanesTratamiento repositorioPlanes)
{
    public async Task EjecutarAsync(Guid planTratamientoId, CancellationToken cancellationToken)
    {
        ValidadorPermisoClinico.ValidarUsuarioActual(contexto);

        var veterinariaId = contexto.VeterinariaId
            ?? throw new InvalidOperationException("Se invocó ReanudarPlanTratamiento sin contexto veterinario resuelto.");

        var plan = await repositorioPlanes.ObtenerPorIdAsync(planTratamientoId, veterinariaId, cancellationToken)
            ?? throw new ExcepcionAplicacion(CodigosError.TratamientoNoEncontrado, "El plan de tratamiento no existe en esta veterinaria.");

        try
        {
            plan.Reanudar(contexto.UsuarioId, DateTimeOffset.UtcNow);
        }
        catch (ExcepcionDominio excepcion)
        {
            throw new ExcepcionAplicacion(CodigosError.TratamientoEstadoInvalido, excepcion.Message);
        }

        await repositorioPlanes.GuardarCambiosAsync(cancellationToken);
    }
}
