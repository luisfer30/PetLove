namespace SaaS.Veterinario.Application.Abstracciones;

/// <summary>
/// Contexto de tenant resuelto automaticamente para el usuario autenticado de la request
/// actual: JWT (sub) -> Usuario -> MembresiaVeterinaria ACTIVA -> Veterinaria ACTIVA -> Roles
/// -> Permisos. Nunca se recibe VeterinariaId desde el cliente.
///
/// Un usuario autenticado puede no tener contexto veterinario en absoluto (sera el caso de
/// los futuros propietarios de mascotas) -- por eso TieneMembresiaActiva/TieneContextoOperativo
/// existen como flags explicitos en vez de asumir que VeterinariaId siempre tiene valor.
/// </summary>
public interface IContextoVeterinaria
{
    Guid UsuarioId { get; }

    /// <summary>El usuario tiene una MembresiaVeterinaria en estado ACTIVA.</summary>
    bool TieneMembresiaActiva { get; }

    /// <summary>
    /// Ademas de tener membresia activa, la Veterinaria correspondiente tambien esta ACTIVA.
    /// Solo cuando esto es true hay Roles/Permisos resueltos y tiene sentido operar como
    /// personal de esa veterinaria.
    /// </summary>
    bool TieneContextoOperativo { get; }

    Guid? VeterinariaId { get; }

    Guid? MembresiaId { get; }

    IReadOnlyCollection<string> Roles { get; }

    IReadOnlyCollection<string> Permisos { get; }

    bool TienePermiso(string codigoPermiso);
}
