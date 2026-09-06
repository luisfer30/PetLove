namespace SaaS.Veterinario.Api.Errores;

/// <summary>
/// Da a cada request un identificador rastreable: reutiliza el de X-Correlation-Id si el
/// cliente lo envio, si no genera uno. Se agrega a la respuesta y queda disponible para el
/// manejador de excepciones y para logging via HttpContext.Items.
/// </summary>
public sealed class CorrelationIdMiddleware(RequestDelegate siguiente, ILogger<CorrelationIdMiddleware> logger)
{
    public const string Encabezado = "X-Correlation-Id";

    public async Task InvokeAsync(HttpContext contexto)
    {
        var correlationId = contexto.Request.Headers.TryGetValue(Encabezado, out var valores) && !string.IsNullOrWhiteSpace(valores)
            ? valores.ToString()
            : Guid.NewGuid().ToString();

        contexto.Items[Encabezado] = correlationId;

        contexto.Response.OnStarting(() =>
        {
            contexto.Response.Headers[Encabezado] = correlationId;
            return Task.CompletedTask;
        });

        using (logger.BeginScope(new Dictionary<string, object> { ["CorrelationId"] = correlationId }))
        {
            await siguiente(contexto);
        }
    }
}

public static class ContextoCorrelacionExtensiones
{
    public static string ObtenerCorrelationId(this HttpContext contexto)
        => contexto.Items.TryGetValue(CorrelationIdMiddleware.Encabezado, out var valor) && valor is string texto
            ? texto
            : contexto.TraceIdentifier;
}
