using SaaS.Veterinario.Application.Abstracciones;
using SaaS.Veterinario.Application.Errores;
using SaaS.Veterinario.Domain.Consultas;

namespace SaaS.Veterinario.Application.Consultas;

public sealed record AgregarDiagnosticoComando(Guid ConsultaId, string Descripcion, string Tipo, bool EsPrincipal);

public sealed record AgregarDiagnosticoResultado(Guid DiagnosticoId);

/// <summary>Solo mientras la Consulta este en BORRADOR (seccion 22); maximo un diagnostico principal (seccion 21).</summary>
public sealed class AgregarDiagnostico(
    IContextoVeterinaria contexto,
    IRepositorioConsultas repositorioConsultas,
    IRepositorioDiagnosticos repositorioDiagnosticos)
{
    public async Task<AgregarDiagnosticoResultado> EjecutarAsync(AgregarDiagnosticoComando comando, CancellationToken cancellationToken)
    {
        var veterinariaId = contexto.VeterinariaId
            ?? throw new InvalidOperationException("Se invocó AgregarDiagnostico sin contexto veterinario resuelto.");

        var consulta = await repositorioConsultas.ObtenerPorIdAsync(comando.ConsultaId, veterinariaId, cancellationToken)
            ?? throw new ExcepcionAplicacion(CodigosError.ConsultaNoEncontrada, "La consulta no existe en esta veterinaria.");

        if (consulta.Estado != EstadoConsulta.Borrador)
        {
            throw new ExcepcionAplicacion(CodigosError.DiagnosticoConsultaNoEditable, "Solo se pueden agregar diagnósticos a una consulta en borrador.");
        }

        if (!Enum.TryParse<TipoDiagnostico>(comando.Tipo, ignoreCase: true, out var tipo))
        {
            throw new ExcepcionAplicacion(CodigosError.DiagnosticoTipoInvalido, "El tipo de diagnóstico indicado no es válido.");
        }

        if (comando.EsPrincipal && await repositorioDiagnosticos.ExistePrincipalAsync(comando.ConsultaId, excluirDiagnosticoId: null, cancellationToken))
        {
            throw new ExcepcionAplicacion(CodigosError.DiagnosticoPrincipalExistente, "Ya existe un diagnóstico principal para esta consulta.");
        }

        var diagnostico = DiagnosticoConsulta.Crear(comando.ConsultaId, comando.Descripcion, tipo, comando.EsPrincipal);

        await repositorioDiagnosticos.AgregarAsync(diagnostico, cancellationToken);

        return new AgregarDiagnosticoResultado(diagnostico.Id);
    }
}
