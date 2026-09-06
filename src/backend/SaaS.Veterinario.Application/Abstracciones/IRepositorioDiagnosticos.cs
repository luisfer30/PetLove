using SaaS.Veterinario.Domain.Consultas;

namespace SaaS.Veterinario.Application.Abstracciones;

/// <summary>
/// El aislamiento multi-tenant de un diagnostico se hereda de su ConsultaVeterinaria (siempre
/// se llega aqui despues de cargar y validar la consulta duena), por eso no repite VeterinariaId.
/// </summary>
public interface IRepositorioDiagnosticos
{
    Task<DiagnosticoConsulta?> ObtenerPorIdAsync(Guid id, Guid consultaId, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<DiagnosticoConsulta>> ListarPorConsultaAsync(Guid consultaId, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<DiagnosticoConsulta>> ListarPorConsultasAsync(IReadOnlyCollection<Guid> consultaIds, CancellationToken cancellationToken);

    /// <summary>Primera barrera de "maximo un diagnostico principal por consulta" (segunda: indice unico parcial en BD).</summary>
    Task<bool> ExistePrincipalAsync(Guid consultaId, Guid? excluirDiagnosticoId, CancellationToken cancellationToken);

    Task AgregarAsync(DiagnosticoConsulta diagnostico, CancellationToken cancellationToken);

    Task GuardarCambiosAsync(CancellationToken cancellationToken);

    Task EliminarAsync(DiagnosticoConsulta diagnostico, CancellationToken cancellationToken);
}
