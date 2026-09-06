using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using SaaS.Veterinario.Domain.Seguimientos;
using SaaS.Veterinario.Domain.Tratamientos;

namespace SaaS.Veterinario.Infrastructure.Persistencia.Conversiones;

/// <summary>Converters de la Etapa 8 (Tratamientos + Programaciones + Seguimiento clinico). Ver FabricaConversorEnum.</summary>
internal static class ConversionesEstadosTratamiento
{
    public static ValueConverter<EstadoPlanTratamiento, string> ConversorEstadoPlanTratamiento { get; } = FabricaConversorEnum.Crear(
        new Dictionary<EstadoPlanTratamiento, string>
        {
            [EstadoPlanTratamiento.Pendiente] = "PENDIENTE",
            [EstadoPlanTratamiento.Activo] = "ACTIVO",
            [EstadoPlanTratamiento.Suspendido] = "SUSPENDIDO",
            [EstadoPlanTratamiento.Completado] = "COMPLETADO",
            [EstadoPlanTratamiento.Cancelado] = "CANCELADO",
        });

    public static ValueConverter<TipoItemTratamiento, string> ConversorTipoItemTratamiento { get; } = FabricaConversorEnum.Crear(
        new Dictionary<TipoItemTratamiento, string>
        {
            [TipoItemTratamiento.Medicamento] = "MEDICAMENTO",
            [TipoItemTratamiento.Cuidado] = "CUIDADO",
            [TipoItemTratamiento.Alimentacion] = "ALIMENTACION",
            [TipoItemTratamiento.Reposo] = "REPOSO",
            [TipoItemTratamiento.Terapia] = "TERAPIA",
            [TipoItemTratamiento.Otro] = "OTRO",
        });

    public static ValueConverter<EstadoItemTratamiento, string> ConversorEstadoItemTratamiento { get; } = FabricaConversorEnum.Crear(
        new Dictionary<EstadoItemTratamiento, string>
        {
            [EstadoItemTratamiento.Activo] = "ACTIVO",
            [EstadoItemTratamiento.Suspendido] = "SUSPENDIDO",
            [EstadoItemTratamiento.Completado] = "COMPLETADO",
            [EstadoItemTratamiento.Cancelado] = "CANCELADO",
        });

    public static ValueConverter<TipoFrecuenciaTratamiento, string> ConversorTipoFrecuenciaTratamiento { get; } = FabricaConversorEnum.Crear(
        new Dictionary<TipoFrecuenciaTratamiento, string>
        {
            [TipoFrecuenciaTratamiento.IntervaloHoras] = "INTERVALO_HORAS",
            [TipoFrecuenciaTratamiento.VecesPorDia] = "VECES_POR_DIA",
            [TipoFrecuenciaTratamiento.HoraFija] = "HORA_FIJA",
            [TipoFrecuenciaTratamiento.Unica] = "UNICA",
            [TipoFrecuenciaTratamiento.SegunNecesidad] = "SEGUN_NECESIDAD",
        });

    public static ValueConverter<ViaAdministracion, string> ConversorViaAdministracion { get; } = FabricaConversorEnum.Crear(
        new Dictionary<ViaAdministracion, string>
        {
            [ViaAdministracion.Oral] = "ORAL",
            [ViaAdministracion.Topica] = "TOPICA",
            [ViaAdministracion.Subcutanea] = "SUBCUTANEA",
            [ViaAdministracion.Intramuscular] = "INTRAMUSCULAR",
            [ViaAdministracion.Intravenosa] = "INTRAVENOSA",
            [ViaAdministracion.Oftalmica] = "OFTALMICA",
            [ViaAdministracion.Otica] = "OTICA",
            [ViaAdministracion.Inhalada] = "INHALADA",
            [ViaAdministracion.Otra] = "OTRA",
        });

    public static ValueConverter<EstadoProgramacionTratamiento, string> ConversorEstadoProgramacionTratamiento { get; } = FabricaConversorEnum.Crear(
        new Dictionary<EstadoProgramacionTratamiento, string>
        {
            [EstadoProgramacionTratamiento.Pendiente] = "PENDIENTE",
            [EstadoProgramacionTratamiento.Realizada] = "REALIZADA",
            [EstadoProgramacionTratamiento.Omitida] = "OMITIDA",
            [EstadoProgramacionTratamiento.Vencida] = "VENCIDA",
            [EstadoProgramacionTratamiento.Cancelada] = "CANCELADA",
        });

    public static ValueConverter<EstadoSeguimientoClinico, string> ConversorEstadoSeguimientoClinico { get; } = FabricaConversorEnum.Crear(
        new Dictionary<EstadoSeguimientoClinico, string>
        {
            [EstadoSeguimientoClinico.Pendiente] = "PENDIENTE",
            [EstadoSeguimientoClinico.Agendado] = "AGENDADO",
            [EstadoSeguimientoClinico.Realizado] = "REALIZADO",
            [EstadoSeguimientoClinico.Vencido] = "VENCIDO",
            [EstadoSeguimientoClinico.Cancelado] = "CANCELADO",
        });

    public static ValueConverter<TipoSeguimientoClinico, string> ConversorTipoSeguimientoClinico { get; } = FabricaConversorEnum.Crear(
        new Dictionary<TipoSeguimientoClinico, string>
        {
            [TipoSeguimientoClinico.Control] = "CONTROL",
            [TipoSeguimientoClinico.RevisionTratamiento] = "REVISION_TRATAMIENTO",
            [TipoSeguimientoClinico.Examen] = "EXAMEN",
            [TipoSeguimientoClinico.PostOperatorio] = "POST_OPERATORIO",
            [TipoSeguimientoClinico.Otro] = "OTRO",
        });
}
