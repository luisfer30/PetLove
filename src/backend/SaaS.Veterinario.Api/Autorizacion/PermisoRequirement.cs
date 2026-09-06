using Microsoft.AspNetCore.Authorization;

namespace SaaS.Veterinario.Api.Autorizacion;

public sealed class PermisoRequirement(string permiso) : IAuthorizationRequirement
{
    public string Permiso { get; } = permiso;
}
