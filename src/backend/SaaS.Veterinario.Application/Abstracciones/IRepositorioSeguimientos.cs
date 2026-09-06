using SaaS.Veterinario.Domain.Seguimientos;

namespace SaaS.Veterinario.Application.Abstracciones;

public interface IRepositorioSeguimientos
{
    Task<SeguimientoClinico?> ObtenerPorIdAsync(Guid id, Guid veterinariaId, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<SeguimientoClinico>> ListarPorMascotaVeterinariaAsync(Guid mascotaVeterinariaId, Guid veterinariaId, CancellationToken cancellationToken);

    /// <summary>Protege la idempotencia del seguimiento automatico desde ProximaFechaControl (seccion 36/53).</summary>
    Task<bool> ExisteAutoGeneradoParaConsultaAsync(Guid consultaOrigenId, CancellationToken cancellationToken);

    Task AgregarAsync(SeguimientoClinico seguimiento, CancellationToken cancellationToken);

    Task GuardarCambiosAsync(CancellationToken cancellationToken);
}
