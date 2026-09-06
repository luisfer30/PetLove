using Moq;
using SaaS.Veterinario.Application.Abstracciones;
using SaaS.Veterinario.Application.Errores;
using SaaS.Veterinario.Application.Mascotas;
using SaaS.Veterinario.Domain.Mascotas;

namespace SaaS.Veterinario.Application.Tests.Mascotas;

public class VincularMascotaExistenteTests
{
    private readonly Mock<IContextoVeterinaria> _contexto = new();
    private readonly Mock<IRepositorioMascotas> _repositorioMascotas = new();
    private readonly Mock<IRepositorioMascotasVeterinarias> _repositorioMascotasVeterinarias = new();

    private readonly Guid _veterinariaId = Guid.NewGuid();

    public VincularMascotaExistenteTests()
    {
        _contexto.SetupGet(c => c.VeterinariaId).Returns(_veterinariaId);
    }

    private VincularMascotaExistente CrearCasoDeUso() => new(
        _contexto.Object, _repositorioMascotas.Object, _repositorioMascotasVeterinarias.Object);

    private static Mascota MascotaGlobal() => Mascota.Crear("PET-ABC123", "Firulais", Guid.NewGuid(), null, SexoMascota.Macho, null, false, null, null, null);

    [Fact]
    public async Task EjecutarAsync_ConCodigoInexistente_LanzaExcepcionAplicacion()
    {
        _repositorioMascotas
            .Setup(r => r.ObtenerPorCodigoPublicoAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Mascota?)null);

        var casoDeUso = CrearCasoDeUso();
        var comando = new VincularMascotaExistenteComando("PET-ZZZZZZ", null);

        var excepcion = await Assert.ThrowsAsync<ExcepcionAplicacion>(() => casoDeUso.EjecutarAsync(comando, CancellationToken.None));

        Assert.Equal(CodigosError.MascotaNoEncontrada, excepcion.Codigo);
    }

    [Fact]
    public async Task EjecutarAsync_ConVinculoYaExistente_LanzaExcepcionAplicacionYNoCreaNada()
    {
        var mascota = MascotaGlobal();
        _repositorioMascotas
            .Setup(r => r.ObtenerPorCodigoPublicoAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(mascota);
        _repositorioMascotasVeterinarias
            .Setup(r => r.ExisteVinculoAsync(_veterinariaId, mascota.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var casoDeUso = CrearCasoDeUso();
        var comando = new VincularMascotaExistenteComando(mascota.CodigoPublico.Valor, null);

        var excepcion = await Assert.ThrowsAsync<ExcepcionAplicacion>(() => casoDeUso.EjecutarAsync(comando, CancellationToken.None));

        Assert.Equal(CodigosError.MascotaYaRegistradaEnVeterinaria, excepcion.Codigo);
        _repositorioMascotasVeterinarias.Verify(r => r.AgregarAsync(It.IsAny<MascotaVeterinaria>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task EjecutarAsync_ConMascotaSinVinculoPrevio_CreaMascotaVeterinariaParaLaVeterinariaDelContexto()
    {
        var mascota = MascotaGlobal();
        _repositorioMascotas
            .Setup(r => r.ObtenerPorCodigoPublicoAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(mascota);
        _repositorioMascotasVeterinarias
            .Setup(r => r.ExisteVinculoAsync(_veterinariaId, mascota.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var casoDeUso = CrearCasoDeUso();
        var comando = new VincularMascotaExistenteComando(mascota.CodigoPublico.Valor, "HC-001");

        var resultado = await casoDeUso.EjecutarAsync(comando, CancellationToken.None);

        Assert.Equal(mascota.CodigoPublico.Valor, resultado.CodigoPublico);
        _repositorioMascotasVeterinarias.Verify(
            r => r.AgregarAsync(It.Is<MascotaVeterinaria>(mv => mv.VeterinariaId == _veterinariaId && mv.MascotaId == mascota.Id), It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
