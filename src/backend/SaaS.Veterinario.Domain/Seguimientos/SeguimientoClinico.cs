using SaaS.Veterinario.Domain.Errores;

namespace SaaS.Veterinario.Domain.Seguimientos;

/// <summary>
/// Puede crearse manualmente o automaticamente al finalizar una Consulta con
/// ProximaFechaControl (seccion 36/53) -- EsGeneradoDesdeProximoControl distingue ambos casos
/// para permitir un indice unico parcial que evite duplicar el seguimiento automatico sin
/// impedir que existan multiples seguimientos manuales para la misma Consulta.
/// </summary>
public sealed class SeguimientoClinico
{
    private static readonly Dictionary<EstadoSeguimientoClinico, EstadoSeguimientoClinico[]> TransicionesPermitidas = new()
    {
        [EstadoSeguimientoClinico.Pendiente] =
        [
            EstadoSeguimientoClinico.Agendado,
            EstadoSeguimientoClinico.Realizado,
            EstadoSeguimientoClinico.Vencido,
            EstadoSeguimientoClinico.Cancelado,
        ],
        [EstadoSeguimientoClinico.Agendado] =
        [
            EstadoSeguimientoClinico.Realizado,
            EstadoSeguimientoClinico.Vencido,
            EstadoSeguimientoClinico.Cancelado,
        ],
        [EstadoSeguimientoClinico.Realizado] = [],
        [EstadoSeguimientoClinico.Vencido] = [],
        [EstadoSeguimientoClinico.Cancelado] = [],
    };

    public Guid Id { get; }
    public Guid VeterinariaId { get; }
    public Guid MascotaVeterinariaId { get; }
    public Guid? ConsultaOrigenId { get; }
    public Guid? PlanTratamientoId { get; }
    public TipoSeguimientoClinico Tipo { get; }
    public DateTimeOffset FechaObjetivo { get; }
    public EstadoSeguimientoClinico Estado { get; private set; }
    public string Motivo { get; private set; }
    public string? Notas { get; private set; }
    public DateTimeOffset? FechaAgendada { get; private set; }
    public Guid? CitaId { get; private set; }
    public DateTimeOffset? FechaRealizacion { get; private set; }
    public bool EsGeneradoDesdeProximoControl { get; }
    public Guid CreadoPorUsuarioId { get; }
    public DateTimeOffset FechaCreacion { get; }
    public Guid? ActualizadoPorUsuarioId { get; private set; }
    public DateTimeOffset FechaActualizacion { get; private set; }

    private SeguimientoClinico(
        Guid id,
        Guid veterinariaId,
        Guid mascotaVeterinariaId,
        Guid? consultaOrigenId,
        Guid? planTratamientoId,
        TipoSeguimientoClinico tipo,
        DateTimeOffset fechaObjetivo,
        string motivo,
        string? notas,
        bool esGeneradoDesdeProximoControl,
        Guid creadoPorUsuarioId,
        DateTimeOffset fechaCreacion)
    {
        Id = id;
        VeterinariaId = veterinariaId;
        MascotaVeterinariaId = mascotaVeterinariaId;
        ConsultaOrigenId = consultaOrigenId;
        PlanTratamientoId = planTratamientoId;
        Tipo = tipo;
        FechaObjetivo = fechaObjetivo;
        Estado = EstadoSeguimientoClinico.Pendiente;
        Motivo = motivo;
        Notas = notas;
        EsGeneradoDesdeProximoControl = esGeneradoDesdeProximoControl;
        CreadoPorUsuarioId = creadoPorUsuarioId;
        FechaCreacion = fechaCreacion;
        FechaActualizacion = fechaCreacion;
    }

    public static SeguimientoClinico Crear(
        Guid veterinariaId,
        Guid mascotaVeterinariaId,
        Guid? consultaOrigenId,
        Guid? planTratamientoId,
        TipoSeguimientoClinico tipo,
        DateTimeOffset fechaObjetivo,
        string motivo,
        string? notas,
        Guid creadoPorUsuarioId,
        bool esGeneradoDesdeProximoControl = false,
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

        if (string.IsNullOrWhiteSpace(motivo))
        {
            throw new ExcepcionDominio("El motivo del seguimiento es obligatorio.");
        }

        if (creadoPorUsuarioId == Guid.Empty)
        {
            throw new ExcepcionDominio("El identificador de quien crea el seguimiento es obligatorio.");
        }

        return new SeguimientoClinico(
            Guid.NewGuid(),
            veterinariaId,
            mascotaVeterinariaId,
            consultaOrigenId,
            planTratamientoId,
            tipo,
            fechaObjetivo,
            motivo.Trim(),
            string.IsNullOrWhiteSpace(notas) ? null : notas.Trim(),
            esGeneradoDesdeProximoControl,
            creadoPorUsuarioId,
            fechaCreacion ?? DateTimeOffset.UtcNow);
    }

    /// <summary>
    /// La verificacion de que la Cita pertenece al mismo tenant y a la misma MascotaVeterinaria
    /// (seccion 37) se hace en Application antes de invocar esto -- el dominio solo protege la
    /// transicion de estado, igual que en el resto de agregados de esta plataforma.
    /// </summary>
    public void MarcarAgendado(Guid citaId, DateTimeOffset fechaAgendada, Guid actualizadoPorUsuarioId, DateTimeOffset momento)
    {
        if (citaId == Guid.Empty)
        {
            throw new ExcepcionDominio("El identificador de la cita es obligatorio.");
        }

        CambiarEstado(EstadoSeguimientoClinico.Agendado, actualizadoPorUsuarioId, momento);
        CitaId = citaId;
        FechaAgendada = fechaAgendada;
    }

    public void MarcarRealizado(Guid actualizadoPorUsuarioId, DateTimeOffset momento)
    {
        CambiarEstado(EstadoSeguimientoClinico.Realizado, actualizadoPorUsuarioId, momento);
        FechaRealizacion = momento;
    }

    public void MarcarVencido(DateTimeOffset momento) => CambiarEstado(EstadoSeguimientoClinico.Vencido, null, momento);

    public void Cancelar(Guid actualizadoPorUsuarioId, DateTimeOffset momento) =>
        CambiarEstado(EstadoSeguimientoClinico.Cancelado, actualizadoPorUsuarioId, momento);

    private void CambiarEstado(EstadoSeguimientoClinico nuevoEstado, Guid? actualizadoPorUsuarioId, DateTimeOffset momento)
    {
        if (!TransicionesPermitidas[Estado].Contains(nuevoEstado))
        {
            throw new ExcepcionDominio($"No se puede transicionar el seguimiento de '{Estado}' a '{nuevoEstado}'.");
        }

        Estado = nuevoEstado;
        ActualizadoPorUsuarioId = actualizadoPorUsuarioId ?? ActualizadoPorUsuarioId;
        FechaActualizacion = momento;
    }
}
