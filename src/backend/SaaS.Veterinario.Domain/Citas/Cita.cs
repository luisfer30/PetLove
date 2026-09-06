using SaaS.Veterinario.Domain.Errores;

namespace SaaS.Veterinario.Domain.Citas;

/// <summary>
/// Una Cita NO es una Consulta (R01): puede existir sin llegar nunca a una consulta (R02).
/// Pertenece obligatoriamente a una MascotaVeterinaria (R05); ClienteVeterinariaId y
/// VeterinarioUsuarioId son opcionales (una mascota puede tener cita sin cliente asociado, y
/// una cita puede quedar sin veterinario asignado todavia).
/// </summary>
public sealed class Cita
{
    // Ajuste post-Etapa 7: ATENDIDA es alcanzable desde PROGRAMADA, CONFIRMADA o EN_ESPERA --
    // en el flujo real de una veterinaria, no toda cita pasa explicitamente por EN_ESPERA antes
    // de ser atendida. Siguen prohibidas las transiciones desde un estado terminal (CANCELADA,
    // NO_ASISTIO, ATENDIDA) hacia ATENDIDA o cualquier otro estado.
    private static readonly Dictionary<EstadoCita, EstadoCita[]> TransicionesPermitidas = new()
    {
        [EstadoCita.Programada] = [EstadoCita.Confirmada, EstadoCita.EnEspera, EstadoCita.Atendida, EstadoCita.Cancelada, EstadoCita.NoAsistio],
        [EstadoCita.Confirmada] = [EstadoCita.EnEspera, EstadoCita.Atendida, EstadoCita.Cancelada, EstadoCita.NoAsistio],
        [EstadoCita.EnEspera] = [EstadoCita.Atendida, EstadoCita.Cancelada],
        [EstadoCita.Atendida] = [],
        [EstadoCita.Cancelada] = [],
        [EstadoCita.NoAsistio] = [],
    };

    public Guid Id { get; }
    public Guid VeterinariaId { get; }
    public Guid MascotaVeterinariaId { get; }
    public Guid? ClienteVeterinariaId { get; private set; }
    public Guid? VeterinarioUsuarioId { get; private set; }
    public DateTimeOffset FechaHoraInicio { get; private set; }
    public DateTimeOffset FechaHoraFin { get; private set; }
    public string Motivo { get; private set; }
    public string? Observaciones { get; private set; }
    public EstadoCita Estado { get; private set; }
    public DateTimeOffset FechaCreacion { get; }
    public Guid CreadoPorUsuarioId { get; }
    public DateTimeOffset FechaActualizacion { get; private set; }
    public Guid? ActualizadoPorUsuarioId { get; private set; }

    private Cita(
        Guid id,
        Guid veterinariaId,
        Guid mascotaVeterinariaId,
        Guid? clienteVeterinariaId,
        Guid? veterinarioUsuarioId,
        DateTimeOffset fechaHoraInicio,
        DateTimeOffset fechaHoraFin,
        string motivo,
        string? observaciones,
        Guid creadoPorUsuarioId,
        DateTimeOffset fechaCreacion)
    {
        Id = id;
        VeterinariaId = veterinariaId;
        MascotaVeterinariaId = mascotaVeterinariaId;
        ClienteVeterinariaId = clienteVeterinariaId;
        VeterinarioUsuarioId = veterinarioUsuarioId;
        FechaHoraInicio = fechaHoraInicio;
        FechaHoraFin = fechaHoraFin;
        Motivo = motivo;
        Observaciones = observaciones;
        Estado = EstadoCita.Programada;
        CreadoPorUsuarioId = creadoPorUsuarioId;
        FechaCreacion = fechaCreacion;
        FechaActualizacion = fechaCreacion;
    }

    public static Cita Crear(
        Guid veterinariaId,
        Guid mascotaVeterinariaId,
        Guid? clienteVeterinariaId,
        Guid? veterinarioUsuarioId,
        DateTimeOffset fechaHoraInicio,
        DateTimeOffset fechaHoraFin,
        string motivo,
        string? observaciones,
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

        if (string.IsNullOrWhiteSpace(motivo))
        {
            throw new ExcepcionDominio("El motivo de la cita es obligatorio.");
        }

        if (fechaHoraFin <= fechaHoraInicio)
        {
            throw new ExcepcionDominio("La fecha y hora de fin de la cita debe ser posterior a la de inicio.");
        }

        if (creadoPorUsuarioId == Guid.Empty)
        {
            throw new ExcepcionDominio("El identificador de quien crea la cita es obligatorio.");
        }

        return new Cita(
            Guid.NewGuid(),
            veterinariaId,
            mascotaVeterinariaId,
            clienteVeterinariaId,
            veterinarioUsuarioId,
            fechaHoraInicio,
            fechaHoraFin,
            motivo.Trim(),
            string.IsNullOrWhiteSpace(observaciones) ? null : observaciones.Trim(),
            creadoPorUsuarioId,
            fechaCreacion ?? DateTimeOffset.UtcNow);
    }

    /// <summary>
    /// Solo permitida mientras la cita esta en un estado editable (Programada/Confirmada) --
    /// la verificacion de "estado razonable" se hace en Application antes de invocar esto,
    /// pero se repite aqui como invariante de dominio para no depender solo del llamador.
    /// </summary>
    public void ActualizarDatos(
        DateTimeOffset fechaHoraInicio,
        DateTimeOffset fechaHoraFin,
        string motivo,
        string? observaciones,
        Guid? veterinarioUsuarioId,
        Guid actualizadoPorUsuarioId,
        DateTimeOffset momento)
    {
        if (Estado != EstadoCita.Programada && Estado != EstadoCita.Confirmada)
        {
            throw new ExcepcionDominio($"No se puede modificar una cita en estado '{Estado}'.");
        }

        if (string.IsNullOrWhiteSpace(motivo))
        {
            throw new ExcepcionDominio("El motivo de la cita es obligatorio.");
        }

        if (fechaHoraFin <= fechaHoraInicio)
        {
            throw new ExcepcionDominio("La fecha y hora de fin de la cita debe ser posterior a la de inicio.");
        }

        FechaHoraInicio = fechaHoraInicio;
        FechaHoraFin = fechaHoraFin;
        Motivo = motivo.Trim();
        Observaciones = string.IsNullOrWhiteSpace(observaciones) ? null : observaciones.Trim();
        VeterinarioUsuarioId = veterinarioUsuarioId;
        ActualizadoPorUsuarioId = actualizadoPorUsuarioId;
        FechaActualizacion = momento;
    }

    public void Confirmar(Guid actualizadoPorUsuarioId, DateTimeOffset momento) => CambiarEstado(EstadoCita.Confirmada, actualizadoPorUsuarioId, momento);

    public void MarcarEnEspera(Guid actualizadoPorUsuarioId, DateTimeOffset momento) => CambiarEstado(EstadoCita.EnEspera, actualizadoPorUsuarioId, momento);

    public void MarcarAtendida(Guid actualizadoPorUsuarioId, DateTimeOffset momento) => CambiarEstado(EstadoCita.Atendida, actualizadoPorUsuarioId, momento);

    public void Cancelar(Guid actualizadoPorUsuarioId, DateTimeOffset momento) => CambiarEstado(EstadoCita.Cancelada, actualizadoPorUsuarioId, momento);

    public void MarcarNoAsistio(Guid actualizadoPorUsuarioId, DateTimeOffset momento) => CambiarEstado(EstadoCita.NoAsistio, actualizadoPorUsuarioId, momento);

    private void CambiarEstado(EstadoCita nuevoEstado, Guid actualizadoPorUsuarioId, DateTimeOffset momento)
    {
        if (!TransicionesPermitidas[Estado].Contains(nuevoEstado))
        {
            throw new ExcepcionDominio($"No se puede transicionar la cita de '{Estado}' a '{nuevoEstado}'.");
        }

        Estado = nuevoEstado;
        ActualizadoPorUsuarioId = actualizadoPorUsuarioId;
        FechaActualizacion = momento;
    }
}
