using SaaS.Veterinario.Application.Abstracciones;
using SaaS.Veterinario.Infrastructure.Persistencia;

namespace SaaS.Veterinario.Infrastructure.Identidad;

public sealed class UnidadDeTrabajoEfCore(SaaSVeterinarioDbContext contexto) : IUnidadDeTrabajo
{
    public async Task EjecutarEnTransaccionAsync(Func<CancellationToken, Task> operacion, CancellationToken cancellationToken)
    {
        await using var transaccion = await contexto.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            await operacion(cancellationToken);
            await transaccion.CommitAsync(cancellationToken);
        }
        catch
        {
            await transaccion.RollbackAsync(cancellationToken);
            throw;
        }
    }
}
