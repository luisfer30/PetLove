using SaaS.Veterinario.Domain.Mascotas;

namespace SaaS.Veterinario.Application.Abstracciones;

public interface IRepositorioResponsables
{
    /// <summary>
    /// Segunda mitad de la regla "un solo responsable principal activo por MascotaVeterinaria"
    /// (la primera es esta comprobacion explicita en el caso de uso; la BD tiene un indice
    /// unico parcial como red de seguridad, igual patron que la invitacion pendiente duplicada).
    /// </summary>
    Task<bool> ExistePrincipalActivoAsync(Guid mascotaVeterinariaId, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<ResponsableMascota>> ListarPorMascotaVeterinariaAsync(
        Guid mascotaVeterinariaId, Guid veterinariaId, CancellationToken cancellationToken);

    Task AgregarAsync(ResponsableMascota responsable, CancellationToken cancellationToken);
}
