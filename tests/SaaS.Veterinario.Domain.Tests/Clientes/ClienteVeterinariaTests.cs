using SaaS.Veterinario.Domain.Errores;
using SaaS.Veterinario.Domain.Clientes;

namespace SaaS.Veterinario.Domain.Tests.Clientes;

public class ClienteVeterinariaTests
{
    [Fact]
    public void Crear_ConDatosValidos_QuedaActivoSinUsuario()
    {
        var cliente = ClienteVeterinaria.Crear(Guid.NewGuid(), "Ana Perez", "ANA@Correo.com", "0999999999", null);

        Assert.Equal(EstadoClienteVeterinaria.Activo, cliente.Estado);
        Assert.Equal("ana@correo.com", cliente.Correo!.Valor);
        Assert.Null(cliente.UsuarioId);
    }

    [Fact]
    public void Crear_SinCorreoNiTelefono_EsValido()
    {
        var cliente = ClienteVeterinaria.Crear(Guid.NewGuid(), "Ana Perez", null, null, null);

        Assert.Null(cliente.Correo);
        Assert.Null(cliente.Telefono);
    }

    [Fact]
    public void Crear_SinNombre_LanzaExcepcionDominio()
    {
        Assert.Throws<ExcepcionDominio>(() => ClienteVeterinaria.Crear(Guid.NewGuid(), "   ", null, null, null));
    }

    [Fact]
    public void Crear_ConVeterinariaIdVacio_LanzaExcepcionDominio()
    {
        Assert.Throws<ExcepcionDominio>(() => ClienteVeterinaria.Crear(Guid.Empty, "Ana Perez", null, null, null));
    }

    [Fact]
    public void Desactivar_CambiaEstadoAInactivo()
    {
        var cliente = ClienteVeterinaria.Crear(Guid.NewGuid(), "Ana Perez", null, null, null);

        cliente.Desactivar();

        Assert.Equal(EstadoClienteVeterinaria.Inactivo, cliente.Estado);
    }
}
