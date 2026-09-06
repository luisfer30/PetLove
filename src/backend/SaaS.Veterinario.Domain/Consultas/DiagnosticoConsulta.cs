using SaaS.Veterinario.Domain.Errores;

namespace SaaS.Veterinario.Domain.Consultas;

/// <summary>
/// Pertenece a una unica ConsultaVeterinaria; una consulta puede tener N diagnosticos, pero
/// como maximo uno con EsPrincipal = true (verificado en Application antes de crear/actualizar,
/// con un indice unico parcial en base de datos como red de seguridad -- mismo patron que el
/// responsable principal de la Etapa 6).
/// </summary>
public sealed class DiagnosticoConsulta
{
    public Guid Id { get; }
    public Guid ConsultaId { get; }
    public string Descripcion { get; private set; }
    public TipoDiagnostico Tipo { get; private set; }
    public bool EsPrincipal { get; private set; }
    public DateTimeOffset FechaCreacion { get; }

    private DiagnosticoConsulta(Guid id, Guid consultaId, string descripcion, TipoDiagnostico tipo, bool esPrincipal, DateTimeOffset fechaCreacion)
    {
        Id = id;
        ConsultaId = consultaId;
        Descripcion = descripcion;
        Tipo = tipo;
        EsPrincipal = esPrincipal;
        FechaCreacion = fechaCreacion;
    }

    public static DiagnosticoConsulta Crear(
        Guid consultaId,
        string descripcion,
        TipoDiagnostico tipo,
        bool esPrincipal,
        DateTimeOffset? fechaCreacion = null)
    {
        if (consultaId == Guid.Empty)
        {
            throw new ExcepcionDominio("El identificador de la consulta es obligatorio.");
        }

        if (string.IsNullOrWhiteSpace(descripcion))
        {
            throw new ExcepcionDominio("La descripción del diagnóstico es obligatoria.");
        }

        return new DiagnosticoConsulta(Guid.NewGuid(), consultaId, descripcion.Trim(), tipo, esPrincipal, fechaCreacion ?? DateTimeOffset.UtcNow);
    }

    public void Actualizar(string descripcion, TipoDiagnostico tipo, bool esPrincipal)
    {
        if (string.IsNullOrWhiteSpace(descripcion))
        {
            throw new ExcepcionDominio("La descripción del diagnóstico es obligatoria.");
        }

        Descripcion = descripcion.Trim();
        Tipo = tipo;
        EsPrincipal = esPrincipal;
    }
}
