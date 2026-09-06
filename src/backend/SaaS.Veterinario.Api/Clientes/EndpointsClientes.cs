using SaaS.Veterinario.Api.Autorizacion;
using SaaS.Veterinario.Application.Abstracciones;
using SaaS.Veterinario.Application.Clientes;

namespace SaaS.Veterinario.Api.Clientes;

public static class EndpointsClientes
{
    public static void MapearEndpointsClientes(this IEndpointRouteBuilder app)
    {
        var grupo = app.MapGroup("/api/v1/clientes").RequireAuthorization();

        grupo.MapGet("/", async (
                IContextoVeterinaria contexto,
                IRepositorioClientes repositorioClientes,
                CancellationToken cancellationToken) =>
            {
                var clientes = await repositorioClientes.ListarPorVeterinariaAsync(contexto.VeterinariaId!.Value, cancellationToken);
                return Results.Ok(clientes.Select(ClienteVeterinariaResponse.DeDominio));
            })
            .RequierePermiso("clientes.ver");

        grupo.MapGet("/{id:guid}", async (
                Guid id,
                IContextoVeterinaria contexto,
                IRepositorioClientes repositorioClientes,
                CancellationToken cancellationToken) =>
            {
                var cliente = await repositorioClientes.ObtenerPorIdAsync(id, contexto.VeterinariaId!.Value, cancellationToken);
                return cliente is null ? Results.NotFound() : Results.Ok(ClienteVeterinariaResponse.DeDominio(cliente));
            })
            .RequierePermiso("clientes.ver");

        grupo.MapPost("/", async (
                RegistrarClienteRequest solicitud,
                RegistrarCliente casoDeUso,
                CancellationToken cancellationToken) =>
            {
                var resultado = await casoDeUso.EjecutarAsync(
                    new RegistrarClienteComando(solicitud.Nombre, solicitud.Correo, solicitud.Telefono, solicitud.Observaciones),
                    cancellationToken);

                return Results.Ok(new RegistrarClienteResponse(resultado.ClienteId));
            })
            .RequierePermiso("clientes.crear");
    }
}
