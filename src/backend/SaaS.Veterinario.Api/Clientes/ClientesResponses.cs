using SaaS.Veterinario.Domain.Clientes;

namespace SaaS.Veterinario.Api.Clientes;

public sealed record ClienteVeterinariaResponse(
    Guid Id,
    string Nombre,
    string? Correo,
    string? Telefono,
    string? Observaciones,
    string Estado)
{
    public static ClienteVeterinariaResponse DeDominio(ClienteVeterinaria cliente)
        => new(cliente.Id, cliente.Nombre, cliente.Correo?.Valor, cliente.Telefono, cliente.Observaciones, cliente.Estado.ToString());
}

public sealed record RegistrarClienteRequest(string Nombre, string? Correo, string? Telefono, string? Observaciones);

public sealed record RegistrarClienteResponse(Guid ClienteId);
