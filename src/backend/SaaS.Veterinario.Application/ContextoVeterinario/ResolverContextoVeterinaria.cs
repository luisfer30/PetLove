using SaaS.Veterinario.Application.Abstracciones;
using SaaS.Veterinario.Domain.Veterinarias;

namespace SaaS.Veterinario.Application.ContextoVeterinario;

/// <summary>
/// Orquesta la resolucion JWT(sub) -> MembresiaVeterinaria ACTIVA -> Veterinaria ACTIVA ->
/// Roles -> Permisos. Vive en Application (no en Infrastructure/Api) porque decidir que la
/// veterinaria tambien deba estar ACTIVA para considerar el contexto "operativo" es una regla
/// de negocio, no un detalle de persistencia o de HTTP.
/// </summary>
public sealed class ResolverContextoVeterinaria(
    IRepositorioMembresias repositorioMembresias,
    IRepositorioVeterinarias repositorioVeterinarias)
{
    public async Task<ResultadoContexto> EjecutarAsync(Guid usuarioId, CancellationToken cancellationToken)
    {
        var info = await repositorioMembresias.ObtenerContextoActivoAsync(usuarioId, cancellationToken);

        if (info is null)
        {
            return ResultadoContexto.SinMembresia();
        }

        var veterinaria = await repositorioVeterinarias.ObtenerPorIdAsync(info.VeterinariaId, cancellationToken);

        if (veterinaria is null || veterinaria.Estado != EstadoVeterinaria.Activa)
        {
            return ResultadoContexto.VeterinariaNoDisponible(info.MembresiaId, info.VeterinariaId);
        }

        return ResultadoContexto.Disponible(info.MembresiaId, info.VeterinariaId, info.Roles, info.Permisos);
    }
}

public sealed record ResultadoContexto(
    bool TieneMembresiaActiva,
    bool TieneContextoOperativo,
    Guid? MembresiaId,
    Guid? VeterinariaId,
    IReadOnlyCollection<string> Roles,
    IReadOnlyCollection<string> Permisos)
{
    public static ResultadoContexto SinMembresia() => new(false, false, null, null, [], []);

    public static ResultadoContexto VeterinariaNoDisponible(Guid membresiaId, Guid veterinariaId) =>
        new(true, false, membresiaId, veterinariaId, [], []);

    public static ResultadoContexto Disponible(
        Guid membresiaId, Guid veterinariaId, IReadOnlyCollection<string> roles, IReadOnlyCollection<string> permisos) =>
        new(true, true, membresiaId, veterinariaId, roles, permisos);
}
