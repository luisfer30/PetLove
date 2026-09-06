using SaaS.Veterinario.Application.Abstracciones;
using SaaS.Veterinario.Domain.Consultas;

namespace SaaS.Veterinario.Application.Consultas;

public sealed class ObtenerConsulta(IContextoVeterinaria contexto, IRepositorioConsultas repositorioConsultas)
{
    public Task<ConsultaVeterinaria?> EjecutarAsync(Guid consultaId, CancellationToken cancellationToken)
    {
        var veterinariaId = contexto.VeterinariaId
            ?? throw new InvalidOperationException("Se invocó ObtenerConsulta sin contexto veterinario resuelto.");

        return repositorioConsultas.ObtenerPorIdAsync(consultaId, veterinariaId, cancellationToken);
    }
}
