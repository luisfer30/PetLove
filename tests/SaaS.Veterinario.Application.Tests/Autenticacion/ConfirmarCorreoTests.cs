using Moq;
using SaaS.Veterinario.Application.Abstracciones;
using SaaS.Veterinario.Application.Autenticacion;
using SaaS.Veterinario.Application.Errores;
using SaaS.Veterinario.Domain.Usuarios;

namespace SaaS.Veterinario.Application.Tests.Autenticacion;

public class ConfirmarCorreoTests
{
    private readonly Mock<IServicioIdentidad> _servicioIdentidad = new();
    private readonly Mock<IRepositorioUsuarios> _repositorioUsuarios = new();

    private ConfirmarCorreo CrearCasoDeUso() => new(_servicioIdentidad.Object, _repositorioUsuarios.Object);

    [Fact]
    public async Task EjecutarAsync_ConTokenInvalido_LanzaExcepcionAplicacionYNoTocaElUsuario()
    {
        _servicioIdentidad
            .Setup(s => s.ConfirmarCorreoAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ResultadoIdentidad.Fallo(["Token invalido."]));

        var casoDeUso = CrearCasoDeUso();
        var comando = new ConfirmarCorreoComando(Guid.NewGuid(), "token-malo");

        var excepcion = await Assert.ThrowsAsync<ExcepcionAplicacion>(() => casoDeUso.EjecutarAsync(comando, CancellationToken.None));

        Assert.Equal(CodigosError.TokenInvalido, excepcion.Codigo);
        _repositorioUsuarios.Verify(r => r.GuardarCambiosAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task EjecutarAsync_ConTokenValido_ConfirmaCorreoYActivaElUsuario()
    {
        var usuario = Usuario.Crear("Ana Perez", "ana@correo.com");

        _servicioIdentidad
            .Setup(s => s.ConfirmarCorreoAsync(usuario.Id, "token-bueno", It.IsAny<CancellationToken>()))
            .ReturnsAsync(ResultadoIdentidad.Ok());
        _repositorioUsuarios.Setup(r => r.ObtenerPorIdAsync(usuario.Id, It.IsAny<CancellationToken>())).ReturnsAsync(usuario);

        var casoDeUso = CrearCasoDeUso();
        var comando = new ConfirmarCorreoComando(usuario.Id, "token-bueno");

        await casoDeUso.EjecutarAsync(comando, CancellationToken.None);

        Assert.True(usuario.CorreoVerificado);
        Assert.Equal(EstadoUsuario.Activo, usuario.Estado);
        _repositorioUsuarios.Verify(r => r.GuardarCambiosAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
