using SaaS.Veterinario.Application.Abstracciones;
using SaaS.Veterinario.Application.Errores;
using SaaS.Veterinario.Domain.Consultas;

namespace SaaS.Veterinario.Application.Consultas;

/// <summary>Valida tenant a traves de la Consulta duena antes de listar (evita revelar diagnosticos de otra veterinaria).</summary>
public sealed class ListarDiagnosticos(
    IContextoVeterinaria contexto,
    IRepositorioConsultas repositorioConsultas,
    IRepositorioDiagnosticos repositorioDiagnosticos)
{
    public async Task<IReadOnlyCollection<DiagnosticoConsulta>> EjecutarAsync(Guid consultaId, CancellationToken cancellationToken)
    {
        var veterinariaId = contexto.VeterinariaId
            ?? throw new InvalidOperationException("Se invocó ListarDiagnosticos sin contexto veterinario resuelto.");

        _ = await repositorioConsultas.ObtenerPorIdAsync(consultaId, veterinariaId, cancellationToken)
            ?? throw new ExcepcionAplicacion(CodigosError.ConsultaNoEncontrada, "La consulta no existe en esta veterinaria.");

        return await repositorioDiagnosticos.ListarPorConsultaAsync(consultaId, cancellationToken);
    }
}
