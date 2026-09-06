using SaaS.Veterinario.Application.Abstracciones;
using SaaS.Veterinario.Application.ContextoVeterinario;

namespace SaaS.Veterinario.Infrastructure.Identidad;

/// <summary>
/// Implementacion mutable de IContextoVeterinaria. Se registra Scoped y se puebla UNA sola
/// vez por request via Cargar(...), llamado por ContextoVeterinarioMiddleware (Api) despues
/// de autenticar -- de ahi que Cargar sea internal en vez de parte de la interfaz publica:
/// ningun caso de uso de Application debe poder mutar su propio contexto.
/// </summary>
public sealed class ContextoVeterinaria : IContextoVeterinaria
{
    private IReadOnlyCollection<string> _roles = [];
    private IReadOnlyCollection<string> _permisos = [];
    private HashSet<string> _permisosBusqueda = [];

    public Guid UsuarioId { get; private set; }
    public bool TieneMembresiaActiva { get; private set; }
    public bool TieneContextoOperativo { get; private set; }
    public Guid? VeterinariaId { get; private set; }
    public Guid? MembresiaId { get; private set; }
    public IReadOnlyCollection<string> Roles => _roles;
    public IReadOnlyCollection<string> Permisos => _permisos;

    public bool TienePermiso(string codigoPermiso) => _permisosBusqueda.Contains(codigoPermiso);

    /// <summary>
    /// Publico (no parte de IContextoVeterinaria) a proposito: solo quien tenga una referencia
    /// al tipo concreto -- en la practica, unicamente ContextoVeterinarioMiddleware en Api,
    /// inyectado explicitamente por tipo -- puede mutar el contexto. Cualquier consumidor que
    /// dependa de la interfaz IContextoVeterinaria nunca ve este metodo.
    /// </summary>
    public void Cargar(Guid usuarioId, ResultadoContexto resultado)
    {
        UsuarioId = usuarioId;
        TieneMembresiaActiva = resultado.TieneMembresiaActiva;
        TieneContextoOperativo = resultado.TieneContextoOperativo;
        VeterinariaId = resultado.VeterinariaId;
        MembresiaId = resultado.MembresiaId;
        _roles = resultado.Roles;
        _permisos = resultado.Permisos;
        _permisosBusqueda = new HashSet<string>(resultado.Permisos, StringComparer.Ordinal);
    }
}
