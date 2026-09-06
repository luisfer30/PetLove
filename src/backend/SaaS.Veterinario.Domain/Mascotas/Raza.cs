using SaaS.Veterinario.Domain.Errores;

namespace SaaS.Veterinario.Domain.Mascotas;

/// <summary>
/// La unicidad de EspecieId+Nombre se protege en base de datos (indice UNIQUE), igual que
/// el resto de unicidades del dominio: una entidad individual no puede verificar por si sola
/// la existencia de otras filas.
/// </summary>
public sealed class Raza
{
    public Guid Id { get; }
    public Guid EspecieId { get; }
    public string Nombre { get; private set; }
    public EstadoRaza Estado { get; private set; }

    private Raza(Guid id, Guid especieId, string nombre)
    {
        Id = id;
        EspecieId = especieId;
        Nombre = nombre;
        Estado = EstadoRaza.Activo;
    }

    public static Raza Crear(Guid especieId, string nombre)
    {
        if (especieId == Guid.Empty)
        {
            throw new ExcepcionDominio("La especie de la raza es obligatoria.");
        }

        if (string.IsNullOrWhiteSpace(nombre))
        {
            throw new ExcepcionDominio("El nombre de la raza es obligatorio.");
        }

        return new Raza(Guid.NewGuid(), especieId, nombre.Trim());
    }

    public void Activar() => Estado = EstadoRaza.Activo;

    public void Desactivar() => Estado = EstadoRaza.Inactivo;
}
