using SaaS.Veterinario.Domain.Errores;

namespace SaaS.Veterinario.Domain.Consultas;

/// <summary>
/// Toda Consulta pertenece obligatoriamente a una MascotaVeterinaria (R04) y puede existir sin
/// Cita previa (R03, ej. emergencia/atencion espontanea) -- CitaId es opcional. A diferencia de
/// Cita, el veterinario aqui SI es obligatorio (seccion 15): una consulta siempre requiere quien
/// la realizo. ObservacionesClinicas (interno) y IndicacionesPropietario (visible al cliente)
/// son campos deliberadamente distintos (R09) -- nunca se combinan ni se derivan uno del otro.
/// </summary>
public sealed class ConsultaVeterinaria
{
    private static readonly Dictionary<EstadoConsulta, EstadoConsulta[]> TransicionesPermitidas = new()
    {
        [EstadoConsulta.Borrador] = [EstadoConsulta.Finalizada, EstadoConsulta.Anulada],
        [EstadoConsulta.Finalizada] = [EstadoConsulta.Anulada],
        [EstadoConsulta.Anulada] = [],
    };

    public Guid Id { get; }
    public Guid VeterinariaId { get; }
    public Guid MascotaVeterinariaId { get; }
    public Guid? CitaId { get; }
    public Guid VeterinarioUsuarioId { get; private set; }
    public DateTimeOffset FechaHora { get; private set; }
    public string MotivoConsulta { get; private set; }
    public decimal? Peso { get; private set; }
    public decimal? Temperatura { get; private set; }
    public int? FrecuenciaCardiaca { get; private set; }
    public string? ObservacionesClinicas { get; private set; }
    public string? IndicacionesPropietario { get; private set; }
    public DateTimeOffset? ProximaFechaControl { get; private set; }
    public string? MotivoProximoControl { get; private set; }
    public EstadoConsulta Estado { get; private set; }
    public DateTimeOffset FechaCreacion { get; }
    public Guid CreadoPorUsuarioId { get; }
    public DateTimeOffset FechaActualizacion { get; private set; }
    public Guid? ActualizadoPorUsuarioId { get; private set; }
    public DateTimeOffset? FechaFinalizacion { get; private set; }
    public Guid? FinalizadoPorUsuarioId { get; private set; }
    public DateTimeOffset? FechaAnulacion { get; private set; }
    public Guid? AnuladoPorUsuarioId { get; private set; }
    public string? MotivoAnulacion { get; private set; }

    private ConsultaVeterinaria(
        Guid id,
        Guid veterinariaId,
        Guid mascotaVeterinariaId,
        Guid? citaId,
        Guid veterinarioUsuarioId,
        DateTimeOffset fechaHora,
        string motivoConsulta,
        decimal? peso,
        decimal? temperatura,
        int? frecuenciaCardiaca,
        string? observacionesClinicas,
        string? indicacionesPropietario,
        DateTimeOffset? proximaFechaControl,
        string? motivoProximoControl,
        Guid creadoPorUsuarioId,
        DateTimeOffset fechaCreacion)
    {
        Id = id;
        VeterinariaId = veterinariaId;
        MascotaVeterinariaId = mascotaVeterinariaId;
        CitaId = citaId;
        VeterinarioUsuarioId = veterinarioUsuarioId;
        FechaHora = fechaHora;
        MotivoConsulta = motivoConsulta;
        Peso = peso;
        Temperatura = temperatura;
        FrecuenciaCardiaca = frecuenciaCardiaca;
        ObservacionesClinicas = observacionesClinicas;
        IndicacionesPropietario = indicacionesPropietario;
        ProximaFechaControl = proximaFechaControl;
        MotivoProximoControl = motivoProximoControl;
        Estado = EstadoConsulta.Borrador;
        CreadoPorUsuarioId = creadoPorUsuarioId;
        FechaCreacion = fechaCreacion;
        FechaActualizacion = fechaCreacion;
    }

    public static ConsultaVeterinaria Crear(
        Guid veterinariaId,
        Guid mascotaVeterinariaId,
        Guid? citaId,
        Guid veterinarioUsuarioId,
        DateTimeOffset fechaHora,
        string motivoConsulta,
        decimal? peso,
        decimal? temperatura,
        int? frecuenciaCardiaca,
        string? observacionesClinicas,
        string? indicacionesPropietario,
        DateTimeOffset? proximaFechaControl,
        string? motivoProximoControl,
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

        if (veterinarioUsuarioId == Guid.Empty)
        {
            throw new ExcepcionDominio("El veterinario que realiza la consulta es obligatorio.");
        }

        if (string.IsNullOrWhiteSpace(motivoConsulta))
        {
            throw new ExcepcionDominio("El motivo de la consulta es obligatorio.");
        }

        ValidarSignosVitales(peso, temperatura, frecuenciaCardiaca);
        ValidarProximoControl(proximaFechaControl, fechaHora);

        if (creadoPorUsuarioId == Guid.Empty)
        {
            throw new ExcepcionDominio("El identificador de quien crea la consulta es obligatorio.");
        }

        return new ConsultaVeterinaria(
            Guid.NewGuid(),
            veterinariaId,
            mascotaVeterinariaId,
            citaId,
            veterinarioUsuarioId,
            fechaHora,
            motivoConsulta.Trim(),
            peso,
            temperatura,
            frecuenciaCardiaca,
            NormalizarOpcional(observacionesClinicas),
            NormalizarOpcional(indicacionesPropietario),
            proximaFechaControl,
            NormalizarOpcional(motivoProximoControl),
            creadoPorUsuarioId,
            fechaCreacion ?? DateTimeOffset.UtcNow);
    }

    /// <summary>Solo mientras Estado = Borrador (R08); vuelve a comprobarlo aqui ademas del pre-chequeo en Application.</summary>
    public void ActualizarBorrador(
        Guid veterinarioUsuarioId,
        DateTimeOffset fechaHora,
        string motivoConsulta,
        decimal? peso,
        decimal? temperatura,
        int? frecuenciaCardiaca,
        string? observacionesClinicas,
        string? indicacionesPropietario,
        DateTimeOffset? proximaFechaControl,
        string? motivoProximoControl,
        Guid actualizadoPorUsuarioId,
        DateTimeOffset momento)
    {
        if (Estado != EstadoConsulta.Borrador)
        {
            throw new ExcepcionDominio($"No se puede modificar una consulta en estado '{Estado}'.");
        }

        if (veterinarioUsuarioId == Guid.Empty)
        {
            throw new ExcepcionDominio("El veterinario que realiza la consulta es obligatorio.");
        }

        if (string.IsNullOrWhiteSpace(motivoConsulta))
        {
            throw new ExcepcionDominio("El motivo de la consulta es obligatorio.");
        }

        ValidarSignosVitales(peso, temperatura, frecuenciaCardiaca);
        ValidarProximoControl(proximaFechaControl, fechaHora);

        VeterinarioUsuarioId = veterinarioUsuarioId;
        FechaHora = fechaHora;
        MotivoConsulta = motivoConsulta.Trim();
        Peso = peso;
        Temperatura = temperatura;
        FrecuenciaCardiaca = frecuenciaCardiaca;
        ObservacionesClinicas = NormalizarOpcional(observacionesClinicas);
        IndicacionesPropietario = NormalizarOpcional(indicacionesPropietario);
        ProximaFechaControl = proximaFechaControl;
        MotivoProximoControl = NormalizarOpcional(motivoProximoControl);
        ActualizadoPorUsuarioId = actualizadoPorUsuarioId;
        FechaActualizacion = momento;
    }

    public void Finalizar(Guid usuarioId, DateTimeOffset momento)
    {
        if (!TransicionesPermitidas[Estado].Contains(EstadoConsulta.Finalizada))
        {
            throw new ExcepcionDominio($"No se puede finalizar una consulta en estado '{Estado}'.");
        }

        Estado = EstadoConsulta.Finalizada;
        FechaFinalizacion = momento;
        FinalizadoPorUsuarioId = usuarioId;
        ActualizadoPorUsuarioId = usuarioId;
        FechaActualizacion = momento;
    }

    public void Anular(Guid usuarioId, string motivo, DateTimeOffset momento)
    {
        if (!TransicionesPermitidas[Estado].Contains(EstadoConsulta.Anulada))
        {
            throw new ExcepcionDominio($"No se puede anular una consulta en estado '{Estado}'.");
        }

        if (string.IsNullOrWhiteSpace(motivo))
        {
            throw new ExcepcionDominio("El motivo de anulación es obligatorio.");
        }

        Estado = EstadoConsulta.Anulada;
        FechaAnulacion = momento;
        AnuladoPorUsuarioId = usuarioId;
        MotivoAnulacion = motivo.Trim();
        ActualizadoPorUsuarioId = usuarioId;
        FechaActualizacion = momento;
    }

    private static void ValidarSignosVitales(decimal? peso, decimal? temperatura, int? frecuenciaCardiaca)
    {
        if (peso is <= 0)
        {
            throw new ExcepcionDominio("El peso debe ser mayor que cero.");
        }

        if (temperatura is <= 0)
        {
            throw new ExcepcionDominio("La temperatura debe ser mayor que cero.");
        }

        if (frecuenciaCardiaca is <= 0)
        {
            throw new ExcepcionDominio("La frecuencia cardíaca debe ser mayor que cero.");
        }
    }

    private static void ValidarProximoControl(DateTimeOffset? proximaFechaControl, DateTimeOffset fechaHora)
    {
        if (proximaFechaControl is { } fecha && fecha <= fechaHora)
        {
            throw new ExcepcionDominio("La fecha del próximo control debe ser posterior a la fecha de la consulta.");
        }
    }

    private static string? NormalizarOpcional(string? valor) => string.IsNullOrWhiteSpace(valor) ? null : valor.Trim();
}
