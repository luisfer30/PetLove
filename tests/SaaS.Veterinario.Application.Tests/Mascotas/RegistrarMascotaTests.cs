using Moq;
using SaaS.Veterinario.Application.Abstracciones;
using SaaS.Veterinario.Application.Errores;
using SaaS.Veterinario.Application.Mascotas;
using SaaS.Veterinario.Domain.Mascotas;

namespace SaaS.Veterinario.Application.Tests.Mascotas;

public class RegistrarMascotaTests
{
    private readonly Mock<IContextoVeterinaria> _contexto = new();
    private readonly Mock<IRepositorioMascotas> _repositorioMascotas = new();
    private readonly Mock<IRepositorioMascotasVeterinarias> _repositorioMascotasVeterinarias = new();
    private readonly Mock<IRepositorioEspecies> _repositorioEspecies = new();
    private readonly Mock<IGeneradorCodigoPublico> _generadorCodigoPublico = new();
    private readonly Mock<IUnidadDeTrabajo> _unidadDeTrabajo = new();

    private readonly Guid _veterinariaId = Guid.NewGuid();
    private readonly Guid _especieId = Guid.NewGuid();

    public RegistrarMascotaTests()
    {
        _contexto.SetupGet(c => c.VeterinariaId).Returns(_veterinariaId);

        _unidadDeTrabajo
            .Setup(u => u.EjecutarEnTransaccionAsync(It.IsAny<Func<CancellationToken, Task>>(), It.IsAny<CancellationToken>()))
            .Returns((Func<CancellationToken, Task> operacion, CancellationToken ct) => operacion(ct));

        _repositorioEspecies
            .Setup(r => r.ObtenerPorIdAsync(_especieId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Especie.Crear("PERRO", "Perro"));

        _repositorioMascotas
            .Setup(r => r.ExisteMicrochipAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _generadorCodigoPublico
            .Setup(g => g.GenerarUnicoAsync(Mascota.PrefijoCodigoPublico, It.IsAny<Func<string, CancellationToken, Task<bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("PET-ABC123");
    }

    private RegistrarMascota CrearCasoDeUso() => new(
        _contexto.Object,
        _repositorioMascotas.Object,
        _repositorioMascotasVeterinarias.Object,
        _repositorioEspecies.Object,
        _generadorCodigoPublico.Object,
        _unidadDeTrabajo.Object);

    private RegistrarMascotaComando ComandoValido() => new(
        "Firulais", _especieId, null, "Macho", null, false, null, null, null, null);

    [Fact]
    public async Task EjecutarAsync_ConDatosValidos_CreaMascotaYMascotaVeterinariaDentroDeUnaTransaccion()
    {
        var casoDeUso = CrearCasoDeUso();

        var resultado = await casoDeUso.EjecutarAsync(ComandoValido(), CancellationToken.None);

        Assert.Equal("PET-ABC123", resultado.CodigoPublico);
        _repositorioMascotas.Verify(r => r.AgregarAsync(It.IsAny<Mascota>(), It.IsAny<CancellationToken>()), Times.Once);
        _repositorioMascotasVeterinarias.Verify(
            r => r.AgregarAsync(It.Is<MascotaVeterinaria>(mv => mv.VeterinariaId == _veterinariaId), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task EjecutarAsync_ConSexoInvalido_LanzaExcepcionAplicacionYNoCreaNada()
    {
        var casoDeUso = CrearCasoDeUso();
        var comando = ComandoValido() with { Sexo = "NoExiste" };

        var excepcion = await Assert.ThrowsAsync<ExcepcionAplicacion>(() => casoDeUso.EjecutarAsync(comando, CancellationToken.None));

        Assert.Equal(CodigosError.MascotaSexoInvalido, excepcion.Codigo);
        _repositorioMascotas.Verify(r => r.AgregarAsync(It.IsAny<Mascota>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task EjecutarAsync_ConEspecieInexistente_LanzaExcepcionAplicacion()
    {
        _repositorioEspecies
            .Setup(r => r.ObtenerPorIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Especie?)null);

        var casoDeUso = CrearCasoDeUso();

        var excepcion = await Assert.ThrowsAsync<ExcepcionAplicacion>(() => casoDeUso.EjecutarAsync(ComandoValido(), CancellationToken.None));

        Assert.Equal(CodigosError.MascotaEspecieInvalida, excepcion.Codigo);
    }

    [Fact]
    public async Task EjecutarAsync_ConRazaDeOtraEspecie_LanzaExcepcionAplicacion()
    {
        var otraEspecieId = Guid.NewGuid();
        var raza = Raza.Crear(otraEspecieId, "Siames");

        _repositorioEspecies
            .Setup(r => r.ObtenerRazaPorIdAsync(raza.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(raza);

        var casoDeUso = CrearCasoDeUso();
        var comando = ComandoValido() with { RazaId = raza.Id };

        var excepcion = await Assert.ThrowsAsync<ExcepcionAplicacion>(() => casoDeUso.EjecutarAsync(comando, CancellationToken.None));

        Assert.Equal(CodigosError.MascotaRazaInvalida, excepcion.Codigo);
    }

    [Fact]
    public async Task EjecutarAsync_ConMicrochipDuplicado_LanzaExcepcionAplicacionYNoCreaNada()
    {
        _repositorioMascotas
            .Setup(r => r.ExisteMicrochipAsync("CHIP-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var casoDeUso = CrearCasoDeUso();
        var comando = ComandoValido() with { NumeroMicrochip = "CHIP-1" };

        var excepcion = await Assert.ThrowsAsync<ExcepcionAplicacion>(() => casoDeUso.EjecutarAsync(comando, CancellationToken.None));

        Assert.Equal(CodigosError.MascotaMicrochipDuplicado, excepcion.Codigo);
        _repositorioMascotas.Verify(r => r.AgregarAsync(It.IsAny<Mascota>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
