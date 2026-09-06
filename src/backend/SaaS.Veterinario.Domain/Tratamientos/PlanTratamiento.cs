using SaaS.Veterinario.Domain.Errores;

namespace SaaS.Veterinario.Domain.Tratamientos;

/// <summary>
/// Pertenece a una unica Veterinaria (R01) y una unica MascotaVeterinaria (R02); ConsultaId es
/// opcional (R03) -- un plan puede ser la continuacion de un tratamiento ya conocido sin
/// consulta asociada (seccion 30). Nace siempre PENDIENTE (seccion 4): activar es una accion
/// explicita, nunca automatica. La regla "si tiene Consulta asociada, esta debe estar
/// FINALIZADA para poder activar el Plan" vive en Application (seccion 29), porque el Plan no
/// conoce el estado actual, mutable, de la Consulta.
/// </summary>
public sealed class PlanTratamiento
{
    private static readonly Dictionary<EstadoPlanTratamiento, EstadoPlanTratamiento[]> TransicionesPermitidas = new()
    {
        [EstadoPlanTratamiento.Pendiente] = [EstadoPlanTratamiento.Activo, EstadoPlanTratamiento.Cancelado],
        [EstadoPlanTratamiento.Activo] = [EstadoPlanTratamiento.Suspendido, EstadoPlanTratamiento.Completado, EstadoPlanTratamiento.Cancelado],
        [EstadoPlanTratamiento.Suspendido] = [EstadoPlanTratamiento.Activo, EstadoPlanTratamiento.Completado, EstadoPlanTratamiento.Cancelado],
        [EstadoPlanTratamiento.Completado] = [],
        [EstadoPlanTratamiento.Cancelado] = [],
    };

    public Guid Id { get; }
    public Guid VeterinariaId { get; }
    public Guid MascotaVeterinariaId { get; }
    public Guid? ConsultaId { get; }
    public string Nombre { get; private set; }
    public string? Descripcion { get; private set; }
    public DateTimeOffset FechaInicio { get; private set; }
    public DateTimeOffset? FechaFinEstimada { get; private set; }
    public EstadoPlanTratamiento Estado { get; private set; }
    public Guid CreadoPorUsuarioId { get; }
    public DateTimeOffset FechaCreacion { get; }
    public Guid? ActualizadoPorUsuarioId { get; private set; }
    public DateTimeOffset FechaActualizacion { get; private set; }
    public Guid? SuspendidoPorUsuarioId { get; private set; }
    public DateTimeOffset? FechaSuspension { get; private set; }
    public string? MotivoSuspension { get; private set; }
    public Guid? CompletadoPorUsuarioId { get; private set; }
    public DateTimeOffset? FechaCompletado { get; private set; }
    public Guid? CanceladoPorUsuarioId { get; private set; }
    public DateTimeOffset? FechaCancelacion { get; private set; }
    public string? MotivoCancelacion { get; private set; }

    private PlanTratamiento(
        Guid id,
        Guid veterinariaId,
        Guid mascotaVeterinariaId,
        Guid? consultaId,
        string nombre,
        string? descripcion,
        DateTimeOffset fechaInicio,
        DateTimeOffset? fechaFinEstimada,
        Guid creadoPorUsuarioId,
        DateTimeOffset fechaCreacion)
    {
        Id = id;
        VeterinariaId = veterinariaId;
        MascotaVeterinariaId = mascotaVeterinariaId;
        ConsultaId = consultaId;
        Nombre = nombre;
        Descripcion = descripcion;
        FechaInicio = fechaInicio;
        FechaFinEstimada = fechaFinEstimada;
        Estado = EstadoPlanTratamiento.Pendiente;
        CreadoPorUsuarioId = creadoPorUsuarioId;
        FechaCreacion = fechaCreacion;
        FechaActualizacion = fechaCreacion;
    }

    public static PlanTratamiento Crear(
        Guid veterinariaId,
        Guid mascotaVeterinariaId,
        Guid? consultaId,
        string nombre,
        string? descripcion,
        DateTimeOffset fechaInicio,
        DateTimeOffset? fechaFinEstimada,
        Guid creadoPorUsuarioId,
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

        if (string.IsNullOrWhiteSpace(nombre))
        {
            throw new ExcepcionDominio("El nombre del plan de tratamiento es obligatorio.");
        }

        if (fechaFinEstimada is { } fin && fin <= fechaInicio)
        {
            throw new ExcepcionDominio("La fecha de fin estimada debe ser posterior a la fecha de inicio.");
        }

        if (creadoPorUsuarioId == Guid.Empty)
        {
            throw new ExcepcionDominio("El identificador de quien crea el plan es obligatorio.");
        }

        return new PlanTratamiento(
            Guid.NewGuid(),
            veterinariaId,
            mascotaVeterinariaId,
            consultaId,
            nombre.Trim(),
            string.IsNullOrWhiteSpace(descripcion) ? null : descripcion.Trim(),
            fechaInicio,
            fechaFinEstimada,
            creadoPorUsuarioId,
            fechaCreacion ?? DateTimeOffset.UtcNow);
    }

    /// <summary>Solo mientras PENDIENTE (seccion 31); ver ActualizarDatosActivo para el subconjunto permitido en ACTIVO.</summary>
    public void ActualizarDatosPendiente(
        string nombre, string? descripcion, DateTimeOffset fechaInicio, DateTimeOffset? fechaFinEstimada,
        Guid actualizadoPorUsuarioId, DateTimeOffset momento)
    {
        if (Estado != EstadoPlanTratamiento.Pendiente)
        {
            throw new ExcepcionDominio($"No se puede editar ampliamente un plan en estado '{Estado}'.");
        }

        if (string.IsNullOrWhiteSpace(nombre))
        {
            throw new ExcepcionDominio("El nombre del plan de tratamiento es obligatorio.");
        }

        if (fechaFinEstimada is { } fin && fin <= fechaInicio)
        {
            throw new ExcepcionDominio("La fecha de fin estimada debe ser posterior a la fecha de inicio.");
        }

        Nombre = nombre.Trim();
        Descripcion = string.IsNullOrWhiteSpace(descripcion) ? null : descripcion.Trim();
        FechaInicio = fechaInicio;
        FechaFinEstimada = fechaFinEstimada;
        ActualizadoPorUsuarioId = actualizadoPorUsuarioId;
        FechaActualizacion = momento;
    }

    /// <summary>Ajustes no estructurales permitidos mientras ACTIVO (seccion 32): solo descripcion/fecha fin estimada.</summary>
    public void ActualizarDatosActivo(string? descripcion, DateTimeOffset? fechaFinEstimada, Guid actualizadoPorUsuarioId, DateTimeOffset momento)
    {
        if (Estado != EstadoPlanTratamiento.Activo)
        {
            throw new ExcepcionDominio($"No se puede aplicar este ajuste a un plan en estado '{Estado}'.");
        }

        if (fechaFinEstimada is { } fin && fin <= FechaInicio)
        {
            throw new ExcepcionDominio("La fecha de fin estimada debe ser posterior a la fecha de inicio.");
        }

        Descripcion = string.IsNullOrWhiteSpace(descripcion) ? null : descripcion.Trim();
        FechaFinEstimada = fechaFinEstimada;
        ActualizadoPorUsuarioId = actualizadoPorUsuarioId;
        FechaActualizacion = momento;
    }

    public void Activar(Guid usuarioId, DateTimeOffset momento) => CambiarEstado(EstadoPlanTratamiento.Activo, usuarioId, momento);

    public void Reanudar(Guid usuarioId, DateTimeOffset momento) => CambiarEstado(EstadoPlanTratamiento.Activo, usuarioId, momento);

    public void Suspender(Guid usuarioId, string motivo, DateTimeOffset momento)
    {
        if (string.IsNullOrWhiteSpace(motivo))
        {
            throw new ExcepcionDominio("El motivo de suspensión es obligatorio.");
        }

        CambiarEstado(EstadoPlanTratamiento.Suspendido, usuarioId, momento);
        SuspendidoPorUsuarioId = usuarioId;
        FechaSuspension = momento;
        MotivoSuspension = motivo.Trim();
    }

    public void Completar(Guid usuarioId, DateTimeOffset momento)
    {
        CambiarEstado(EstadoPlanTratamiento.Completado, usuarioId, momento);
        CompletadoPorUsuarioId = usuarioId;
        FechaCompletado = momento;
    }

    public void Cancelar(Guid usuarioId, string motivo, DateTimeOffset momento)
    {
        if (string.IsNullOrWhiteSpace(motivo))
        {
            throw new ExcepcionDominio("El motivo de cancelación es obligatorio.");
        }

        CambiarEstado(EstadoPlanTratamiento.Cancelado, usuarioId, momento);
        CanceladoPorUsuarioId = usuarioId;
        FechaCancelacion = momento;
        MotivoCancelacion = motivo.Trim();
    }

    private void CambiarEstado(EstadoPlanTratamiento nuevoEstado, Guid actualizadoPorUsuarioId, DateTimeOffset momento)
    {
        if (!TransicionesPermitidas[Estado].Contains(nuevoEstado))
        {
            throw new ExcepcionDominio($"No se puede transicionar el plan de tratamiento de '{Estado}' a '{nuevoEstado}'.");
        }

        Estado = nuevoEstado;
        ActualizadoPorUsuarioId = actualizadoPorUsuarioId;
        FechaActualizacion = momento;
    }
}
