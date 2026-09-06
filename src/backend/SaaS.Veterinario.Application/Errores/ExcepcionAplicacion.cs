namespace SaaS.Veterinario.Application.Errores;

/// <summary>
/// Error funcional de un caso de uso (no una violacion de invariante del dominio).
/// Codigo es un identificador estable (p. ej. "AUTENTICACION_CREDENCIALES_INVALIDAS")
/// que la Api mapea a ProblemDetails; el dominio no conoce esta clase.
/// </summary>
public sealed class ExcepcionAplicacion : Exception
{
    public string Codigo { get; }

    public ExcepcionAplicacion(string codigo, string mensaje) : base(mensaje)
    {
        Codigo = codigo;
    }
}
