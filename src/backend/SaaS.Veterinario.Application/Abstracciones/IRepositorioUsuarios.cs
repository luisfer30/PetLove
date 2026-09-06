using SaaS.Veterinario.Domain.Usuarios;

namespace SaaS.Veterinario.Application.Abstracciones;

/// <summary>
/// Repositorio especifico de Usuario (no generico): las unicas operaciones que los casos
/// de uso de autenticacion realmente necesitan. AgregarAsync persiste de inmediato;
/// GuardarCambiosAsync existe para el flujo de "cargar, mutar via metodos de dominio,
/// persistir" (p. ej. ConfirmarCorreo + Activar).
/// </summary>
public interface IRepositorioUsuarios
{
    Task<bool> ExisteCorreoAsync(string correo, CancellationToken cancellationToken);

    Task<Usuario?> ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken);

    Task<Usuario?> ObtenerPorCorreoAsync(string correo, CancellationToken cancellationToken);

    Task AgregarAsync(Usuario usuario, CancellationToken cancellationToken);

    Task GuardarCambiosAsync(CancellationToken cancellationToken);
}
