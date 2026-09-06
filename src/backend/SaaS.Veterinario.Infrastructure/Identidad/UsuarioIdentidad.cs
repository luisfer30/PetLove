using Microsoft.AspNetCore.Identity;

namespace SaaS.Veterinario.Infrastructure.Identidad;

/// <summary>
/// Identidad de autenticacion (ASP.NET Core Identity), separada de la entidad de dominio
/// Usuario. UsuarioId enlaza 1:1 con SaaS.Veterinario.Domain.Usuarios.Usuario.Id -- esta
/// clase NUNCA contiene datos funcionales (VeterinariaId, roles/permisos de negocio, etc.),
/// solo lo que Identity necesita para autenticar (password hash, lockout, security stamp).
/// UserName se fija siempre igual al correo normalizado; no existe un username visible
/// independiente para el usuario final.
/// </summary>
public sealed class UsuarioIdentidad : IdentityUser<Guid>
{
    public Guid UsuarioId { get; set; }
}
