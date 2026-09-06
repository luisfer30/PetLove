using SaaS.Veterinario.Api.Autorizacion;
using SaaS.Veterinario.Application.Abstracciones;
using SaaS.Veterinario.Application.Personal;

namespace SaaS.Veterinario.Api.Personal;

public static class EndpointsPersonal
{
    public static void MapearEndpointsPersonal(this IEndpointRouteBuilder app)
    {
        var grupo = app.MapGroup("/api/v1/personal").RequireAuthorization();

        grupo.MapGet("/", async (
                IContextoVeterinaria contexto,
                IRepositorioMembresias repositorioMembresias,
                CancellationToken cancellationToken) =>
            {
                var personal = await repositorioMembresias.ObtenerPersonalActivoAsync(contexto.VeterinariaId!.Value, cancellationToken);
                return Results.Ok(personal.Select(MiembroPersonalResponse.DeInfo));
            })
            .RequierePermiso("personal.ver");

        grupo.MapPost("/invitaciones", async (
                InvitarPersonalRequest solicitud,
                InvitarPersonal casoDeUso,
                CancellationToken cancellationToken) =>
            {
                var resultado = await casoDeUso.EjecutarAsync(
                    new InvitarPersonalComando(solicitud.Correo, solicitud.Roles),
                    cancellationToken);

                return Results.Ok(new InvitarPersonalResponse(resultado.InvitacionId));
            })
            .RequierePermiso("personal.invitar");
    }
}
