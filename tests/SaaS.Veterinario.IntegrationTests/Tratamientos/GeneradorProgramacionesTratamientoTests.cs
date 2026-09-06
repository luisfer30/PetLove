using SaaS.Veterinario.Application.Abstracciones;
using SaaS.Veterinario.Domain.Tratamientos;
using SaaS.Veterinario.Infrastructure.Tratamientos;

namespace SaaS.Veterinario.IntegrationTests.Tratamientos;

/// <summary>
/// Pruebas del generador de programaciones (seccion 59). No requieren PostgreSQL -- es logica
/// pura (aritmetica de fechas + TimeZoneInfo) -- por eso son [Fact] normales, no [SkippableFact];
/// viven en este proyecto porque es el unico que referencia Infrastructure directamente.
/// </summary>
public class GeneradorProgramacionesTratamientoTests
{
    private readonly GeneradorProgramacionesTratamiento generador = new();

    private static readonly DateTimeOffset InicioUtc = new(2026, 9, 6, 8, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Generar_IntervaloHoras_ProduceInstantesEquiespaciados()
    {
        var parametros = new ParametrosGeneracionProgramaciones(
            TipoFrecuenciaTratamiento.IntervaloHoras, InicioUtc, InicioUtc.AddDays(1), null, 8, [], "UTC");

        var resultado = generador.Generar(parametros);

        Assert.Equal(3, resultado.Count);
        Assert.Equal(InicioUtc, resultado.ElementAt(0));
        Assert.Equal(InicioUtc.AddHours(8), resultado.ElementAt(1));
        Assert.Equal(InicioUtc.AddHours(16), resultado.ElementAt(2));
    }

    [Fact]
    public void Generar_IntervaloHoras_CruzaMedianocheCorrectamente()
    {
        // 08:00 + 8h = 16:00 (mismo dia); +8h = 00:00 (dia siguiente) -- ambos deben incluirse
        // sin ningun tratamiento especial, ya que es aritmetica de instante absoluto.
        var parametros = new ParametrosGeneracionProgramaciones(
            TipoFrecuenciaTratamiento.IntervaloHoras, InicioUtc, InicioUtc.AddHours(20), null, 8, [], "UTC");

        var resultado = generador.Generar(parametros);

        Assert.Equal(3, resultado.Count);
        Assert.Contains(new DateTimeOffset(2026, 9, 7, 0, 0, 0, TimeSpan.Zero), resultado);
    }

    [Fact]
    public void Generar_HoraFijaConMultiplesHorarios_ProduceDosPorDia()
    {
        var horarios = new List<TimeOnly> { new(8, 0), new(20, 0) };
        var parametros = new ParametrosGeneracionProgramaciones(
            TipoFrecuenciaTratamiento.HoraFija, InicioUtc, InicioUtc.AddDays(2), null, null, horarios, "UTC");

        var resultado = generador.Generar(parametros);

        // Dias 6, 7 y 8 de septiembre -- pero el 8 solo cuenta la ocurrencia <= fechaFin (08:00).
        Assert.Equal(5, resultado.Count);
    }

    [Fact]
    public void Generar_HoraFijaEnRangoDeVariosDias_GeneraUnaOcurrenciaPorHorarioYDia()
    {
        // InicioUtc es 08:00 y el horario pedido es 09:00: el ultimo dia (10) su ocurrencia de
        // las 09:00 cae DESPUES del corte (fechaFin = dia 10 08:00, por DuracionDias=4) y por lo
        // tanto queda excluida -- solo los dias 6,7,8,9 producen una ocurrencia valida.
        var horarios = new List<TimeOnly> { new(9, 0) };
        var parametros = new ParametrosGeneracionProgramaciones(
            TipoFrecuenciaTratamiento.HoraFija, InicioUtc, null, 4, null, horarios, "UTC");

        var resultado = generador.Generar(parametros);

        Assert.Equal(4, resultado.Count);
    }

    [Fact]
    public void Generar_Unica_ProduceExactamenteUnaProgramacionEnFechaInicio()
    {
        var parametros = new ParametrosGeneracionProgramaciones(TipoFrecuenciaTratamiento.Unica, InicioUtc, null, null, null, [], "UTC");

        var resultado = generador.Generar(parametros);

        Assert.Single(resultado);
        Assert.Equal(InicioUtc, resultado.Single());
    }

    [Fact]
    public void Generar_SegunNecesidad_NoProduceNingunaProgramacion()
    {
        var parametros = new ParametrosGeneracionProgramaciones(TipoFrecuenciaTratamiento.SegunNecesidad, InicioUtc, InicioUtc.AddDays(30), null, null, [], "UTC");

        var resultado = generador.Generar(parametros);

        Assert.Empty(resultado);
    }

    [Fact]
    public void Generar_HoraFijaConZonaHorariaGuayaquil_ProduceElInstanteAbsolutoCorrecto()
    {
        // 08:00 hora clinica local en America/Guayaquil (UTC-5) representa el mismo instante
        // que 2026-09-06T13:00:00Z -- seccion 23/28/65. El resultado se normaliza a Offset=0
        // (ToUniversalTime) porque Npgsql exige timestamptz con offset UTC al escribir, pero el
        // INSTANTE ABSOLUTO debe ser exactamente el correspondiente a las 08:00 en Guayaquil, no
        // a las 08:00 UTC. No depende del timezone de la maquina que ejecuta el test:
        // America/Guayaquil se pasa explicitamente.
        var inicioEnGuayaquil = new DateTimeOffset(2026, 9, 6, 8, 0, 0, TimeSpan.FromHours(-5));
        var horarios = new List<TimeOnly> { new(8, 0) };
        var parametros = new ParametrosGeneracionProgramaciones(
            TipoFrecuenciaTratamiento.HoraFija, inicioEnGuayaquil, inicioEnGuayaquil, null, null, horarios, "America/Guayaquil");

        var resultado = generador.Generar(parametros);

        var instante = Assert.Single(resultado);
        // Normalizado para Postgres: Offset=0, pero representa el MISMO instante que 08:00-05:00.
        Assert.Equal(TimeSpan.Zero, instante.Offset);
        Assert.Equal(new DateTimeOffset(2026, 9, 6, 8, 0, 0, TimeSpan.FromHours(-5)), instante);
        // Vista de vuelta en la zona de Guayaquil, la hora de pared sigue siendo las 08:00.
        Assert.Equal(8, TimeZoneInfo.ConvertTime(instante, TimeZoneInfo.FindSystemTimeZoneById("America/Guayaquil")).Hour);
    }
}
