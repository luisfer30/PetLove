using SaaS.Veterinario.Application.Abstracciones;
using SaaS.Veterinario.Domain.Citas;

namespace SaaS.Veterinario.Application.Citas;

public sealed class ObtenerCita(IContextoVeterinaria contexto, IRepositorioCitas repositorioCitas)
{
    public Task<Cita?> EjecutarAsync(Guid citaId, CancellationToken cancellationToken)
    {
        var veterinariaId = contexto.VeterinariaId
            ?? throw new InvalidOperationException("Se invocó ObtenerCita sin contexto veterinario resuelto.");

        return repositorioCitas.ObtenerPorIdAsync(citaId, veterinariaId, cancellationToken);
    }
}
