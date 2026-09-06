using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace SaaS.Veterinario.Api.Autorizacion;

/// <summary>
/// Reconoce nombres de politica con el prefijo "PERMISO:" y construye la AuthorizationPolicy
/// dinamicamente (RequireAuthenticatedUser + PermisoRequirement), en vez de exigir registrar
/// una politica nombrada por cada permiso de negocio existente. Cualquier otro nombre de
/// politica se delega al proveedor por defecto.
/// </summary>
public sealed class PermisoPolicyProvider(IOptions<AuthorizationOptions> opciones) : IAuthorizationPolicyProvider
{
    private readonly DefaultAuthorizationPolicyProvider _predeterminado = new(opciones);

    public Task<AuthorizationPolicy> GetDefaultPolicyAsync() => _predeterminado.GetDefaultPolicyAsync();

    public Task<AuthorizationPolicy?> GetFallbackPolicyAsync() => _predeterminado.GetFallbackPolicyAsync();

    public Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
    {
        if (PoliticaPermiso.EsPoliticaDePermiso(policyName))
        {
            var permiso = PoliticaPermiso.ExtraerPermiso(policyName);
            var politica = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .AddRequirements(new PermisoRequirement(permiso))
                .Build();

            return Task.FromResult<AuthorizationPolicy?>(politica);
        }

        return _predeterminado.GetPolicyAsync(policyName);
    }
}
