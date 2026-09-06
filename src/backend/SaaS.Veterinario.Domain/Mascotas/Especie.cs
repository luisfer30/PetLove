using SaaS.Veterinario.Domain.Errores;

namespace SaaS.Veterinario.Domain.Mascotas;

/// <summary>
/// Catalogo global (no pertenece a ninguna veterinaria). Mismo patron que Rol: sin
/// coleccion propia, Estado como simple activo/inactivo sin maquina de transiciones.
/// </summary>
public sealed class Especie
{
    public Guid Id { get; }
    public string Codigo { get; }
    public string Nombre { get; private set; }
    public EstadoEspecie Estado { get; private set; }

    private Especie(Guid id, string codigo, string nombre)
    {
        Id = id;
        Codigo = codigo;
        Nombre = nombre;
        Estado = EstadoEspecie.Activo;
    }

    public static Especie Crear(string codigo, string nombre)
    {
        if (string.IsNullOrWhiteSpace(codigo))
        {
            throw new ExcepcionDominio("El código de la especie es obligatorio.");
        }

        if (string.IsNullOrWhiteSpace(nombre))
        {
            throw new ExcepcionDominio("El nombre de la especie es obligatorio.");
        }

        return new Especie(Guid.NewGuid(), codigo.Trim().ToUpperInvariant(), nombre.Trim());
    }

    public void Activar() => Estado = EstadoEspecie.Activo;

    public void Desactivar() => Estado = EstadoEspecie.Inactivo;
}
