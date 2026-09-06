using SaaS.Veterinario.Application.Abstracciones;
using SaaS.Veterinario.Application.Clientes;
using SaaS.Veterinario.Application.Mascotas;
using SaaS.Veterinario.Infrastructure.Clientes;
using SaaS.Veterinario.Infrastructure.Mascotas;

namespace SaaS.Veterinario.Api.Extensiones;

public static class ServiciosMascotasExtensiones
{
    public static IServiceCollection AgregarCasosDeUsoDeMascotas(this IServiceCollection servicios)
    {
        servicios.AddScoped<IRepositorioClientes, RepositorioClientes>();
        servicios.AddScoped<IRepositorioEspecies, RepositorioEspecies>();
        servicios.AddScoped<IRepositorioMascotas, RepositorioMascotas>();
        servicios.AddScoped<IRepositorioMascotasVeterinarias, RepositorioMascotasVeterinarias>();
        servicios.AddScoped<IRepositorioResponsables, RepositorioResponsables>();
        servicios.AddScoped<IGeneradorCodigoPublico, GeneradorCodigoPublico>();

        servicios.AddScoped<RegistrarCliente>();
        servicios.AddScoped<RegistrarMascota>();
        servicios.AddScoped<VincularMascotaExistente>();
        servicios.AddScoped<BuscarMascota>();
        servicios.AddScoped<RegistrarResponsableMascota>();

        return servicios;
    }
}
