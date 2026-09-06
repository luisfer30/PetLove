using SaaS.Veterinario.Application.Abstracciones;
using SaaS.Veterinario.Domain.Tratamientos;

namespace SaaS.Veterinario.Infrastructure.Tratamientos;

/// <summary>
/// Genera los instantes de cada ProgramacionTratamiento respetando la ZonaHoraria de la
/// Veterinaria (seccion 23/28): para HORA_FIJA/VECES_POR_DIA, cada horario se interpreta como
/// hora de pared LOCAL en esa zona y se convierte al offset correcto para esa fecha especifica
/// (TimeZoneInfo.GetUtcOffset por instante, no un offset fijo copiado), lo que maneja
/// correctamente cualquier zona con cambios de horario de verano. INTERVALO_HORAS, en cambio,
/// es aritmetica de duracion pura sobre el instante absoluto (sumar N horas es correcto en
/// cualquier zona sin necesidad de reinterpretar hora de pared).
/// </summary>
public sealed class GeneradorProgramacionesTratamiento : IGeneradorProgramacionesTratamiento
{
    public IReadOnlyCollection<DateTimeOffset> Generar(ParametrosGeneracionProgramaciones parametros)
        => parametros.FrecuenciaTipo switch
        {
            TipoFrecuenciaTratamiento.SegunNecesidad => [],
            TipoFrecuenciaTratamiento.Unica => [parametros.FechaInicio],
            TipoFrecuenciaTratamiento.IntervaloHoras => GenerarPorIntervalo(parametros),
            TipoFrecuenciaTratamiento.HoraFija or TipoFrecuenciaTratamiento.VecesPorDia => GenerarPorHorariosLocales(parametros),
            _ => [],
        };

    private static IReadOnlyCollection<DateTimeOffset> GenerarPorIntervalo(ParametrosGeneracionProgramaciones parametros)
    {
        var fechaFin = ResolverFechaFin(parametros);
        if (fechaFin is null || parametros.IntervaloHoras is not > 0)
        {
            return [];
        }

        var resultado = new List<DateTimeOffset>();
        var actual = parametros.FechaInicio;

        while (actual < fechaFin.Value)
        {
            resultado.Add(actual);
            actual = actual.AddHours(parametros.IntervaloHoras.Value);
        }

        return resultado;
    }

    private static IReadOnlyCollection<DateTimeOffset> GenerarPorHorariosLocales(ParametrosGeneracionProgramaciones parametros)
    {
        var fechaFin = ResolverFechaFin(parametros);
        if (fechaFin is null || parametros.Horarios.Count == 0)
        {
            return [];
        }

        var zona = TimeZoneInfo.FindSystemTimeZoneById(parametros.ZonaHorariaVeterinaria);
        var inicioLocal = TimeZoneInfo.ConvertTime(parametros.FechaInicio, zona);
        var finLocal = TimeZoneInfo.ConvertTime(fechaFin.Value, zona);
        var horariosOrdenados = parametros.Horarios.OrderBy(h => h).ToList();

        var resultado = new List<DateTimeOffset>();

        for (var fecha = inicioLocal.Date; fecha <= finLocal.Date; fecha = fecha.AddDays(1))
        {
            foreach (var hora in horariosOrdenados)
            {
                var horaLocalIngenua = fecha + hora.ToTimeSpan();
                var offset = zona.GetUtcOffset(horaLocalIngenua);
                var instante = new DateTimeOffset(horaLocalIngenua, offset);

                if (instante >= parametros.FechaInicio && instante <= fechaFin.Value)
                {
                    // Postgres/Npgsql exige Offset=0 al escribir timestamptz (el valor interno
                    // siempre es UTC; Npgsql rechaza cualquier DateTimeOffset con otro offset,
                    // incluso representando el mismo instante). Se normaliza aqui, ya con la
                    // conversion de zona horaria ya aplicada -- el instante absoluto no cambia.
                    resultado.Add(instante.ToUniversalTime());
                }
            }
        }

        return resultado;
    }

    private static DateTimeOffset? ResolverFechaFin(ParametrosGeneracionProgramaciones parametros)
        => parametros.FechaFin ?? (parametros.DuracionDias is { } dias ? parametros.FechaInicio.AddDays(dias) : null);
}
