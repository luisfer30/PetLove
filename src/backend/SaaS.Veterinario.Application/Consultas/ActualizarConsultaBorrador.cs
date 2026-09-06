using SaaS.Veterinario.Application.Abstracciones;
using SaaS.Veterinario.Application.Clinico;
using SaaS.Veterinario.Application.Errores;
using SaaS.Veterinario.Domain.Consultas;

namespace SaaS.Veterinario.Application.Consultas;

public sealed record ActualizarConsultaComando(
    Guid ConsultaId,
    Guid VeterinarioUsuarioId,
    DateTimeOffset FechaHora,
    string MotivoConsulta,
    decimal? Peso,
    decimal? Temperatura,
    int? FrecuenciaCardiaca,
    string? ObservacionesClinicas,
    string? IndicacionesPropietario,
    DateTimeOffset? ProximaFechaControl,
    string? MotivoProximoControl);

/// <summary>Solo mientras la Consulta este en BORRADOR (R08); ver seccion 11.</summary>
public sealed class ActualizarConsultaBorrador(
    IContextoVeterinaria contexto,
    IRepositorioConsultas repositorioConsultas,
    IRepositorioMembresias repositorioMembresias)
{
    public async Task EjecutarAsync(ActualizarConsultaComando comando, CancellationToken cancellationToken)
    {
        var veterinariaId = contexto.VeterinariaId
            ?? throw new InvalidOperationException("Se invocó ActualizarConsultaBorrador sin contexto veterinario resuelto.");

        var consulta = await repositorioConsultas.ObtenerPorIdAsync(comando.ConsultaId, veterinariaId, cancellationToken)
            ?? throw new ExcepcionAplicacion(CodigosError.ConsultaNoEncontrada, "La consulta no existe en esta veterinaria.");

        if (consulta.Estado != EstadoConsulta.Borrador)
        {
            throw new ExcepcionAplicacion(CodigosError.ConsultaFinalizadaNoEditable, "Solo se pueden editar consultas en borrador.");
        }

        await ValidadorVeterinarioClinico.ValidarAsync(
            repositorioMembresias, comando.VeterinarioUsuarioId, veterinariaId, CodigosError.ConsultaVeterinarioInvalido, cancellationToken);

        consulta.ActualizarBorrador(
            comando.VeterinarioUsuarioId,
            comando.FechaHora,
            comando.MotivoConsulta,
            comando.Peso,
            comando.Temperatura,
            comando.FrecuenciaCardiaca,
            comando.ObservacionesClinicas,
            comando.IndicacionesPropietario,
            comando.ProximaFechaControl,
            comando.MotivoProximoControl,
            contexto.UsuarioId,
            DateTimeOffset.UtcNow);

        await repositorioConsultas.GuardarCambiosAsync(cancellationToken);
    }
}
