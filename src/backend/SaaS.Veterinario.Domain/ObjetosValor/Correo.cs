using System.Text.RegularExpressions;
using SaaS.Veterinario.Domain.Errores;

namespace SaaS.Veterinario.Domain.ObjetosValor;

public sealed record Correo
{
    private static readonly Regex FormatoValido =
        new(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled);

    public string Valor { get; }

    private Correo(string valor)
    {
        Valor = valor;
    }

    public static Correo Crear(string valor)
    {
        if (string.IsNullOrWhiteSpace(valor))
        {
            throw new ExcepcionDominio("El correo es obligatorio.");
        }

        var normalizado = valor.Trim().ToLowerInvariant();

        if (!FormatoValido.IsMatch(normalizado))
        {
            throw new ExcepcionDominio($"El correo '{valor}' no tiene un formato válido.");
        }

        return new Correo(normalizado);
    }

    public override string ToString() => Valor;
}
