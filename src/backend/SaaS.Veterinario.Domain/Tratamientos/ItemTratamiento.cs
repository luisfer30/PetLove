using SaaS.Veterinario.Domain.Errores;

namespace SaaS.Veterinario.Domain.Tratamientos;

/// <summary>
/// Un PlanTratamiento contiene N ItemsTratamiento (R08). La frecuencia determina que datos son
/// obligatorios (seccion 12): INTERVALO_HORAS exige IntervaloHoras; VECES_POR_DIA exige
/// VecesPorDia Y esa misma cantidad de Horarios explicitos (seccion 24 -- nunca se adivinan
/// horas de "3 veces al dia"); HORA_FIJA exige al menos un Horario; UNICA y SEGUN_NECESIDAD no
/// requieren ninguno de los dos (UNICA usa FechaInicio como el propio instante programado).
/// EsSegunNecesidad es una propiedad calculada, nunca un input independiente, para que jamas
/// quede en un estado inconsistente con FrecuenciaTipo.
/// </summary>
public sealed class ItemTratamiento
{
    private static readonly Dictionary<EstadoItemTratamiento, EstadoItemTratamiento[]> TransicionesPermitidas = new()
    {
        [EstadoItemTratamiento.Activo] = [EstadoItemTratamiento.Suspendido, EstadoItemTratamiento.Completado, EstadoItemTratamiento.Cancelado],
        [EstadoItemTratamiento.Suspendido] = [EstadoItemTratamiento.Activo, EstadoItemTratamiento.Completado, EstadoItemTratamiento.Cancelado],
        [EstadoItemTratamiento.Completado] = [],
        [EstadoItemTratamiento.Cancelado] = [],
    };

    private readonly List<HorarioItemTratamiento> _horarios = [];

    public Guid Id { get; }
    public Guid PlanTratamientoId { get; }
    public TipoItemTratamiento Tipo { get; }
    public string Nombre { get; private set; }
    public string? Descripcion { get; private set; }
    public decimal? DosisCantidad { get; private set; }
    public string? DosisUnidad { get; private set; }
    public ViaAdministracion? ViaAdministracion { get; private set; }
    public TipoFrecuenciaTratamiento FrecuenciaTipo { get; }
    public int? IntervaloHoras { get; }
    public int? VecesPorDia { get; }
    public DateTimeOffset FechaInicio { get; private set; }
    public DateTimeOffset? FechaFin { get; private set; }
    public int? DuracionDias { get; private set; }
    public string? Instrucciones { get; private set; }
    public bool EsSegunNecesidad => FrecuenciaTipo == TipoFrecuenciaTratamiento.SegunNecesidad;
    public EstadoItemTratamiento Estado { get; private set; }
    public int Orden { get; private set; }
    public DateTimeOffset FechaCreacion { get; }
    public DateTimeOffset FechaActualizacion { get; private set; }
    public IReadOnlyCollection<HorarioItemTratamiento> Horarios => _horarios.AsReadOnly();

    private ItemTratamiento(
        Guid id,
        Guid planTratamientoId,
        TipoItemTratamiento tipo,
        string nombre,
        string? descripcion,
        decimal? dosisCantidad,
        string? dosisUnidad,
        ViaAdministracion? viaAdministracion,
        TipoFrecuenciaTratamiento frecuenciaTipo,
        int? intervaloHoras,
        int? vecesPorDia,
        DateTimeOffset fechaInicio,
        DateTimeOffset? fechaFin,
        int? duracionDias,
        string? instrucciones,
        int orden,
        DateTimeOffset fechaCreacion)
    {
        Id = id;
        PlanTratamientoId = planTratamientoId;
        Tipo = tipo;
        Nombre = nombre;
        Descripcion = descripcion;
        DosisCantidad = dosisCantidad;
        DosisUnidad = dosisUnidad;
        ViaAdministracion = viaAdministracion;
        FrecuenciaTipo = frecuenciaTipo;
        IntervaloHoras = intervaloHoras;
        VecesPorDia = vecesPorDia;
        FechaInicio = fechaInicio;
        FechaFin = fechaFin;
        DuracionDias = duracionDias;
        Instrucciones = instrucciones;
        Estado = EstadoItemTratamiento.Activo;
        Orden = orden;
        FechaCreacion = fechaCreacion;
        FechaActualizacion = fechaCreacion;
    }

    public static ItemTratamiento Crear(
        Guid planTratamientoId,
        TipoItemTratamiento tipo,
        string nombre,
        string? descripcion,
        decimal? dosisCantidad,
        string? dosisUnidad,
        ViaAdministracion? viaAdministracion,
        TipoFrecuenciaTratamiento frecuenciaTipo,
        int? intervaloHoras,
        int? vecesPorDia,
        IReadOnlyCollection<TimeOnly> horarios,
        DateTimeOffset fechaInicio,
        DateTimeOffset? fechaFin,
        int? duracionDias,
        string? instrucciones,
        int orden,
        DateTimeOffset? fechaCreacion = null)
    {
        if (planTratamientoId == Guid.Empty)
        {
            throw new ExcepcionDominio("El identificador del plan de tratamiento es obligatorio.");
        }

        if (string.IsNullOrWhiteSpace(nombre))
        {
            throw new ExcepcionDominio("El nombre del item de tratamiento es obligatorio.");
        }

        if (dosisCantidad is <= 0)
        {
            throw new ExcepcionDominio("La cantidad de dosis debe ser mayor que cero.");
        }

        if (fechaFin is { } fin && fin <= fechaInicio)
        {
            throw new ExcepcionDominio("La fecha de fin debe ser posterior a la fecha de inicio.");
        }

        if (duracionDias is <= 0)
        {
            throw new ExcepcionDominio("La duración en días debe ser mayor que cero.");
        }

        ValidarFrecuencia(frecuenciaTipo, intervaloHoras, vecesPorDia, horarios);

        var fecha = fechaCreacion ?? DateTimeOffset.UtcNow;

        var item = new ItemTratamiento(
            Guid.NewGuid(),
            planTratamientoId,
            tipo,
            nombre.Trim(),
            NormalizarOpcional(descripcion),
            dosisCantidad,
            NormalizarOpcional(dosisUnidad),
            viaAdministracion,
            frecuenciaTipo,
            intervaloHoras,
            vecesPorDia,
            fechaInicio,
            fechaFin,
            duracionDias,
            NormalizarOpcional(instrucciones),
            orden,
            fecha);

        var ordenHorario = 0;
        foreach (var hora in horarios.Distinct().OrderBy(h => h))
        {
            item._horarios.Add(new HorarioItemTratamiento(item.Id, hora, ordenHorario++));
        }

        return item;
    }

    /// <summary>Cambios no estructurales (seccion 32): instrucciones/descripcion, nunca dosis/frecuencia/horarios.</summary>
    public void ActualizarInstrucciones(string? descripcion, string? instrucciones, DateTimeOffset momento)
    {
        if (Estado != EstadoItemTratamiento.Activo && Estado != EstadoItemTratamiento.Suspendido)
        {
            throw new ExcepcionDominio($"No se puede modificar un item en estado '{Estado}'.");
        }

        Descripcion = NormalizarOpcional(descripcion);
        Instrucciones = NormalizarOpcional(instrucciones);
        FechaActualizacion = momento;
    }

    public void Suspender(DateTimeOffset momento) => CambiarEstado(EstadoItemTratamiento.Suspendido, momento);

    public void Reanudar(DateTimeOffset momento) => CambiarEstado(EstadoItemTratamiento.Activo, momento);

    public void Completar(DateTimeOffset momento) => CambiarEstado(EstadoItemTratamiento.Completado, momento);

    public void Cancelar(DateTimeOffset momento) => CambiarEstado(EstadoItemTratamiento.Cancelado, momento);

    private void CambiarEstado(EstadoItemTratamiento nuevoEstado, DateTimeOffset momento)
    {
        if (!TransicionesPermitidas[Estado].Contains(nuevoEstado))
        {
            throw new ExcepcionDominio($"No se puede transicionar el item de tratamiento de '{Estado}' a '{nuevoEstado}'.");
        }

        Estado = nuevoEstado;
        FechaActualizacion = momento;
    }

    private static void ValidarFrecuencia(
        TipoFrecuenciaTratamiento frecuenciaTipo, int? intervaloHoras, int? vecesPorDia, IReadOnlyCollection<TimeOnly> horarios)
    {
        switch (frecuenciaTipo)
        {
            case TipoFrecuenciaTratamiento.IntervaloHoras:
                if (intervaloHoras is not > 0)
                {
                    throw new ExcepcionDominio("La frecuencia por intervalo de horas requiere un intervalo mayor que cero.");
                }

                break;

            case TipoFrecuenciaTratamiento.VecesPorDia:
                if (vecesPorDia is not > 0)
                {
                    throw new ExcepcionDominio("La frecuencia por veces al día requiere una cantidad mayor que cero.");
                }

                if (horarios.Count != vecesPorDia)
                {
                    throw new ExcepcionDominio("Debe indicar exactamente un horario por cada vez al día (no se infieren horarios automáticamente).");
                }

                break;

            case TipoFrecuenciaTratamiento.HoraFija:
                if (horarios.Count == 0)
                {
                    throw new ExcepcionDominio("La frecuencia por hora fija requiere al menos un horario.");
                }

                break;

            case TipoFrecuenciaTratamiento.Unica:
            case TipoFrecuenciaTratamiento.SegunNecesidad:
                if (horarios.Count > 0 || intervaloHoras is not null || vecesPorDia is not null)
                {
                    throw new ExcepcionDominio($"La frecuencia '{frecuenciaTipo}' no admite horarios, intervalo ni veces al día.");
                }

                break;
        }
    }

    private static string? NormalizarOpcional(string? valor) => string.IsNullOrWhiteSpace(valor) ? null : valor.Trim();
}
