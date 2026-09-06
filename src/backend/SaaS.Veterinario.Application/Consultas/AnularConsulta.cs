using SaaS.Veterinario.Application.Abstracciones;
using SaaS.Veterinario.Application.Errores;
using SaaS.Veterinario.Domain.Errores;

namespace SaaS.Veterinario.Application.Consultas;

public sealed record AnularConsultaComando(Guid ConsultaId, string Motivo);

/// <summary>Nunca elimina la consulta fisicamente (seccion 13); permitida desde BORRADOR o FINALIZADA.</summary>
public sealed class AnularConsulta(IContextoVeterinaria contexto, IRepositorioConsultas repositorioConsultas)
{
    public async Task EjecutarAsync(AnularConsultaComando comando, CancellationToken cancellationToken)
    {
        var veterinariaId = contexto.VeterinariaId
            ?? throw new InvalidOperationException("Se invocó AnularConsulta sin contexto veterinario resuelto.");

        var consulta = await repositorioConsultas.ObtenerPorIdAsync(comando.ConsultaId, veterinariaId, cancellationToken)
            ?? throw new ExcepcionAplicacion(CodigosError.ConsultaNoEncontrada, "La consulta no existe en esta veterinaria.");

        try
        {
            consulta.Anular(contexto.UsuarioId, comando.Motivo, DateTimeOffset.UtcNow);
        }
        catch (ExcepcionDominio excepcion)
        {
            throw new ExcepcionAplicacion(CodigosError.ConsultaEstadoInvalido, excepcion.Message);
        }

        await repositorioConsultas.GuardarCambiosAsync(cancellationToken);
    }
}
