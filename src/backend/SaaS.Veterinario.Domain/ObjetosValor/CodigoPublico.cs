using System.Text.RegularExpressions;
using SaaS.Veterinario.Domain.Errores;

namespace SaaS.Veterinario.Domain.ObjetosValor;

public sealed record CodigoPublico
{
    private static readonly Regex SufijoValido = new(@"^[A-Z0-9]{4,10}$", RegexOptions.Compiled);

    public string Valor { get; }

    private CodigoPublico(string valor)
    {
        Valor = valor;
    }

    public static CodigoPublico Crear(string valor, string prefijo)
    {
        if (string.IsNullOrWhiteSpace(prefijo))
        {
            throw new ArgumentException("El prefijo esperado del código público es obligatorio.", nameof(prefijo));
        }

        if (string.IsNullOrWhiteSpace(valor))
        {
            throw new ExcepcionDominio("El código público es obligatorio.");
        }

        var valorNormalizado = valor.Trim().ToUpperInvariant();
        var prefijoConGuion = $"{prefijo.Trim().ToUpperInvariant()}-";

        if (!valorNormalizado.StartsWith(prefijoConGuion, StringComparison.Ordinal))
        {
            throw new ExcepcionDominio($"El código público debe iniciar con '{prefijoConGuion}'.");
        }

        var sufijo = valorNormalizado[prefijoConGuion.Length..];

        if (!SufijoValido.IsMatch(sufijo))
        {
            throw new ExcepcionDominio($"El código público '{valor}' tiene un formato inválido.");
        }

        return new CodigoPublico(valorNormalizado);
    }

    public override string ToString() => Valor;
}
