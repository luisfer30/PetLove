using SaaS.Veterinario.Application.Abstracciones;
using SaaS.Veterinario.Application.Errores;
using SaaS.Veterinario.Domain.Mascotas;

namespace SaaS.Veterinario.Application.Mascotas;

public sealed record RegistrarResponsableComando(
    Guid MascotaVeterinariaId,
    Guid? ClienteVeterinariaId,
    string Nombre,
    string? Correo,
    string? Telefono,
    string TipoRelacion,
    bool EsPrincipal,
    bool PuedeVerHistorial,
    bool PuedeGestionarTratamientos);

public sealed record RegistrarResponsableResultado(Guid ResponsableId);

/// <summary>
/// UsuarioId nunca se acepta como parametro (seccion 25 de la Etapa 6: "no se acepta
/// libremente desde frontend en V1") -- el responsable siempre nace NO_VINCULADO.
/// </summary>
public sealed class RegistrarResponsableMascota(
    IContextoVeterinaria contexto,
    IRepositorioMascotasVeterinarias repositorioMascotasVeterinarias,
    IRepositorioClientes repositorioClientes,
    IRepositorioResponsables repositorioResponsables)
{
    public async Task<RegistrarResponsableResultado> EjecutarAsync(RegistrarResponsableComando comando, CancellationToken cancellationToken)
    {
        var veterinariaId = contexto.VeterinariaId
            ?? throw new InvalidOperationException("Se invocó RegistrarResponsableMascota sin contexto veterinario resuelto.");

        var mascotaVeterinaria = await repositorioMascotasVeterinarias.ObtenerPorIdAsync(comando.MascotaVeterinariaId, veterinariaId, cancellationToken)
            ?? throw new ExcepcionAplicacion(CodigosError.MascotaNoEncontrada, "La ficha de la mascota no existe en esta veterinaria.");

        if (comando.ClienteVeterinariaId is { } clienteId)
        {
            _ = await repositorioClientes.ObtenerPorIdAsync(clienteId, veterinariaId, cancellationToken)
                ?? throw new ExcepcionAplicacion(CodigosError.ClienteInvalido, "El cliente indicado no existe en esta veterinaria.");
        }

        if (!Enum.TryParse<TipoRelacionResponsable>(comando.TipoRelacion, ignoreCase: true, out var tipoRelacion))
        {
            throw new ExcepcionAplicacion(CodigosError.ResponsableTipoRelacionInvalido, "El tipo de relación indicado no es válido.");
        }

        if (comando.EsPrincipal && await repositorioResponsables.ExistePrincipalActivoAsync(mascotaVeterinaria.Id, cancellationToken))
        {
            throw new ExcepcionAplicacion(
                CodigosError.ResponsablePrincipalYaExiste,
                "Ya existe un responsable principal activo para esta mascota.");
        }

        var responsable = ResponsableMascota.Crear(
            veterinariaId,
            mascotaVeterinaria.Id,
            comando.ClienteVeterinariaId,
            comando.Nombre,
            comando.Correo,
            comando.Telefono,
            tipoRelacion,
            comando.EsPrincipal,
            comando.PuedeVerHistorial,
            comando.PuedeGestionarTratamientos);

        await repositorioResponsables.AgregarAsync(responsable, cancellationToken);

        return new RegistrarResponsableResultado(responsable.Id);
    }
}
