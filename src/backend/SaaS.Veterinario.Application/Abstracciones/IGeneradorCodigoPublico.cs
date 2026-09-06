namespace SaaS.Veterinario.Application.Abstracciones;

/// <summary>
/// Servicio reutilizable para generar codigos publicos (PET-XXXXXX, y en el futuro otros
/// prefijos). Application no genera aleatoriedad por si misma -- igual que con RefreshToken
/// e InvitacionPersonal, la implementacion real vive en Infrastructure.
/// </summary>
public interface IGeneradorCodigoPublico
{
    /// <summary>
    /// Genera un codigo con el prefijo indicado y reintenta si existeCodigoAsync confirma
    /// colision, hasta un limite razonable de intentos.
    /// </summary>
    Task<string> GenerarUnicoAsync(
        string prefijo,
        Func<string, CancellationToken, Task<bool>> existeCodigoAsync,
        CancellationToken cancellationToken);
}
