using SaaS.Veterinario.Application.Abstracciones;
using SaaS.Veterinario.Application.Errores;

namespace SaaS.Veterinario.Application.Personal;

public sealed record InvitarPersonalComando(string Correo, IReadOnlyCollection<string> CodigosRoles);

public sealed record InvitarPersonalResultado(Guid InvitacionId);

/// <summary>
/// VeterinariaId y "quien invita" salen exclusivamente de IContextoVeterinaria -- nunca del
/// cuerpo de la request. No activa ninguna membresia (eso corresponde al flujo de aceptacion,
/// fuera del alcance de esta etapa).
/// </summary>
public sealed class InvitarPersonal(
    IContextoVeterinaria contexto,
    IRepositorioRoles repositorioRoles,
    IServicioInvitaciones servicioInvitaciones,
    IServicioCorreo servicioCorreo)
{
    public async Task<InvitarPersonalResultado> EjecutarAsync(InvitarPersonalComando comando, CancellationToken cancellationToken)
    {
        var veterinariaId = contexto.VeterinariaId
            ?? throw new InvalidOperationException("Se invocó InvitarPersonal sin contexto veterinario resuelto.");

        var codigosDistintos = comando.CodigosRoles
            .Where(c => !string.IsNullOrWhiteSpace(c))
            .Select(c => c.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        if (codigosDistintos.Length == 0)
        {
            throw new ExcepcionAplicacion(CodigosError.PersonalRolInvalido, "Debe indicar al menos un rol válido.");
        }

        var roles = await repositorioRoles.ObtenerPorCodigosAsync(codigosDistintos, cancellationToken);

        if (roles.Count != codigosDistintos.Length)
        {
            throw new ExcepcionAplicacion(CodigosError.PersonalRolInvalido, "Uno o más roles indicados no existen.");
        }

        var resultado = await servicioInvitaciones.CrearAsync(
            veterinariaId,
            contexto.UsuarioId,
            comando.Correo,
            roles.Select(r => r.Id).ToArray(),
            cancellationToken);

        await servicioCorreo.EnviarInvitacionPersonalAsync(comando.Correo, resultado.TokenPlano, cancellationToken);

        return new InvitarPersonalResultado(resultado.InvitacionId);
    }
}
