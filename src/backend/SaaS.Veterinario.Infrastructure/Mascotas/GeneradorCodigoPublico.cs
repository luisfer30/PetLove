using System.Security.Cryptography;
using System.Text;
using SaaS.Veterinario.Application.Abstracciones;

namespace SaaS.Veterinario.Infrastructure.Mascotas;

/// <summary>
/// Genera sufijos de 6 caracteres sobre un alfabeto sin caracteres ambiguos (sin O/0, I/1),
/// reintentando ante colision. El formato resultante ("PREFIJO-XXXXXX") cumple el patron
/// exigido por CodigoPublico.Crear.
/// </summary>
public sealed class GeneradorCodigoPublico : IGeneradorCodigoPublico
{
    private const string Alfabeto = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
    private const int LongitudSufijo = 6;
    private const int IntentosMaximos = 5;

    public async Task<string> GenerarUnicoAsync(
        string prefijo,
        Func<string, CancellationToken, Task<bool>> existeCodigoAsync,
        CancellationToken cancellationToken)
    {
        for (var intento = 0; intento < IntentosMaximos; intento++)
        {
            var candidato = $"{prefijo}-{GenerarSufijo()}";

            if (!await existeCodigoAsync(candidato, cancellationToken))
            {
                return candidato;
            }
        }

        throw new InvalidOperationException(
            $"No se pudo generar un código público único con el prefijo '{prefijo}' tras {IntentosMaximos} intentos.");
    }

    private static string GenerarSufijo()
    {
        var sufijo = new StringBuilder(LongitudSufijo);

        for (var i = 0; i < LongitudSufijo; i++)
        {
            sufijo.Append(Alfabeto[RandomNumberGenerator.GetInt32(Alfabeto.Length)]);
        }

        return sufijo.ToString();
    }
}
