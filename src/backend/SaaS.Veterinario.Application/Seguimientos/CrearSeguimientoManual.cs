using SaaS.Veterinario.Application.Abstracciones;
using SaaS.Veterinario.Application.Errores;
using SaaS.Veterinario.Domain.Seguimientos;

namespace SaaS.Veterinario.Application.Seguimientos;

public sealed record CrearSeguimientoManualComando(
    Guid MascotaVeterinariaId, Guid? ConsultaOrigenId, Guid? PlanTratamientoId, string Tipo, DateTimeOffset FechaObjetivo, string Motivo, string? Notas);

public sealed record CrearSeguimientoManualResultado(Guid SeguimientoId);

public sealed class CrearSeguimientoManual(
    IContextoVeterinaria contexto,
    IRepositorioMascotasVeterinarias repositorioMascotasVeterinarias,
    IRepositorioSeguimientos repositorioSeguimientos)
{
    public async Task<CrearSeguimientoManualResultado> EjecutarAsync(CrearSeguimientoManualComando comando, CancellationToken cancellationToken)
    {
        var veterinariaId = contexto.VeterinariaId
            ?? throw new InvalidOperationException("Se invocó CrearSeguimientoManual sin contexto veterinario resuelto.");

        _ = await repositorioMascotasVeterinarias.ObtenerPorIdAsync(comando.MascotaVeterinariaId, veterinariaId, cancellationToken)
            ?? throw new ExcepcionAplicacion(CodigosError.MascotaVeterinariaNoEncontrada, "La ficha de la mascota no existe en esta veterinaria.");

        if (!Enum.TryParse<TipoSeguimientoClinico>(comando.Tipo, ignoreCase: true, out var tipo))
        {
            throw new ExcepcionAplicacion(CodigosError.SeguimientoEstadoInvalido, "El tipo de seguimiento indicado no es válido.");
        }

        var seguimiento = SeguimientoClinico.Crear(
            veterinariaId, comando.MascotaVeterinariaId, comando.ConsultaOrigenId, comando.PlanTratamientoId,
            tipo, comando.FechaObjetivo, comando.Motivo, comando.Notas, contexto.UsuarioId);

        await repositorioSeguimientos.AgregarAsync(seguimiento, cancellationToken);

        return new CrearSeguimientoManualResultado(seguimiento.Id);
    }
}
