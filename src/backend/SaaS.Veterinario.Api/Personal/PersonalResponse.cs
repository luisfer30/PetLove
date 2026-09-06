using SaaS.Veterinario.Application.Abstracciones;

namespace SaaS.Veterinario.Api.Personal;

public sealed record MiembroPersonalResponse(
    Guid MembresiaId,
    Guid UsuarioId,
    string Nombre,
    string Correo,
    IReadOnlyCollection<string> Roles)
{
    public static MiembroPersonalResponse DeInfo(InfoMiembroPersonal info)
        => new(info.MembresiaId, info.UsuarioId, info.Nombre, info.Correo, info.Roles);
}

public sealed record InvitarPersonalRequest(string Correo, IReadOnlyCollection<string> Roles);

public sealed record InvitarPersonalResponse(Guid InvitacionId);
