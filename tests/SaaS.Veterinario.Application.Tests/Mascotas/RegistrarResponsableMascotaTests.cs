using Moq;
using SaaS.Veterinario.Application.Abstracciones;
using SaaS.Veterinario.Application.Errores;
using SaaS.Veterinario.Application.Mascotas;
using SaaS.Veterinario.Domain.Clientes;
using SaaS.Veterinario.Domain.Mascotas;

namespace SaaS.Veterinario.Application.Tests.Mascotas;

public class RegistrarResponsableMascotaTests
{
    private readonly Mock<IContextoVeterinaria> _contexto = new();
    private readonly Mock<IRepositorioMascotasVeterinarias> _repositorioMascotasVeterinarias = new();
    private readonly Mock<IRepositorioClientes> _repositorioClientes = new();
    private readonly Mock<IRepositorioResponsables> _repositorioResponsables = new();

    private readonly Guid _veterinariaId = Guid.NewGuid();

    public RegistrarResponsableMascotaTests()
    {
        _contexto.SetupGet(c => c.VeterinariaId).Returns(_veterinariaId);
        _repositorioResponsables
            .Setup(r => r.ExistePrincipalActivoAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
    }

    private RegistrarResponsableMascota CrearCasoDeUso() => new(
        _contexto.Object, _repositorioMascotasVeterinarias.Object, _repositorioClientes.Object, _repositorioResponsables.Object);

    private static MascotaVeterinaria FichaValida(Guid veterinariaId) => MascotaVeterinaria.Crear(veterinariaId, Guid.NewGuid());

    private RegistrarResponsableComando ComandoValido(Guid mascotaVeterinariaId) => new(
        mascotaVeterinariaId, null, "Ana Perez", null, null, "Propietario", true, true, true);

    [Fact]
    public async Task EjecutarAsync_ConFichaInexistenteEnLaVeterinaria_LanzaExcepcionAplicacion()
    {
        var mascotaVeterinariaId = Guid.NewGuid();
        _repositorioMascotasVeterinarias
            .Setup(r => r.ObtenerPorIdAsync(mascotaVeterinariaId, _veterinariaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((MascotaVeterinaria?)null);

        var casoDeUso = CrearCasoDeUso();

        var excepcion = await Assert.ThrowsAsync<ExcepcionAplicacion>(
            () => casoDeUso.EjecutarAsync(ComandoValido(mascotaVeterinariaId), CancellationToken.None));

        Assert.Equal(CodigosError.MascotaNoEncontrada, excepcion.Codigo);
    }

    [Fact]
    public async Task EjecutarAsync_ConClienteDeOtraVeterinaria_LanzaExcepcionAplicacion()
    {
        var ficha = FichaValida(_veterinariaId);
        _repositorioMascotasVeterinarias
            .Setup(r => r.ObtenerPorIdAsync(ficha.Id, _veterinariaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ficha);

        var clienteId = Guid.NewGuid();
        _repositorioClientes
            .Setup(r => r.ObtenerPorIdAsync(clienteId, _veterinariaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ClienteVeterinaria?)null);

        var casoDeUso = CrearCasoDeUso();
        var comando = ComandoValido(ficha.Id) with { ClienteVeterinariaId = clienteId };

        var excepcion = await Assert.ThrowsAsync<ExcepcionAplicacion>(() => casoDeUso.EjecutarAsync(comando, CancellationToken.None));

        Assert.Equal(CodigosError.ClienteInvalido, excepcion.Codigo);
    }

    [Fact]
    public async Task EjecutarAsync_ConTipoRelacionInvalido_LanzaExcepcionAplicacion()
    {
        var ficha = FichaValida(_veterinariaId);
        _repositorioMascotasVeterinarias
            .Setup(r => r.ObtenerPorIdAsync(ficha.Id, _veterinariaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ficha);

        var casoDeUso = CrearCasoDeUso();
        var comando = ComandoValido(ficha.Id) with { TipoRelacion = "NoExiste" };

        var excepcion = await Assert.ThrowsAsync<ExcepcionAplicacion>(() => casoDeUso.EjecutarAsync(comando, CancellationToken.None));

        Assert.Equal(CodigosError.ResponsableTipoRelacionInvalido, excepcion.Codigo);
    }

    [Fact]
    public async Task EjecutarAsync_ConPrincipalYaExistente_LanzaExcepcionAplicacionYNoCreaNada()
    {
        var ficha = FichaValida(_veterinariaId);
        _repositorioMascotasVeterinarias
            .Setup(r => r.ObtenerPorIdAsync(ficha.Id, _veterinariaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ficha);
        _repositorioResponsables
            .Setup(r => r.ExistePrincipalActivoAsync(ficha.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var casoDeUso = CrearCasoDeUso();
        var comando = ComandoValido(ficha.Id) with { EsPrincipal = true };

        var excepcion = await Assert.ThrowsAsync<ExcepcionAplicacion>(() => casoDeUso.EjecutarAsync(comando, CancellationToken.None));

        Assert.Equal(CodigosError.ResponsablePrincipalYaExiste, excepcion.Codigo);
        _repositorioResponsables.Verify(r => r.AgregarAsync(It.IsAny<ResponsableMascota>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task EjecutarAsync_ConDatosValidos_CreaResponsableSinUsuarioIdYNoVinculado()
    {
        var ficha = FichaValida(_veterinariaId);
        _repositorioMascotasVeterinarias
            .Setup(r => r.ObtenerPorIdAsync(ficha.Id, _veterinariaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ficha);

        var casoDeUso = CrearCasoDeUso();

        var resultado = await casoDeUso.EjecutarAsync(ComandoValido(ficha.Id), CancellationToken.None);

        Assert.NotEqual(Guid.Empty, resultado.ResponsableId);
        _repositorioResponsables.Verify(
            r => r.AgregarAsync(
                It.Is<ResponsableMascota>(rm =>
                    rm.VeterinariaId == _veterinariaId
                    && rm.MascotaVeterinariaId == ficha.Id
                    && rm.UsuarioId == null
                    && rm.EstadoVinculacion == EstadoVinculacionResponsable.NoVinculado),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
