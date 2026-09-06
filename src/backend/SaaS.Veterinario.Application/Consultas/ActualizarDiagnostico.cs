using SaaS.Veterinario.Application.Abstracciones;
using SaaS.Veterinario.Application.Errores;
using SaaS.Veterinario.Domain.Consultas;

namespace SaaS.Veterinario.Application.Consultas;

public sealed record ActualizarDiagnosticoComando(Guid ConsultaId, Guid DiagnosticoId, string Descripcion, string Tipo, bool EsPrincipal);

public sealed class ActualizarDiagnostico(
    IContextoVeterinaria contexto,
    IRepositorioConsultas repositorioConsultas,
    IRepositorioDiagnosticos repositorioDiagnosticos)
{
    public async Task EjecutarAsync(ActualizarDiagnosticoComando comando, CancellationToken cancellationToken)
    {
        var veterinariaId = contexto.VeterinariaId
            ?? throw new InvalidOperationException("Se invocó ActualizarDiagnostico sin contexto veterinario resuelto.");

        var consulta = await repositorioConsultas.ObtenerPorIdAsync(comando.ConsultaId, veterinariaId, cancellationToken)
            ?? throw new ExcepcionAplicacion(CodigosError.ConsultaNoEncontrada, "La consulta no existe en esta veterinaria.");

        if (consulta.Estado != EstadoConsulta.Borrador)
        {
            throw new ExcepcionAplicacion(CodigosError.DiagnosticoConsultaNoEditable, "Solo se pueden editar diagnósticos de una consulta en borrador.");
        }

        var diagnostico = await repositorioDiagnosticos.ObtenerPorIdAsync(comando.DiagnosticoId, comando.ConsultaId, cancellationToken)
            ?? throw new ExcepcionAplicacion(CodigosError.DiagnosticoNoEncontrado, "El diagnóstico no existe en esta consulta.");

        if (!Enum.TryParse<TipoDiagnostico>(comando.Tipo, ignoreCase: true, out var tipo))
        {
            throw new ExcepcionAplicacion(CodigosError.DiagnosticoTipoInvalido, "El tipo de diagnóstico indicado no es válido.");
        }

        if (comando.EsPrincipal
            && await repositorioDiagnosticos.ExistePrincipalAsync(comando.ConsultaId, excluirDiagnosticoId: comando.DiagnosticoId, cancellationToken))
        {
            throw new ExcepcionAplicacion(CodigosError.DiagnosticoPrincipalExistente, "Ya existe un diagnóstico principal para esta consulta.");
        }

        diagnostico.Actualizar(comando.Descripcion, tipo, comando.EsPrincipal);

        await repositorioDiagnosticos.GuardarCambiosAsync(cancellationToken);
    }
}
