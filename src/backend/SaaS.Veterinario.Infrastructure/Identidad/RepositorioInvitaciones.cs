using Microsoft.EntityFrameworkCore;
using SaaS.Veterinario.Application.Abstracciones;
using SaaS.Veterinario.Domain.ObjetosValor;
using SaaS.Veterinario.Domain.Personal;
using SaaS.Veterinario.Infrastructure.Persistencia;

namespace SaaS.Veterinario.Infrastructure.Identidad;

public sealed class RepositorioInvitaciones(SaaSVeterinarioDbContext contexto) : IRepositorioInvitaciones
{
    public Task<bool> ExisteInvitacionPendienteAsync(Guid veterinariaId, string correo, CancellationToken cancellationToken)
        => contexto.InvitacionesPersonal.AnyAsync(
            i => i.VeterinariaId == veterinariaId
                && i.Correo == Correo.Crear(correo)
                && i.Estado == EstadoInvitacionPersonal.Pendiente,
            cancellationToken);
}
