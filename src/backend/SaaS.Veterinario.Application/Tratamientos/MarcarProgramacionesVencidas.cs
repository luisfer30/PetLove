using SaaS.Veterinario.Application.Abstracciones;

namespace SaaS.Veterinario.Application.Tratamientos;

public sealed record MarcarProgramacionesVencidasResultado(int CantidadMarcadas);

/// <summary>
/// Logica reusable (seccion 20): marca PENDIENTE -> VENCIDA toda programacion cuya
/// FechaHoraProgramada sea anterior a la fecha de corte recibida. Deliberadamente NO se expone
/// via endpoint HTTP ni se ejecuta automaticamente en esta etapa -- queda lista para que la
/// Etapa 9 la invoque desde su motor de recordatorios/jobs. No asume ningun margen de
/// vencimiento por si misma: el llamador decide la fecha de corte.
/// </summary>
public sealed class MarcarProgramacionesVencidas(IRepositorioProgramacionesTratamiento repositorioProgramaciones)
{
    public async Task<MarcarProgramacionesVencidasResultado> EjecutarAsync(DateTimeOffset fechaCorte, CancellationToken cancellationToken)
    {
        var vencidas = await repositorioProgramaciones.ListarPendientesVencidasAsync(fechaCorte, cancellationToken);

        foreach (var programacion in vencidas)
        {
            programacion.MarcarVencida(DateTimeOffset.UtcNow);
        }

        if (vencidas.Count > 0)
        {
            await repositorioProgramaciones.GuardarCambiosAsync(cancellationToken);
        }

        return new MarcarProgramacionesVencidasResultado(vencidas.Count);
    }
}
