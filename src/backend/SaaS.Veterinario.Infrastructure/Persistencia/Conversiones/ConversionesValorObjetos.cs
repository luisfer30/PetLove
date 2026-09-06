using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using SaaS.Veterinario.Domain.ObjetosValor;

namespace SaaS.Veterinario.Infrastructure.Persistencia.Conversiones;

/// <summary>
/// Conversiones EF Core para los Value Objects del dominio. Los VOs son inmutables,
/// por lo que el snapshot del ValueComparer puede devolver la misma instancia (identidad)
/// en vez de reconstruir el valor.
/// </summary>
internal static class ConversionesValorObjetos
{
    public static ValueConverter<Correo, string> ConversorCorreo { get; } = new(
        correo => correo.Valor,
        valor => Correo.Crear(valor));

    public static ValueComparer<Correo> ComparadorCorreo { get; } = new(
        (a, b) => a!.Valor == b!.Valor,
        c => c.Valor.GetHashCode(),
        c => c);

    public static ValueComparer<CodigoPublico> ComparadorCodigoPublico { get; } = new(
        (a, b) => a!.Valor == b!.Valor,
        c => c.Valor.GetHashCode(),
        c => c);

    public static ValueConverter<CodigoPublico, string> CrearConversorCodigoPublico(string prefijo) => new(
        codigo => codigo.Valor,
        valor => CodigoPublico.Crear(valor, prefijo));
}
