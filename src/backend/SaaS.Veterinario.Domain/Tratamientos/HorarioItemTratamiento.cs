using SaaS.Veterinario.Domain.Errores;

namespace SaaS.Veterinario.Domain.Tratamientos;

/// <summary>
/// Estructura relacional para representar horarios multiples de un ItemTratamiento (seccion
/// 13) -- reemplaza deliberadamente un campo unico "HoraEspecifica" que no podria representar
/// mas de un horario por dia. Es un hijo manipulado exclusivamente a traves de ItemTratamiento
/// (mismo patron que InvitacionPersonalRol/MembresiaRol), nunca creado de forma independiente.
/// </summary>
public sealed class HorarioItemTratamiento
{
    public Guid Id { get; }
    public Guid ItemTratamientoId { get; }
    public TimeOnly Hora { get; }
    public int Orden { get; }

    internal HorarioItemTratamiento(Guid itemTratamientoId, TimeOnly hora, int orden)
    {
        if (itemTratamientoId == Guid.Empty)
        {
            throw new ExcepcionDominio("El identificador del item de tratamiento es obligatorio.");
        }

        Id = Guid.NewGuid();
        ItemTratamientoId = itemTratamientoId;
        Hora = hora;
        Orden = orden;
    }
}
