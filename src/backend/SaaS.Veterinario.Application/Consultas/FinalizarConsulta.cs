using SaaS.Veterinario.Application.Abstracciones;
using SaaS.Veterinario.Application.Errores;
using SaaS.Veterinario.Domain.Consultas;
using SaaS.Veterinario.Domain.Errores;

namespace SaaS.Veterinario.Application.Consultas;

/// <summary>
/// Al finalizar una Consulta vinculada a una Cita, se marca la Cita como ATENDIDA
/// automaticamente (seccion 14) dentro de la MISMA transaccion (seccion 42). ATENDIDA es
/// alcanzable desde PROGRAMADA, CONFIRMADA o EN_ESPERA (ajuste post-Etapa 7); si la cita esta
/// en un estado realmente terminal (CANCELADA, NO_ASISTIO, ya ATENDIDA), la transicion falla y
/// toda la operacion -- incluida la finalizacion de la consulta -- se revierte. IUnidadDeTrabajo
/// se usa unicamente como frontera transaccional, igual que en RegistrarMascota.
/// </summary>
public sealed class FinalizarConsulta(
    IContextoVeterinaria contexto,
    IRepositorioConsultas repositorioConsultas,
    IRepositorioCitas repositorioCitas,
    IUnidadDeTrabajo unidadDeTrabajo)
{
    public async Task EjecutarAsync(Guid consultaId, CancellationToken cancellationToken)
    {
        var veterinariaId = contexto.VeterinariaId
            ?? throw new InvalidOperationException("Se invocó FinalizarConsulta sin contexto veterinario resuelto.");

        var consulta = await repositorioConsultas.ObtenerPorIdAsync(consultaId, veterinariaId, cancellationToken)
            ?? throw new ExcepcionAplicacion(CodigosError.ConsultaNoEncontrada, "La consulta no existe en esta veterinaria.");

        if (consulta.Estado != EstadoConsulta.Borrador)
        {
            throw new ExcepcionAplicacion(CodigosError.ConsultaEstadoInvalido, "Solo se pueden finalizar consultas en borrador.");
        }

        await unidadDeTrabajo.EjecutarEnTransaccionAsync(async token =>
        {
            var usuarioId = contexto.UsuarioId;
            var momento = DateTimeOffset.UtcNow;

            consulta.Finalizar(usuarioId, momento);
            await repositorioConsultas.GuardarCambiosAsync(token);

            if (consulta.CitaId is { } citaId)
            {
                var cita = await repositorioCitas.ObtenerPorIdAsync(citaId, veterinariaId, token)
                    ?? throw new ExcepcionAplicacion(CodigosError.CitaNoEncontrada, "La cita vinculada ya no existe en esta veterinaria.");

                try
                {
                    cita.MarcarAtendida(usuarioId, momento);
                }
                catch (ExcepcionDominio excepcion)
                {
                    throw new ExcepcionAplicacion(CodigosError.CitaEstadoInvalido, excepcion.Message);
                }

                await repositorioCitas.GuardarCambiosAsync(token);
            }
        }, cancellationToken);
    }
}
