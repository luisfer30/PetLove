namespace SaaS.Veterinario.Application.Abstracciones;

/// <summary>
/// Envuelve una operacion en una unica transaccion relacional. No es un Unit of Work
/// generico encima de EF Core (eso esta prohibido por la arquitectura): existe porque
/// el registro de un Usuario combina DOS escrituras independientes -- el alta del Usuario
/// de dominio (via IRepositorioUsuarios) y el alta de la identidad ASP.NET Identity (via
/// IServicioIdentidad, que hace su propio SaveChanges internamente) -- y ambas deben
/// confirmarse o revertirse juntas.
/// </summary>
public interface IUnidadDeTrabajo
{
    Task EjecutarEnTransaccionAsync(Func<CancellationToken, Task> operacion, CancellationToken cancellationToken);
}
