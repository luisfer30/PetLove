using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using SaaS.Veterinario.Domain.Clientes;
using SaaS.Veterinario.Domain.Mascotas;

namespace SaaS.Veterinario.Infrastructure.Persistencia.Conversiones;

/// <summary>Converters de la Etapa 6 (Clientes + Mascotas). Ver FabricaConversorEnum.</summary>
internal static class ConversionesEstadosMascotas
{
    public static ValueConverter<EstadoEspecie, string> ConversorEstadoEspecie { get; } = FabricaConversorEnum.Crear(
        new Dictionary<EstadoEspecie, string>
        {
            [EstadoEspecie.Activo] = "ACTIVO",
            [EstadoEspecie.Inactivo] = "INACTIVO",
        });

    public static ValueConverter<EstadoRaza, string> ConversorEstadoRaza { get; } = FabricaConversorEnum.Crear(
        new Dictionary<EstadoRaza, string>
        {
            [EstadoRaza.Activo] = "ACTIVO",
            [EstadoRaza.Inactivo] = "INACTIVO",
        });

    public static ValueConverter<EstadoMascota, string> ConversorEstadoMascota { get; } = FabricaConversorEnum.Crear(
        new Dictionary<EstadoMascota, string>
        {
            [EstadoMascota.Activa] = "ACTIVA",
            [EstadoMascota.Inactiva] = "INACTIVA",
            [EstadoMascota.Fallecida] = "FALLECIDA",
        });

    public static ValueConverter<SexoMascota, string> ConversorSexoMascota { get; } = FabricaConversorEnum.Crear(
        new Dictionary<SexoMascota, string>
        {
            [SexoMascota.Macho] = "MACHO",
            [SexoMascota.Hembra] = "HEMBRA",
            [SexoMascota.Desconocido] = "DESCONOCIDO",
        });

    public static ValueConverter<EstadoClienteVeterinaria, string> ConversorEstadoClienteVeterinaria { get; } = FabricaConversorEnum.Crear(
        new Dictionary<EstadoClienteVeterinaria, string>
        {
            [EstadoClienteVeterinaria.Activo] = "ACTIVO",
            [EstadoClienteVeterinaria.Inactivo] = "INACTIVO",
        });

    public static ValueConverter<EstadoMascotaVeterinaria, string> ConversorEstadoMascotaVeterinaria { get; } = FabricaConversorEnum.Crear(
        new Dictionary<EstadoMascotaVeterinaria, string>
        {
            [EstadoMascotaVeterinaria.Activa] = "ACTIVA",
            [EstadoMascotaVeterinaria.Inactiva] = "INACTIVA",
        });

    public static ValueConverter<TipoRelacionResponsable, string> ConversorTipoRelacionResponsable { get; } = FabricaConversorEnum.Crear(
        new Dictionary<TipoRelacionResponsable, string>
        {
            [TipoRelacionResponsable.Propietario] = "PROPIETARIO",
            [TipoRelacionResponsable.Familiar] = "FAMILIAR",
            [TipoRelacionResponsable.Cuidador] = "CUIDADOR",
            [TipoRelacionResponsable.Otro] = "OTRO",
        });

    public static ValueConverter<EstadoVinculacionResponsable, string> ConversorEstadoVinculacionResponsable { get; } = FabricaConversorEnum.Crear(
        new Dictionary<EstadoVinculacionResponsable, string>
        {
            [EstadoVinculacionResponsable.NoVinculado] = "NO_VINCULADO",
            [EstadoVinculacionResponsable.Pendiente] = "PENDIENTE",
            [EstadoVinculacionResponsable.Vinculado] = "VINCULADO",
        });

    public static ValueConverter<EstadoResponsableMascota, string> ConversorEstadoResponsableMascota { get; } = FabricaConversorEnum.Crear(
        new Dictionary<EstadoResponsableMascota, string>
        {
            [EstadoResponsableMascota.Activo] = "ACTIVO",
            [EstadoResponsableMascota.Inactivo] = "INACTIVO",
        });
}
