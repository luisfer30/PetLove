using SaaS.Veterinario.Domain.Consultas;

namespace SaaS.Veterinario.Application.Abstracciones;

/// <summary>Toda lectura por Id exige tambien VeterinariaId (R07), igual que IRepositorioCitas.</summary>
public interface IRepositorioConsultas
{
    Task<ConsultaVeterinaria?> ObtenerPorIdAsync(Guid id, Guid veterinariaId, CancellationToken cancellationToken);

    /// <summary>El historial clinico es independiente por MascotaVeterinaria (R10) -- nunca cruza tenants.</summary>
    Task<IReadOnlyCollection<ConsultaVeterinaria>> ListarPorMascotaVeterinariaAsync(
        Guid mascotaVeterinariaId, Guid veterinariaId, CancellationToken cancellationToken);

    Task AgregarAsync(ConsultaVeterinaria consulta, CancellationToken cancellationToken);

    Task GuardarCambiosAsync(CancellationToken cancellationToken);
}
