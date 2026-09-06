using SaaS.Veterinario.Domain.Mascotas;

namespace SaaS.Veterinario.Application.Abstracciones;

public interface IRepositorioMascotasVeterinarias
{
    Task<bool> ExisteVinculoAsync(Guid veterinariaId, Guid mascotaId, CancellationToken cancellationToken);

    /// <summary>Tenant-scoped: retorna null si la ficha no existe O pertenece a otra veterinaria (nunca distingue los dos casos).</summary>
    Task<MascotaVeterinaria?> ObtenerPorIdAsync(Guid id, Guid veterinariaId, CancellationToken cancellationToken);

    Task<InfoMascotaFicha?> ObtenerFichaPorIdAsync(Guid id, Guid veterinariaId, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<InfoMascotaFicha>> ListarPorVeterinariaAsync(Guid veterinariaId, CancellationToken cancellationToken);

    Task AgregarAsync(MascotaVeterinaria mascotaVeterinaria, CancellationToken cancellationToken);
}

/// <summary>Proyeccion de solo lectura para listar/consultar la ficha local (DTO de Application, no la entidad EF).</summary>
public sealed record InfoMascotaFicha(
    Guid MascotaVeterinariaId,
    Guid MascotaId,
    string CodigoPublico,
    string Nombre,
    string Especie,
    string? Raza,
    string Sexo,
    string? NumeroHistoria,
    string EstadoLocal);
