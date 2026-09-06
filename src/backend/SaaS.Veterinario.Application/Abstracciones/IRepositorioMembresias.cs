namespace SaaS.Veterinario.Application.Abstracciones;

/// <summary>
/// Consultas de solo lectura sobre MembresiaVeterinaria necesarias para resolver contexto y
/// listar personal. No es un repositorio generico: expone exactamente lo que se necesita.
/// </summary>
public interface IRepositorioMembresias
{
    /// <summary>
    /// Busca la unica MembresiaVeterinaria ACTIVA del usuario (a lo sumo una, garantizado por
    /// el indice unico parcial de la Etapa 3) junto con los codigos de Rol y Permiso ya
    /// resueltos y sin duplicados.
    /// </summary>
    Task<InfoContextoMembresia?> ObtenerContextoActivoAsync(Guid usuarioId, CancellationToken cancellationToken);

    /// <summary>Personal actualmente activo (membresias ACTIVA) de una veterinaria.</summary>
    Task<IReadOnlyCollection<InfoMiembroPersonal>> ObtenerPersonalActivoAsync(Guid veterinariaId, CancellationToken cancellationToken);
}

public sealed record InfoContextoMembresia(
    Guid MembresiaId,
    Guid VeterinariaId,
    IReadOnlyCollection<string> Roles,
    IReadOnlyCollection<string> Permisos);

public sealed record InfoMiembroPersonal(
    Guid MembresiaId,
    Guid UsuarioId,
    string Nombre,
    string Correo,
    IReadOnlyCollection<string> Roles);
