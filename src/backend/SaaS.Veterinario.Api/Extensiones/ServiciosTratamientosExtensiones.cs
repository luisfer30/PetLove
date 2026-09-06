using SaaS.Veterinario.Application.Abstracciones;
using SaaS.Veterinario.Application.Seguimientos;
using SaaS.Veterinario.Application.Tratamientos;
using SaaS.Veterinario.Infrastructure.Seguimientos;
using SaaS.Veterinario.Infrastructure.Tratamientos;

namespace SaaS.Veterinario.Api.Extensiones;

public static class ServiciosTratamientosExtensiones
{
    public static IServiceCollection AgregarCasosDeUsoDeTratamientos(this IServiceCollection servicios)
    {
        servicios.AddScoped<IRepositorioPlanesTratamiento, RepositorioPlanesTratamiento>();
        servicios.AddScoped<IRepositorioItemsTratamiento, RepositorioItemsTratamiento>();
        servicios.AddScoped<IRepositorioProgramacionesTratamiento, RepositorioProgramacionesTratamiento>();
        servicios.AddScoped<IRepositorioSeguimientos, RepositorioSeguimientos>();
        servicios.AddScoped<IGeneradorProgramacionesTratamiento, GeneradorProgramacionesTratamiento>();

        servicios.AddScoped<CrearPlanTratamiento>();
        servicios.AddScoped<ActualizarPlanTratamiento>();
        servicios.AddScoped<ActivarPlanTratamiento>();
        servicios.AddScoped<ReanudarPlanTratamiento>();
        servicios.AddScoped<SuspenderPlanTratamiento>();
        servicios.AddScoped<CompletarPlanTratamiento>();
        servicios.AddScoped<CancelarPlanTratamiento>();
        servicios.AddScoped<ObtenerPlanTratamiento>();
        servicios.AddScoped<ListarPlanesTratamiento>();

        servicios.AddScoped<AgregarItemTratamiento>();
        servicios.AddScoped<ActualizarItemTratamiento>();
        servicios.AddScoped<SuspenderItemTratamiento>();
        servicios.AddScoped<CancelarItemTratamiento>();
        servicios.AddScoped<ListarItemsTratamiento>();

        servicios.AddScoped<RegistrarProgramacionRealizada>();
        servicios.AddScoped<MarcarProgramacionOmitida>();
        servicios.AddScoped<MarcarProgramacionesVencidas>();
        servicios.AddScoped<ListarProgramacionesPorPlan>();
        servicios.AddScoped<ListarProgramacionesPorMascota>();
        servicios.AddScoped<ObtenerAdherencia>();

        servicios.AddScoped<CrearSeguimientoManual>();
        servicios.AddScoped<MarcarSeguimientoAgendado>();
        servicios.AddScoped<RegistrarSeguimientoRealizado>();
        servicios.AddScoped<CancelarSeguimiento>();
        servicios.AddScoped<ListarSeguimientos>();

        return servicios;
    }
}
