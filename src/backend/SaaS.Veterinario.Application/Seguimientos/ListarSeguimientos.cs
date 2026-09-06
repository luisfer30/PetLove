using SaaS.Veterinario.Application.Abstracciones;
using SaaS.Veterinario.Application.Errores;
using SaaS.Veterinario.Domain.Seguimientos;

namespace SaaS.Veterinario.Application.Seguimientos;

public sealed class ListarSeguimientos(
    IContextoVeterinaria contexto, IRepositorioMascotasVeterinarias repositorioMascotasVeterinarias, IRepositorioSeguimientos repositorioSeguimientos)
{
    public async Task<IReadOnlyCollection<SeguimientoClinico>> EjecutarAsync(Guid mascotaVeterinariaId, CancellationToken cancellationToken)
    {
        var veterinariaId = contexto.VeterinariaId
            ?? throw new InvalidOperationException("Se invocó ListarSeguimientos sin contexto veterinario resuelto.");

        _ = await repositorioMascotasVeterinarias.ObtenerPorIdAsync(mascotaVeterinariaId, veterinariaId, cancellationToken)
            ?? throw new ExcepcionAplicacion(CodigosError.MascotaVeterinariaNoEncontrada, "La ficha de la mascota no existe en esta veterinaria.");

        return await repositorioSeguimientos.ListarPorMascotaVeterinariaAsync(mascotaVeterinariaId, veterinariaId, cancellationToken);
    }
}
