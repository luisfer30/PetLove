using Moq;
using SaaS.Veterinario.Application.Abstracciones;
using SaaS.Veterinario.Application.Mascotas;
using SaaS.Veterinario.Domain.Mascotas;

namespace SaaS.Veterinario.Application.Tests.Mascotas;

public class BuscarMascotaTests
{
    private readonly Mock<IRepositorioMascotas> _repositorioMascotas = new();
    private readonly Mock<IRepositorioEspecies> _repositorioEspecies = new();

    private BuscarMascota CrearCasoDeUso() => new(_repositorioMascotas.Object, _repositorioEspecies.Object);

    private static Mascota MascotaConMicrochip() => Mascota.Crear(
        "PET-ABC123", "Firulais", Guid.NewGuid(), null, SexoMascota.Macho, null, false, null, "CHIP-1", null);

    [Fact]
    public async Task PorCodigoPublicoAsync_ConCodigoInexistente_RetornaNull()
    {
        _repositorioMascotas
            .Setup(r => r.ObtenerPorCodigoPublicoAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Mascota?)null);

        var resultado = await CrearCasoDeUso().PorCodigoPublicoAsync("PET-ZZZZZZ", CancellationToken.None);

        Assert.Null(resultado);
    }

    [Fact]
    public async Task PorCodigoPublicoAsync_ConMascotaExistente_NoIncluyeElMicrochipEnLaRespuesta()
    {
        var mascota = MascotaConMicrochip();
        _repositorioMascotas
            .Setup(r => r.ObtenerPorCodigoPublicoAsync(mascota.CodigoPublico.Valor, It.IsAny<CancellationToken>()))
            .ReturnsAsync(mascota);
        _repositorioEspecies
            .Setup(r => r.ObtenerPorIdAsync(mascota.EspecieId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Especie.Crear("PERRO", "Perro"));

        var resultado = await CrearCasoDeUso().PorCodigoPublicoAsync(mascota.CodigoPublico.Valor, CancellationToken.None);

        Assert.NotNull(resultado);
        Assert.Null(resultado!.NumeroMicrochip);
        Assert.Equal(mascota.CodigoPublico.Valor, resultado.CodigoPublico);
    }

    [Fact]
    public async Task PorMicrochipAsync_ConMascotaExistente_EcoaElMicrochipEnLaRespuesta()
    {
        var mascota = MascotaConMicrochip();
        _repositorioMascotas
            .Setup(r => r.ObtenerPorMicrochipAsync("CHIP-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(mascota);
        _repositorioEspecies
            .Setup(r => r.ObtenerPorIdAsync(mascota.EspecieId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Especie.Crear("PERRO", "Perro"));

        var resultado = await CrearCasoDeUso().PorMicrochipAsync("CHIP-1", CancellationToken.None);

        Assert.NotNull(resultado);
        Assert.Equal("CHIP-1", resultado!.NumeroMicrochip);
    }
}
