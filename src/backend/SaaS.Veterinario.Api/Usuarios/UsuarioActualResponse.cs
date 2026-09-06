using SaaS.Veterinario.Domain.Usuarios;

namespace SaaS.Veterinario.Api.Usuarios;

public sealed record UsuarioActualResponse(
    Guid Id,
    string Nombre,
    string Correo,
    string? Telefono,
    bool CorreoVerificado,
    bool TelefonoVerificado,
    string Estado)
{
    public static UsuarioActualResponse DeDominio(Usuario usuario)
        => new(
            usuario.Id,
            usuario.Nombre,
            usuario.Correo.Valor,
            usuario.Telefono,
            usuario.CorreoVerificado,
            usuario.TelefonoVerificado,
            usuario.Estado.ToString());
}
