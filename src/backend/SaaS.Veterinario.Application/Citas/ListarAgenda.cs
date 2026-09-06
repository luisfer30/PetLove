using SaaS.Veterinario.Application.Abstracciones;
using SaaS.Veterinario.Domain.Citas;

namespace SaaS.Veterinario.Application.Citas;

public sealed record ListarAgendaConsulta(DateTimeOffset? FechaDesde, DateTimeOffset? FechaHasta, Guid? VeterinarioId, EstadoCita? Estado);

/// <summary>Siempre limitado a la veterinaria del contexto (seccion 6) -- nunca recibe VeterinariaId del llamador.</summary>
public sealed class ListarAgenda(IContextoVeterinaria contexto, IRepositorioCitas repositorioCitas)
{
    public Task<IReadOnlyCollection<Cita>> EjecutarAsync(ListarAgendaConsulta consulta, CancellationToken cancellationToken)
    {
        var veterinariaId = contexto.VeterinariaId
            ?? throw new InvalidOperationException("Se invocó ListarAgenda sin contexto veterinario resuelto.");

        return repositorioCitas.ListarAsync(veterinariaId, consulta.FechaDesde, consulta.FechaHasta, consulta.VeterinarioId, consulta.Estado, cancellationToken);
    }
}
