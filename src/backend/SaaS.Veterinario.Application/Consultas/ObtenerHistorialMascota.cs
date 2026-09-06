using SaaS.Veterinario.Application.Abstracciones;
using SaaS.Veterinario.Application.Errores;

namespace SaaS.Veterinario.Application.Consultas;

public sealed record HistorialEvento(string Tipo, DateTimeOffset FechaHora, string Titulo, string Estado, string? Resumen);

public sealed record HistorialMascotaResultado(
    Guid MascotaVeterinariaId, string CodigoPublicoMascota, string NombreMascota, IReadOnlyCollection<HistorialEvento> Eventos);

/// <summary>
/// El historial es independiente por MascotaVeterinaria (R10): consultas/citas/diagnosticos de
/// la MISMA mascota global en OTRA veterinaria nunca aparecen aqui, porque todo se filtra por
/// (mascotaVeterinariaId, veterinariaId) desde el inicio, nunca solo por la identidad global.
/// </summary>
public sealed class ObtenerHistorialMascota(
    IContextoVeterinaria contexto,
    IRepositorioMascotasVeterinarias repositorioMascotasVeterinarias,
    IRepositorioCitas repositorioCitas,
    IRepositorioConsultas repositorioConsultas,
    IRepositorioDiagnosticos repositorioDiagnosticos)
{
    public async Task<HistorialMascotaResultado> EjecutarAsync(Guid mascotaVeterinariaId, CancellationToken cancellationToken)
    {
        var veterinariaId = contexto.VeterinariaId
            ?? throw new InvalidOperationException("Se invocó ObtenerHistorialMascota sin contexto veterinario resuelto.");

        var ficha = await repositorioMascotasVeterinarias.ObtenerFichaPorIdAsync(mascotaVeterinariaId, veterinariaId, cancellationToken)
            ?? throw new ExcepcionAplicacion(CodigosError.MascotaVeterinariaNoEncontrada, "La ficha de la mascota no existe en esta veterinaria.");

        var citas = await repositorioCitas.ListarPorMascotaVeterinariaAsync(mascotaVeterinariaId, veterinariaId, cancellationToken);
        var consultas = await repositorioConsultas.ListarPorMascotaVeterinariaAsync(mascotaVeterinariaId, veterinariaId, cancellationToken);
        var diagnosticos = await repositorioDiagnosticos.ListarPorConsultasAsync(consultas.Select(c => c.Id).ToList(), cancellationToken);

        var eventos = citas
            .Select(c => new HistorialEvento("Cita", c.FechaHoraInicio, c.Motivo, c.Estado.ToString(), c.Observaciones))
            .Concat(consultas.Select(c => new HistorialEvento("Consulta", c.FechaHora, c.MotivoConsulta, c.Estado.ToString(), c.ObservacionesClinicas)))
            .Concat(diagnosticos.Select(d => new HistorialEvento(
                "Diagnostico", d.FechaCreacion, d.Descripcion, d.Tipo.ToString(), d.EsPrincipal ? "Diagnóstico principal" : null)))
            .OrderByDescending(e => e.FechaHora)
            .ToList();

        return new HistorialMascotaResultado(ficha.MascotaVeterinariaId, ficha.CodigoPublico, ficha.Nombre, eventos);
    }
}
