using SaaS.Veterinario.Domain.Citas;

namespace SaaS.Veterinario.Application.Abstracciones;

/// <summary>
/// Toda lectura por Id exige tambien VeterinariaId (R07): una cita de otra veterinaria nunca
/// debe distinguirse de una inexistente.
/// </summary>
public interface IRepositorioCitas
{
    Task<Cita?> ObtenerPorIdAsync(Guid id, Guid veterinariaId, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<Cita>> ListarAsync(
        Guid veterinariaId,
        DateTimeOffset? fechaDesde,
        DateTimeOffset? fechaHasta,
        Guid? veterinarioUsuarioId,
        EstadoCita? estado,
        CancellationToken cancellationToken);

    Task<IReadOnlyCollection<Cita>> ListarPorMascotaVeterinariaAsync(Guid mascotaVeterinariaId, Guid veterinariaId, CancellationToken cancellationToken);

    /// <summary>
    /// Deteccion basica de solapamiento (seccion 38): mismo veterinario, mismo tenant, cita no
    /// cancelada/no-asistida, e intervalo de horario superpuesto. excluirCitaId se usa al
    /// actualizar una cita existente para no compararla contra si misma.
    /// </summary>
    Task<bool> ExisteSolapamientoAsync(
        Guid veterinariaId,
        Guid veterinarioUsuarioId,
        DateTimeOffset fechaHoraInicio,
        DateTimeOffset fechaHoraFin,
        Guid? excluirCitaId,
        CancellationToken cancellationToken);

    Task AgregarAsync(Cita cita, CancellationToken cancellationToken);

    Task GuardarCambiosAsync(CancellationToken cancellationToken);
}
