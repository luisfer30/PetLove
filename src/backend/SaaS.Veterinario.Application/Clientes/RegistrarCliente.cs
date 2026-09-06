using SaaS.Veterinario.Application.Abstracciones;
using SaaS.Veterinario.Domain.Clientes;

namespace SaaS.Veterinario.Application.Clientes;

public sealed record RegistrarClienteComando(string Nombre, string? Correo, string? Telefono, string? Observaciones);

public sealed record RegistrarClienteResultado(Guid ClienteId);

/// <summary>
/// No devuelve ninguna señal sobre si el correo coincide con un Usuario global existente
/// (decision explicita: la vinculacion cliente-usuario y cualquier senal relacionada quedan
/// diferidas a la etapa que implemente ese mecanismo con la verificacion adecuada).
/// </summary>
public sealed class RegistrarCliente(IContextoVeterinaria contexto, IRepositorioClientes repositorioClientes)
{
    public async Task<RegistrarClienteResultado> EjecutarAsync(RegistrarClienteComando comando, CancellationToken cancellationToken)
    {
        var veterinariaId = contexto.VeterinariaId
            ?? throw new InvalidOperationException("Se invocó RegistrarCliente sin contexto veterinario resuelto.");

        var cliente = ClienteVeterinaria.Crear(veterinariaId, comando.Nombre, comando.Correo, comando.Telefono, comando.Observaciones);

        await repositorioClientes.AgregarAsync(cliente, cancellationToken);

        return new RegistrarClienteResultado(cliente.Id);
    }
}
