using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace SaaS.Veterinario.Infrastructure.Persistencia.Conversiones;

/// <summary>
/// Reduce el boilerplate de ConversionesEstados/ConversionesEstadosMascotas: a partir de un
/// solo diccionario enum-a-texto arma el diccionario inverso y el ValueConverter. Mismo
/// motivo de siempre para no usar switch/throw: los lambdas de ValueConverter son arboles de
/// expresion.
/// </summary>
internal static class FabricaConversorEnum
{
    public static ValueConverter<TEnum, string> Crear<TEnum>(IReadOnlyDictionary<TEnum, string> mapa)
        where TEnum : struct, Enum
    {
        var inverso = mapa.ToDictionary(par => par.Value, par => par.Key);
        return new ValueConverter<TEnum, string>(valor => mapa[valor], texto => inverso[texto]);
    }
}
