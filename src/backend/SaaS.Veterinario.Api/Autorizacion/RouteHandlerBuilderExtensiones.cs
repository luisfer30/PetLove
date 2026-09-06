namespace SaaS.Veterinario.Api.Autorizacion;

/// <summary>
/// Equivalente fluido de [RequierePermiso] para endpoints minimal API: aplica exactamente la
/// misma politica dinamica por nombre ("PERMISO:codigo"), resuelta por PermisoPolicyProvider.
/// </summary>
public static class RouteHandlerBuilderExtensiones
{
    public static RouteHandlerBuilder RequierePermiso(this RouteHandlerBuilder builder, string permiso)
        => builder.RequireAuthorization(PoliticaPermiso.NombrePolitica(permiso));
}
