using System.Security.Cryptography;
using System.Text;
using SaaS.Veterinario.Application.Abstracciones;
using SaaS.Veterinario.Domain.Personal;
using SaaS.Veterinario.Infrastructure.Persistencia;

namespace SaaS.Veterinario.Infrastructure.Identidad;

/// <summary>
/// Igual que ServicioTokens con RefreshToken: genera el valor aleatorio criptografico,
/// persiste solo su hash SHA-256, y devuelve el valor en texto plano unicamente para que el
/// llamador (el caso de uso InvitarPersonal) se lo pase a IServicioCorreo.
/// </summary>
public sealed class ServicioInvitaciones(SaaSVeterinarioDbContext contexto) : IServicioInvitaciones
{
    private const int DiasExpiracion = 7;

    public async Task<InvitacionCreada> CrearAsync(
        Guid veterinariaId,
        Guid creadoPorUsuarioId,
        string correo,
        IReadOnlyCollection<Guid> rolIds,
        CancellationToken cancellationToken)
    {
        var ahora = DateTimeOffset.UtcNow;
        var tokenPlano = GenerarValorAleatorioSeguro();
        var tokenHash = Hashear(tokenPlano);

        var invitacion = InvitacionPersonal.Crear(
            veterinariaId,
            correo,
            tokenHash,
            creadoPorUsuarioId,
            ahora.AddDays(DiasExpiracion),
            ahora);

        foreach (var rolId in rolIds)
        {
            invitacion.AsignarRol(rolId);
        }

        contexto.Add(invitacion);
        await contexto.SaveChangesAsync(cancellationToken);

        return new InvitacionCreada(invitacion.Id, tokenPlano);
    }

    private static string GenerarValorAleatorioSeguro()
    {
        var bytes = RandomNumberGenerator.GetBytes(64);
        return Convert.ToBase64String(bytes);
    }

    private static string Hashear(string valor)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(valor));
        return Convert.ToHexString(bytes);
    }
}
