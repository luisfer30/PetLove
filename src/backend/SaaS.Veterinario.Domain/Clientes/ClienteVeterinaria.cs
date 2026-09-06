using SaaS.Veterinario.Domain.Errores;
using SaaS.Veterinario.Domain.ObjetosValor;

namespace SaaS.Veterinario.Domain.Clientes;

/// <summary>
/// Pertenece exclusivamente a una Veterinaria (R08) y puede existir sin Usuario global (R09).
/// A diferencia de Usuario/Veterinaria, Correo es OPCIONAL aqui: es solo dato de contacto de
/// esta veterinaria (R13), no una identidad logica, por eso no se exige ni se corre unicidad
/// global sobre el.
/// </summary>
public sealed class ClienteVeterinaria
{
    public Guid Id { get; }
    public Guid VeterinariaId { get; }
    public Guid? UsuarioId { get; private set; }
    public string Nombre { get; private set; }
    public Correo? Correo { get; private set; }
    public string? Telefono { get; private set; }
    public string? Observaciones { get; private set; }
    public EstadoClienteVeterinaria Estado { get; private set; }
    public DateTimeOffset FechaCreacion { get; }
    public DateTimeOffset FechaActualizacion { get; private set; }
    public DateTimeOffset? FechaEliminacion { get; private set; }

    private ClienteVeterinaria(
        Guid id,
        Guid veterinariaId,
        string nombre,
        Correo? correo,
        string? telefono,
        string? observaciones,
        DateTimeOffset fechaCreacion)
    {
        Id = id;
        VeterinariaId = veterinariaId;
        Nombre = nombre;
        Correo = correo;
        Telefono = telefono;
        Observaciones = observaciones;
        Estado = EstadoClienteVeterinaria.Activo;
        FechaCreacion = fechaCreacion;
        FechaActualizacion = fechaCreacion;
    }

    public static ClienteVeterinaria Crear(
        Guid veterinariaId,
        string nombre,
        string? correo,
        string? telefono,
        string? observaciones,
        DateTimeOffset? fechaCreacion = null)
    {
        if (veterinariaId == Guid.Empty)
        {
            throw new ExcepcionDominio("El identificador de la veterinaria es obligatorio.");
        }

        if (string.IsNullOrWhiteSpace(nombre))
        {
            throw new ExcepcionDominio("El nombre del cliente es obligatorio.");
        }

        var correoValidado = string.IsNullOrWhiteSpace(correo) ? null : Correo.Crear(correo);
        var fecha = fechaCreacion ?? DateTimeOffset.UtcNow;

        return new ClienteVeterinaria(
            Guid.NewGuid(),
            veterinariaId,
            nombre.Trim(),
            correoValidado,
            NormalizarOpcional(telefono),
            NormalizarOpcional(observaciones),
            fecha);
    }

    public void Activar() => Estado = EstadoClienteVeterinaria.Activo;

    public void Desactivar() => Estado = EstadoClienteVeterinaria.Inactivo;

    private static string? NormalizarOpcional(string? valor)
        => string.IsNullOrWhiteSpace(valor) ? null : valor.Trim();
}
