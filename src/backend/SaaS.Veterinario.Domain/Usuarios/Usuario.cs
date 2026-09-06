using SaaS.Veterinario.Domain.Errores;
using SaaS.Veterinario.Domain.ObjetosValor;

namespace SaaS.Veterinario.Domain.Usuarios;

public sealed class Usuario
{
    private static readonly Dictionary<EstadoUsuario, EstadoUsuario[]> TransicionesPermitidas = new()
    {
        [EstadoUsuario.PendienteVerificacion] = [EstadoUsuario.Activo],
        [EstadoUsuario.Activo] = [EstadoUsuario.Bloqueado, EstadoUsuario.Suspendido],
        [EstadoUsuario.Bloqueado] = [EstadoUsuario.Activo],
        [EstadoUsuario.Suspendido] = [EstadoUsuario.Activo],
    };

    public Guid Id { get; }
    public string Nombre { get; private set; }
    public Correo Correo { get; private set; }
    public string? Telefono { get; private set; }
    public bool CorreoVerificado { get; private set; }
    public bool TelefonoVerificado { get; private set; }
    public EstadoUsuario Estado { get; private set; }
    public DateTimeOffset FechaCreacion { get; }
    public DateTimeOffset FechaActualizacion { get; private set; }

    private Usuario(Guid id, string nombre, Correo correo, string? telefono, DateTimeOffset fechaCreacion)
    {
        Id = id;
        Nombre = nombre;
        Correo = correo;
        Telefono = telefono;
        CorreoVerificado = false;
        TelefonoVerificado = false;
        Estado = EstadoUsuario.PendienteVerificacion;
        FechaCreacion = fechaCreacion;
        FechaActualizacion = fechaCreacion;
    }

    public static Usuario Crear(string nombre, string correo, string? telefono = null, DateTimeOffset? fechaCreacion = null)
    {
        if (string.IsNullOrWhiteSpace(nombre))
        {
            throw new ExcepcionDominio("El nombre del usuario es obligatorio.");
        }

        var correoValidado = Correo.Crear(correo);
        var fecha = fechaCreacion ?? DateTimeOffset.UtcNow;

        return new Usuario(Guid.NewGuid(), nombre.Trim(), correoValidado, NormalizarTelefono(telefono), fecha);
    }

    public void Activar(DateTimeOffset momento) => CambiarEstado(EstadoUsuario.Activo, momento);

    public void Bloquear(DateTimeOffset momento) => CambiarEstado(EstadoUsuario.Bloqueado, momento);

    public void Suspender(DateTimeOffset momento) => CambiarEstado(EstadoUsuario.Suspendido, momento);

    public void ConfirmarCorreo()
    {
        CorreoVerificado = true;
    }

    public void ConfirmarTelefono()
    {
        if (string.IsNullOrWhiteSpace(Telefono))
        {
            throw new ExcepcionDominio("No se puede confirmar el teléfono porque el usuario no tiene uno registrado.");
        }

        TelefonoVerificado = true;
    }

    private void CambiarEstado(EstadoUsuario nuevoEstado, DateTimeOffset momento)
    {
        if (!TransicionesPermitidas[Estado].Contains(nuevoEstado))
        {
            throw new ExcepcionDominio(
                $"No se puede transicionar el usuario de '{Estado}' a '{nuevoEstado}'.");
        }

        Estado = nuevoEstado;
        FechaActualizacion = momento;
    }

    private static string? NormalizarTelefono(string? telefono)
        => string.IsNullOrWhiteSpace(telefono) ? null : telefono.Trim();
}
