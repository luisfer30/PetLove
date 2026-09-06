using SaaS.Veterinario.Application.Abstracciones;
using SaaS.Veterinario.Application.Mascotas;
using SaaS.Veterinario.Domain.Mascotas;

namespace SaaS.Veterinario.Api.Mascotas;

public sealed record MascotaFichaResponse(
    Guid MascotaVeterinariaId,
    Guid MascotaId,
    string CodigoPublico,
    string Nombre,
    string Especie,
    string? Raza,
    string Sexo,
    string? NumeroHistoria,
    string Estado)
{
    public static MascotaFichaResponse DeInfo(InfoMascotaFicha info)
        => new(info.MascotaVeterinariaId, info.MascotaId, info.CodigoPublico, info.Nombre, info.Especie, info.Raza, info.Sexo, info.NumeroHistoria, info.EstadoLocal);
}

public sealed record RegistrarMascotaRequest(
    string Nombre,
    Guid EspecieId,
    Guid? RazaId,
    string Sexo,
    DateOnly? FechaNacimiento,
    bool FechaNacimientoAproximada,
    string? Color,
    string? NumeroMicrochip,
    bool? Esterilizado,
    string? NumeroHistoria);

public sealed record RegistrarMascotaResponse(Guid MascotaVeterinariaId, Guid MascotaId, string CodigoPublico);

public sealed record MascotaEncontradaResponse(
    string CodigoPublico,
    string Nombre,
    string Especie,
    string? Raza,
    string Sexo,
    DateOnly? FechaNacimiento,
    string? NumeroMicrochip)
{
    public static MascotaEncontradaResponse DeAplicacion(MascotaEncontrada encontrada)
        => new(encontrada.CodigoPublico, encontrada.Nombre, encontrada.Especie, encontrada.Raza, encontrada.Sexo, encontrada.FechaNacimiento, encontrada.NumeroMicrochip);
}

public sealed record VincularMascotaRequest(string CodigoPublico, string? NumeroHistoria);

public sealed record VincularMascotaResponse(Guid MascotaVeterinariaId, string CodigoPublico, string Nombre);

public sealed record ResponsableMascotaResponse(
    Guid Id,
    Guid? ClienteVeterinariaId,
    string Nombre,
    string? Correo,
    string? Telefono,
    string TipoRelacion,
    bool EsPrincipal,
    bool PuedeVerHistorial,
    bool PuedeGestionarTratamientos,
    string EstadoVinculacion,
    string Estado)
{
    public static ResponsableMascotaResponse DeDominio(ResponsableMascota responsable)
        => new(
            responsable.Id,
            responsable.ClienteVeterinariaId,
            responsable.Nombre,
            responsable.Correo?.Valor,
            responsable.Telefono,
            responsable.TipoRelacion.ToString(),
            responsable.EsPrincipal,
            responsable.PuedeVerHistorial,
            responsable.PuedeGestionarTratamientos,
            responsable.EstadoVinculacion.ToString(),
            responsable.Estado.ToString());
}

public sealed record RegistrarResponsableRequest(
    Guid? ClienteVeterinariaId,
    string Nombre,
    string? Correo,
    string? Telefono,
    string TipoRelacion,
    bool EsPrincipal,
    bool PuedeVerHistorial,
    bool PuedeGestionarTratamientos);

public sealed record RegistrarResponsableResponse(Guid ResponsableId);
