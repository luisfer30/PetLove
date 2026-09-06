using SaaS.Veterinario.Application.Abstracciones;
using SaaS.Veterinario.Application.Citas;
using SaaS.Veterinario.Application.Consultas;
using SaaS.Veterinario.Infrastructure.Citas;
using SaaS.Veterinario.Infrastructure.Consultas;

namespace SaaS.Veterinario.Api.Extensiones;

public static class ServiciosClinicaExtensiones
{
    public static IServiceCollection AgregarCasosDeUsoDeClinica(this IServiceCollection servicios)
    {
        servicios.AddScoped<IRepositorioCitas, RepositorioCitas>();
        servicios.AddScoped<IRepositorioConsultas, RepositorioConsultas>();
        servicios.AddScoped<IRepositorioDiagnosticos, RepositorioDiagnosticos>();

        servicios.AddScoped<CrearCita>();
        servicios.AddScoped<ActualizarCita>();
        servicios.AddScoped<GestionarEstadoCita>();
        servicios.AddScoped<ObtenerCita>();
        servicios.AddScoped<ListarAgenda>();

        servicios.AddScoped<CrearConsulta>();
        servicios.AddScoped<ActualizarConsultaBorrador>();
        servicios.AddScoped<FinalizarConsulta>();
        servicios.AddScoped<AnularConsulta>();
        servicios.AddScoped<ObtenerConsulta>();
        servicios.AddScoped<AgregarDiagnostico>();
        servicios.AddScoped<ActualizarDiagnostico>();
        servicios.AddScoped<EliminarDiagnostico>();
        servicios.AddScoped<ListarDiagnosticos>();
        servicios.AddScoped<ObtenerHistorialMascota>();

        return servicios;
    }
}
