using System.IdentityModel.Tokens.Jwt;

namespace SaaS.Veterinario.Api.Autenticacion;

public static class ClaimsPrincipalExtensiones
{
    /// <summary>
    /// Extrae el UsuarioId de dominio desde el claim "sub" del JWT ya validado. Se usa en
    /// endpoints [Authorize] en vez de confiar en cualquier identificador enviado por el
    /// cliente en el cuerpo de la request.
    /// </summary>
    public static Guid ObtenerUsuarioIdAutenticado(this HttpContext httpContext)
    {
        var valor = httpContext.User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
            ?? throw new InvalidOperationException("El token no contiene el claim 'sub'.");

        return Guid.Parse(valor);
    }
}
