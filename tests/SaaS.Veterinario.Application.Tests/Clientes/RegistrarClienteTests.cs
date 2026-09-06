using Moq;
using SaaS.Veterinario.Application.Abstracciones;
using SaaS.Veterinario.Application.Clientes;
using SaaS.Veterinario.Domain.Clientes;

namespace SaaS.Veterinario.Application.Tests.Clientes;

public class RegistrarClienteTests
{
    private readonly Mock<IContextoVeterinaria> _contexto = new();
    private readonly Mock<IRepositorioClientes> _repositorioClientes = new();

    private readonly Guid _veterinariaId = Guid.NewGuid();

    public RegistrarClienteTests()
    {
        _contexto.SetupGet(c => c.VeterinariaId).Returns(_veterinariaId);
    }

    private RegistrarCliente CrearCasoDeUso() => new(_contexto.Object, _repositorioClientes.Object);

    [Fact]
    public async Task EjecutarAsync_ConDatosValidos_CreaClienteParaLaVeterinariaDelContexto()
    {
        var casoDeUso = CrearCasoDeUso();
        var comando = new RegistrarClienteComando("Ana Perez", "ana@correo.com", "0999999999", null);

        var resultado = await casoDeUso.EjecutarAsync(comando, CancellationToken.None);

        Assert.NotEqual(Guid.Empty, resultado.ClienteId);
        _repositorioClientes.Verify(
            r => r.AgregarAsync(It.Is<ClienteVeterinaria>(c => c.VeterinariaId == _veterinariaId && c.Nombre == "Ana Perez"), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task EjecutarAsync_SinCorreo_CreaClienteConCorreoNulo()
    {
        var casoDeUso = CrearCasoDeUso();
        var comando = new RegistrarClienteComando("Ana Perez", null, null, null);

        await casoDeUso.EjecutarAsync(comando, CancellationToken.None);

        _repositorioClientes.Verify(
            r => r.AgregarAsync(It.Is<ClienteVeterinaria>(c => c.Correo == null), It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
