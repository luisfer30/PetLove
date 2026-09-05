using SaaS.Veterinario.Domain.Errores;

namespace SaaS.Veterinario.Domain.Permisos;

public sealed class Permiso
{
    public Guid Id { get; }
    public string Codigo { get; }
    public string Nombre { get; private set; }
    public string? Descripcion { get; private set; }

    private Permiso(Guid id, string codigo, string nombre, string? descripcion)
    {
        Id = id;
        Codigo = codigo;
        Nombre = nombre;
        Descripcion = descripcion;
    }

    public static Permiso Crear(string codigo, string nombre, string? descripcion = null)
    {
        if (string.IsNullOrWhiteSpace(codigo))
        {
            throw new ExcepcionDominio("El código del permiso es obligatorio.");
        }

        if (string.IsNullOrWhiteSpace(nombre))
        {
            throw new ExcepcionDominio("El nombre del permiso es obligatorio.");
        }

        var descripcionNormalizada = string.IsNullOrWhiteSpace(descripcion) ? null : descripcion.Trim();

        return new Permiso(Guid.NewGuid(), codigo.Trim().ToLowerInvariant(), nombre.Trim(), descripcionNormalizada);
    }
}
