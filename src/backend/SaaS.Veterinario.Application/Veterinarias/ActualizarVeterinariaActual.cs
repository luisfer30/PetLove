using SaaS.Veterinario.Application.Abstracciones;
using SaaS.Veterinario.Application.Errores;

namespace SaaS.Veterinario.Application.Veterinarias;

public sealed record ActualizarVeterinariaActualComando(
    string NombreComercial,
    string? RazonSocial,
    string? Ruc,
    string? Telefono,
    string? Direccion,
    string? Ciudad,
    string Pais,
    string ZonaHoraria);

/// <summary>
/// VeterinariaId nunca llega como parametro: siempre sale de IContextoVeterinaria, que ya fue
/// resuelto para esta request antes de que el endpoint (protegido por RequierePermiso) se
/// ejecute.
/// </summary>
public sealed class ActualizarVeterinariaActual(
    IContextoVeterinaria contexto,
    IRepositorioVeterinarias repositorioVeterinarias)
{
    public async Task EjecutarAsync(ActualizarVeterinariaActualComando comando, CancellationToken cancellationToken)
    {
        // El endpoint que invoca este caso de uso exige RequierePermiso("veterinaria.editar"),
        // lo que ya garantiza TieneContextoOperativo == true (ver PermisoAuthorizationHandler)
        // -- si VeterinariaId fuera null aqui seria un error de programacion, no un caso de
        // negocio esperable, de ahi InvalidOperationException en vez de ExcepcionAplicacion.
        var veterinariaId = contexto.VeterinariaId
            ?? throw new InvalidOperationException("Se invocó ActualizarVeterinariaActual sin contexto veterinario resuelto.");

        var veterinaria = await repositorioVeterinarias.ObtenerPorIdAsync(veterinariaId, cancellationToken)
            ?? throw new ExcepcionAplicacion(CodigosError.VeterinariaNoDisponible, "La veterinaria del contexto actual no existe.");

        veterinaria.ActualizarInformacion(
            comando.NombreComercial,
            comando.RazonSocial,
            comando.Ruc,
            comando.Telefono,
            comando.Direccion,
            comando.Ciudad,
            comando.Pais,
            comando.ZonaHoraria,
            DateTimeOffset.UtcNow);

        await repositorioVeterinarias.GuardarCambiosAsync(cancellationToken);
    }
}
