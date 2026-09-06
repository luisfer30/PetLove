using Moq;
using SaaS.Veterinario.Application.Abstracciones;
using SaaS.Veterinario.Application.Autenticacion;

namespace SaaS.Veterinario.Application.Tests.Autenticacion;

public class SolicitarRecuperacionPasswordTests
{
    private readonly Mock<IServicioIdentidad> _servicioIdentidad = new();
    private readonly Mock<IServicioCorreo> _servicioCorreo = new();

    private SolicitarRecuperacionPassword CrearCasoDeUso() => new(_servicioIdentidad.Object, _servicioCorreo.Object);

    [Fact]
    public async Task EjecutarAsync_ConCorreoInexistente_NoEnviaCorreoYTerminaEnSilencio()
    {
        _servicioIdentidad
            .Setup(s => s.ObtenerUsuarioIdPorCorreoAsync("noexiste@correo.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync((Guid?)null);

        var casoDeUso = CrearCasoDeUso();

        await casoDeUso.EjecutarAsync(new SolicitarRecuperacionPasswordComando("noexiste@correo.com"), CancellationToken.None);

        _servicioCorreo.Verify(
            c => c.EnviarRecuperacionPasswordAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task EjecutarAsync_ConCorreoExistente_GeneraTokenYEnviaCorreo()
    {
        var usuarioId = Guid.NewGuid();
        _servicioIdentidad
            .Setup(s => s.ObtenerUsuarioIdPorCorreoAsync("ana@correo.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(usuarioId);
        _servicioIdentidad
            .Setup(s => s.GenerarTokenRecuperacionPasswordAsync(usuarioId, It.IsAny<CancellationToken>()))
            .ReturnsAsync("token-recuperacion");

        var casoDeUso = CrearCasoDeUso();

        await casoDeUso.EjecutarAsync(new SolicitarRecuperacionPasswordComando("ana@correo.com"), CancellationToken.None);

        _servicioCorreo.Verify(
            c => c.EnviarRecuperacionPasswordAsync("ana@correo.com", "token-recuperacion", It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
