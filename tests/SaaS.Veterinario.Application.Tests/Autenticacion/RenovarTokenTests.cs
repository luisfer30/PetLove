using Moq;
using SaaS.Veterinario.Application.Abstracciones;
using SaaS.Veterinario.Application.Autenticacion;
using SaaS.Veterinario.Application.Errores;

namespace SaaS.Veterinario.Application.Tests.Autenticacion;

public class RenovarTokenTests
{
    private readonly Mock<IServicioTokens> _servicioTokens = new();

    private RenovarToken CrearCasoDeUso() => new(_servicioTokens.Object);

    [Fact]
    public async Task EjecutarAsync_Exitoso_RetornaLosTokensNuevos()
    {
        var tokens = new ParTokens("access", "refresh", DateTimeOffset.UtcNow, DateTimeOffset.UtcNow);
        _servicioTokens
            .Setup(t => t.RenovarAsync("plano", null, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ResultadoRenovacion(EstadoRenovacion.Exitoso, tokens));

        var resultado = await CrearCasoDeUso().EjecutarAsync(new RenovarTokenComando("plano", null, null), CancellationToken.None);

        Assert.Equal(tokens, resultado);
    }

    [Theory]
    [InlineData(EstadoRenovacion.Invalido, CodigosError.RefreshTokenInvalido)]
    [InlineData(EstadoRenovacion.Expirado, CodigosError.RefreshTokenExpirado)]
    [InlineData(EstadoRenovacion.Revocado, CodigosError.RefreshTokenRevocado)]
    public async Task EjecutarAsync_ConEstadoNoExitoso_LanzaExcepcionAplicacionConElCodigoCorrespondiente(
        EstadoRenovacion estado, string codigoEsperado)
    {
        _servicioTokens
            .Setup(t => t.RenovarAsync(It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ResultadoRenovacion(estado, null));

        var casoDeUso = CrearCasoDeUso();

        var excepcion = await Assert.ThrowsAsync<ExcepcionAplicacion>(
            () => casoDeUso.EjecutarAsync(new RenovarTokenComando("plano", null, null), CancellationToken.None));

        Assert.Equal(codigoEsperado, excepcion.Codigo);
    }
}
