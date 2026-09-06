using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;
using SaaS.Veterinario.Api.Autorizacion;
using SaaS.Veterinario.Application.Abstracciones;
using SaaS.Veterinario.Application.ContextoVeterinario;
using SaaS.Veterinario.Application.Personal;
using SaaS.Veterinario.Application.Veterinarias;
using SaaS.Veterinario.Infrastructure.Identidad;

namespace SaaS.Veterinario.Api.Extensiones;

public static class ServiciosContextoVeterinarioExtensiones
{
    /// <summary>
    /// ContextoVeterinaria (concreto) y IContextoVeterinaria SIEMPRE deben resolver a la MISMA
    /// instancia Scoped -- de lo contrario ContextoVeterinarioMiddleware cargaría una y los
    /// casos de uso/handlers leerían otra vacía. Se registra dos veces a proposito: una vez
    /// como tipo concreto (para que el middleware pueda llamar a Cargar) y otra como la
    /// interfaz publica, delegando al mismo tipo concreto ya resuelto en el scope.
    /// </summary>
    public static IServiceCollection AgregarContextoVeterinario(this IServiceCollection servicios)
    {
        servicios.AddScoped<ContextoVeterinaria>();
        servicios.AddScoped<IContextoVeterinaria>(sp => sp.GetRequiredService<ContextoVeterinaria>());

        servicios.AddScoped<IRepositorioMembresias, RepositorioMembresias>();
        servicios.AddScoped<IRepositorioVeterinarias, RepositorioVeterinarias>();
        servicios.AddScoped<IRepositorioRoles, RepositorioRoles>();
        servicios.AddScoped<IRepositorioInvitaciones, RepositorioInvitaciones>();
        servicios.AddScoped<IServicioInvitaciones, ServicioInvitaciones>();

        servicios.AddScoped<ResolverContextoVeterinaria>();
        servicios.AddScoped<ActualizarVeterinariaActual>();
        servicios.AddScoped<InvitarPersonal>();

        return servicios;
    }

    /// <summary>
    /// PermisoAuthorizationHandler depende de IContextoVeterinaria (Scoped), asi que el
    /// handler tambien debe ser Scoped -- ASP.NET Core resuelve IAuthorizationHandler desde
    /// el contenedor de la request, no hay problema de "captive dependency" aqui.
    /// </summary>
    public static IServiceCollection AgregarAutorizacionPorPermisos(this IServiceCollection servicios)
    {
        servicios.AddScoped<IAuthorizationHandler, PermisoAuthorizationHandler>();
        servicios.AddSingleton<IAuthorizationPolicyProvider, PermisoPolicyProvider>();
        servicios.AddSingleton<IAuthorizationMiddlewareResultHandler, ManejadorResultadoAutorizacion>();

        return servicios;
    }
}
