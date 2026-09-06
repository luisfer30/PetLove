namespace SaaS.Veterinario.Domain.Mascotas;

/// <summary>
/// PENDIENTE queda preparado (campo del enum) para una futura etapa de invitacion del
/// responsable a vincularse con un Usuario global; en esta etapa solo se alcanzan
/// NoVinculado (siempre, al crear) y Vinculado (no hay caso de uso que lo dispare todavia).
/// </summary>
public enum EstadoVinculacionResponsable
{
    NoVinculado,
    Pendiente,
    Vinculado
}
