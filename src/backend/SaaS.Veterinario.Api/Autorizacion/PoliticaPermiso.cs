namespace SaaS.Veterinario.Api.Autorizacion;

/// <summary>
/// Convierte un codigo de permiso de negocio (p. ej. "personal.invitar") en un nombre de
/// politica de ASP.NET Core Authorization ("PERMISO:personal.invitar"). Permite permisos
/// dinamicos sin registrar una politica por cada uno -- ver PermisoPolicyProvider, que
/// reconoce este prefijo y construye la politica al vuelo.
/// </summary>
public static class PoliticaPermiso
{
    private const string Prefijo = "PERMISO:";

    public static string NombrePolitica(string permiso) => Prefijo + permiso;

    public static bool EsPoliticaDePermiso(string nombrePolitica) =>
        nombrePolitica.StartsWith(Prefijo, StringComparison.Ordinal);

    public static string ExtraerPermiso(string nombrePolitica) => nombrePolitica[Prefijo.Length..];
}
