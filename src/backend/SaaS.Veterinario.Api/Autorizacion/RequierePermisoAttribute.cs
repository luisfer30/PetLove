using Microsoft.AspNetCore.Authorization;

namespace SaaS.Veterinario.Api.Autorizacion;

/// <summary>
/// [RequierePermiso("personal.invitar")]. Pensado para uso futuro con controllers/paginas
/// que soportan atributos directamente; los endpoints minimal API de esta etapa usan el
/// equivalente fluido RequierePermiso(...) de RouteHandlerBuilderExtensiones, que aplica
/// exactamente la misma politica dinamica por nombre.
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false)]
public sealed class RequierePermisoAttribute(string permiso) : AuthorizeAttribute(PoliticaPermiso.NombrePolitica(permiso));
