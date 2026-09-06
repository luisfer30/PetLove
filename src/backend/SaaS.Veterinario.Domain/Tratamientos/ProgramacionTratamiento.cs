using SaaS.Veterinario.Domain.Errores;

namespace SaaS.Veterinario.Domain.Tratamientos;

/// <summary>
/// Representa UNA actividad concreta programada en una fecha/hora determinada (R10) --
/// deliberadamente distinta de un futuro Recordatorio (R11/R12, Etapa 9). Denormaliza
/// VeterinariaId y MascotaVeterinariaId (redundantes con el Plan/Item) para permitir consultas
/// de agenda directas sin joins, mismo criterio que otras entidades de la Etapa 7.
/// </summary>
public sealed class ProgramacionTratamiento
{
    private static readonly Dictionary<EstadoProgramacionTratamiento, EstadoProgramacionTratamiento[]> TransicionesPermitidas = new()
    {
        [EstadoProgramacionTratamiento.Pendiente] =
        [
            EstadoProgramacionTratamiento.Realizada,
            EstadoProgramacionTratamiento.Omitida,
            EstadoProgramacionTratamiento.Vencida,
            EstadoProgramacionTratamiento.Cancelada,
        ],
        [EstadoProgramacionTratamiento.Realizada] = [],
        [EstadoProgramacionTratamiento.Omitida] = [],
        [EstadoProgramacionTratamiento.Vencida] = [],
        [EstadoProgramacionTratamiento.Cancelada] = [],
    };

    public Guid Id { get; }
    public Guid VeterinariaId { get; }
    public Guid MascotaVeterinariaId { get; }
    public Guid PlanTratamientoId { get; }
    public Guid ItemTratamientoId { get; }
    public DateTimeOffset FechaHoraProgramada { get; }
    public EstadoProgramacionTratamiento Estado { get; private set; }
    public DateTimeOffset? FechaRealizacion { get; private set; }
    public Guid? RegistradoPorUsuarioId { get; private set; }
    public string? ObservacionRealizacion { get; private set; }
    public DateTimeOffset FechaCreacion { get; }
    public DateTimeOffset FechaActualizacion { get; private set; }

    private ProgramacionTratamiento(
        Guid id,
        Guid veterinariaId,
        Guid mascotaVeterinariaId,
        Guid planTratamientoId,
        Guid itemTratamientoId,
        DateTimeOffset fechaHoraProgramada,
        DateTimeOffset fechaCreacion)
    {
        Id = id;
        VeterinariaId = veterinariaId;
        MascotaVeterinariaId = mascotaVeterinariaId;
        PlanTratamientoId = planTratamientoId;
        ItemTratamientoId = itemTratamientoId;
        FechaHoraProgramada = fechaHoraProgramada;
        Estado = EstadoProgramacionTratamiento.Pendiente;
        FechaCreacion = fechaCreacion;
        FechaActualizacion = fechaCreacion;
    }

    public static ProgramacionTratamiento Crear(
        Guid veterinariaId,
        Guid mascotaVeterinariaId,
        Guid planTratamientoId,
        Guid itemTratamientoId,
        DateTimeOffset fechaHoraProgramada,
        DateTimeOffset? fechaCreacion = null)
    {
        if (veterinariaId == Guid.Empty)
        {
            throw new ExcepcionDominio("El identificador de la veterinaria es obligatorio.");
        }

        if (mascotaVeterinariaId == Guid.Empty)
        {
            throw new ExcepcionDominio("El identificador de la ficha de la mascota es obligatorio.");
        }

        if (planTratamientoId == Guid.Empty)
        {
            throw new ExcepcionDominio("El identificador del plan de tratamiento es obligatorio.");
        }

        if (itemTratamientoId == Guid.Empty)
        {
            throw new ExcepcionDominio("El identificador del item de tratamiento es obligatorio.");
        }

        return new ProgramacionTratamiento(
            Guid.NewGuid(), veterinariaId, mascotaVeterinariaId, planTratamientoId, itemTratamientoId,
            fechaHoraProgramada, fechaCreacion ?? DateTimeOffset.UtcNow);
    }

    public void MarcarRealizada(Guid registradoPorUsuarioId, string? observacion, DateTimeOffset momento)
    {
        CambiarEstado(EstadoProgramacionTratamiento.Realizada, momento);
        FechaRealizacion = momento;
        RegistradoPorUsuarioId = registradoPorUsuarioId;
        ObservacionRealizacion = string.IsNullOrWhiteSpace(observacion) ? null : observacion.Trim();
    }

    public void MarcarOmitida(Guid registradoPorUsuarioId, string? observacion, DateTimeOffset momento)
    {
        CambiarEstado(EstadoProgramacionTratamiento.Omitida, momento);
        RegistradoPorUsuarioId = registradoPorUsuarioId;
        ObservacionRealizacion = string.IsNullOrWhiteSpace(observacion) ? null : observacion.Trim();
    }

    /// <summary>Sin usuario: pensado para un proceso reusable (seccion 20), no para una accion manual de una persona.</summary>
    public void MarcarVencida(DateTimeOffset momento) => CambiarEstado(EstadoProgramacionTratamiento.Vencida, momento);

    /// <summary>No modifica REALIZADA/OMITIDA/VENCIDA (seccion 21) -- CambiarEstado ya lo garantiza via la tabla de transiciones.</summary>
    public void Cancelar(DateTimeOffset momento) => CambiarEstado(EstadoProgramacionTratamiento.Cancelada, momento);

    private void CambiarEstado(EstadoProgramacionTratamiento nuevoEstado, DateTimeOffset momento)
    {
        if (!TransicionesPermitidas[Estado].Contains(nuevoEstado))
        {
            throw new ExcepcionDominio($"No se puede transicionar la programación de '{Estado}' a '{nuevoEstado}'.");
        }

        Estado = nuevoEstado;
        FechaActualizacion = momento;
    }
}
