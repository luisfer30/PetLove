using SaaS.Veterinario.Application.Abstracciones;
using SaaS.Veterinario.Application.Errores;
using SaaS.Veterinario.Domain.Consultas;

namespace SaaS.Veterinario.Application.Consultas;

public sealed class EliminarDiagnostico(
    IContextoVeterinaria contexto,
    IRepositorioConsultas repositorioConsultas,
    IRepositorioDiagnosticos repositorioDiagnosticos)
{
    public async Task EjecutarAsync(Guid consultaId, Guid diagnosticoId, CancellationToken cancellationToken)
    {
        var veterinariaId = contexto.VeterinariaId
            ?? throw new InvalidOperationException("Se invocó EliminarDiagnostico sin contexto veterinario resuelto.");

        var consulta = await repositorioConsultas.ObtenerPorIdAsync(consultaId, veterinariaId, cancellationToken)
            ?? throw new ExcepcionAplicacion(CodigosError.ConsultaNoEncontrada, "La consulta no existe en esta veterinaria.");

        if (consulta.Estado != EstadoConsulta.Borrador)
        {
            throw new ExcepcionAplicacion(CodigosError.DiagnosticoConsultaNoEditable, "Solo se pueden eliminar diagnósticos de una consulta en borrador.");
        }

        var diagnostico = await repositorioDiagnosticos.ObtenerPorIdAsync(diagnosticoId, consultaId, cancellationToken)
            ?? throw new ExcepcionAplicacion(CodigosError.DiagnosticoNoEncontrado, "El diagnóstico no existe en esta consulta.");

        await repositorioDiagnosticos.EliminarAsync(diagnostico, cancellationToken);
    }
}
