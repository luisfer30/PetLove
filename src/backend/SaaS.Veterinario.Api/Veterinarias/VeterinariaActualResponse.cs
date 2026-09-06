using SaaS.Veterinario.Domain.Veterinarias;

namespace SaaS.Veterinario.Api.Veterinarias;

public sealed record VeterinariaActualResponse(
    Guid Id,
    string CodigoPublico,
    string NombreComercial,
    string? RazonSocial,
    string? Ruc,
    string Correo,
    string? Telefono,
    string? Direccion,
    string? Ciudad,
    string Pais,
    string ZonaHoraria,
    string Estado)
{
    public static VeterinariaActualResponse DeDominio(Veterinaria veterinaria)
        => new(
            veterinaria.Id,
            veterinaria.CodigoPublico.Valor,
            veterinaria.NombreComercial,
            veterinaria.RazonSocial,
            veterinaria.Ruc,
            veterinaria.Correo.Valor,
            veterinaria.Telefono,
            veterinaria.Direccion,
            veterinaria.Ciudad,
            veterinaria.Pais,
            veterinaria.ZonaHoraria,
            veterinaria.Estado.ToString());
}

public sealed record ActualizarVeterinariaRequest(
    string NombreComercial,
    string? RazonSocial,
    string? Ruc,
    string? Telefono,
    string? Direccion,
    string? Ciudad,
    string Pais,
    string ZonaHoraria);
