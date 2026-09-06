using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using SaaS.Veterinario.Domain.Citas;
using SaaS.Veterinario.Domain.Consultas;

namespace SaaS.Veterinario.Infrastructure.Persistencia.Conversiones;

/// <summary>Converters de la Etapa 7 (Agenda + Consultas + Diagnosticos). Ver FabricaConversorEnum.</summary>
internal static class ConversionesEstadosClinico
{
    public static ValueConverter<EstadoCita, string> ConversorEstadoCita { get; } = FabricaConversorEnum.Crear(
        new Dictionary<EstadoCita, string>
        {
            [EstadoCita.Programada] = "PROGRAMADA",
            [EstadoCita.Confirmada] = "CONFIRMADA",
            [EstadoCita.EnEspera] = "EN_ESPERA",
            [EstadoCita.Atendida] = "ATENDIDA",
            [EstadoCita.Cancelada] = "CANCELADA",
            [EstadoCita.NoAsistio] = "NO_ASISTIO",
        });

    public static ValueConverter<EstadoConsulta, string> ConversorEstadoConsulta { get; } = FabricaConversorEnum.Crear(
        new Dictionary<EstadoConsulta, string>
        {
            [EstadoConsulta.Borrador] = "BORRADOR",
            [EstadoConsulta.Finalizada] = "FINALIZADA",
            [EstadoConsulta.Anulada] = "ANULADA",
        });

    public static ValueConverter<TipoDiagnostico, string> ConversorTipoDiagnostico { get; } = FabricaConversorEnum.Crear(
        new Dictionary<TipoDiagnostico, string>
        {
            [TipoDiagnostico.Presuntivo] = "PRESUNTIVO",
            [TipoDiagnostico.Confirmado] = "CONFIRMADO",
            [TipoDiagnostico.Diferencial] = "DIFERENCIAL",
            [TipoDiagnostico.Otro] = "OTRO",
        });
}
