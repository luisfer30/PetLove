using SaaS.Veterinario.Application.Abstracciones;
using SaaS.Veterinario.Application.Errores;
using SaaS.Veterinario.Domain.Tratamientos;

namespace SaaS.Veterinario.Application.Tratamientos;

public sealed class ListarProgramacionesPorMascota(
    IContextoVeterinaria contexto, IRepositorioMascotasVeterinarias repositorioMascotasVeterinarias, IRepositorioProgramacionesTratamiento repositorioProgramaciones)
{
    public async Task<IReadOnlyCollection<ProgramacionTratamiento>> EjecutarAsync(Guid mascotaVeterinariaId, CancellationToken cancellationToken)
    {
        var veterinariaId = contexto.VeterinariaId
            ?? throw new InvalidOperationException("Se invocó ListarProgramacionesPorMascota sin contexto veterinario resuelto.");

        _ = await repositorioMascotasVeterinarias.ObtenerPorIdAsync(mascotaVeterinariaId, veterinariaId, cancellationToken)
            ?? throw new ExcepcionAplicacion(CodigosError.MascotaVeterinariaNoEncontrada, "La ficha de la mascota no existe en esta veterinaria.");

        return await repositorioProgramaciones.ListarPorMascotaVeterinariaAsync(mascotaVeterinariaId, veterinariaId, cancellationToken);
    }
}
