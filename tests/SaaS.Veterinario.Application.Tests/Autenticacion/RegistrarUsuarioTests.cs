using Moq;
using SaaS.Veterinario.Application.Abstracciones;
using SaaS.Veterinario.Application.Autenticacion;
using SaaS.Veterinario.Application.Errores;
using SaaS.Veterinario.Domain.Usuarios;

namespace SaaS.Veterinario.Application.Tests.Autenticacion;

public class RegistrarUsuarioTests
{
    private readonly Mock<IRepositorioUsuarios> _repositorioUsuarios = new();
    private readonly Mock<IServicioIdentidad> _servicioIdentidad = new();
    private readonly Mock<IServicioCorreo> _servicioCorreo = new();
    private readonly Mock<IUnidadDeTrabajo> _unidadDeTrabajo = new();

    private RegistrarUsuario CrearCasoDeUso()
        => new(_repositorioUsuarios.Object, _servicioIdentidad.Object, _servicioCorreo.Object, _unidadDeTrabajo.Object);

    public RegistrarUsuarioTests()
    {
        // La unidad de trabajo de prueba ejecuta la operacion tal cual, sin transaccion real,
        // para poder verificar que el caso de uso realmente invoca ambos pasos en orden.
        _unidadDeTrabajo
            .Setup(u => u.EjecutarEnTransaccionAsync(It.IsAny<Func<CancellationToken, Task>>(), It.IsAny<CancellationToken>()))
            .Returns((Func<CancellationToken, Task> operacion, CancellationToken ct) => operacion(ct));

        _servicioIdentidad
            .Setup(s => s.GenerarTokenConfirmacionCorreoAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("token-confirmacion");
    }

    [Fact]
    public async Task EjecutarAsync_ConCorreoExistenteEnDominio_LanzaExcepcionAplicacionYNoTocaIdentidad()
    {
        _repositorioUsuarios.Setup(r => r.ExisteCorreoAsync("ana@correo.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var casoDeUso = CrearCasoDeUso();
        var comando = new RegistrarUsuarioComando("Ana Perez", "ana@correo.com", null, "password123");

        var excepcion = await Assert.ThrowsAsync<ExcepcionAplicacion>(() => casoDeUso.EjecutarAsync(comando, CancellationToken.None));

        Assert.Equal(CodigosError.CorreoExistente, excepcion.Codigo);
        _servicioIdentidad.Verify(s => s.CrearAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task EjecutarAsync_ConCorreoExistenteEnIdentidad_LanzaExcepcionAplicacion()
    {
        _repositorioUsuarios.Setup(r => r.ExisteCorreoAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);
        _servicioIdentidad.Setup(s => s.ExisteCorreoAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);

        var casoDeUso = CrearCasoDeUso();
        var comando = new RegistrarUsuarioComando("Ana Perez", "ana@correo.com", null, "password123");

        var excepcion = await Assert.ThrowsAsync<ExcepcionAplicacion>(() => casoDeUso.EjecutarAsync(comando, CancellationToken.None));

        Assert.Equal(CodigosError.CorreoExistente, excepcion.Codigo);
    }

    [Fact]
    public async Task EjecutarAsync_ConDatosValidos_CreaUsuarioEIdentidadYRetornaResultado()
    {
        _repositorioUsuarios.Setup(r => r.ExisteCorreoAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);
        _servicioIdentidad.Setup(s => s.ExisteCorreoAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);
        _servicioIdentidad
            .Setup(s => s.CrearAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ResultadoIdentidad.Ok());

        var casoDeUso = CrearCasoDeUso();
        var comando = new RegistrarUsuarioComando("Ana Perez", "ANA@correo.com", "0999999999", "password123");

        var resultado = await casoDeUso.EjecutarAsync(comando, CancellationToken.None);

        Assert.Equal("ana@correo.com", resultado.Correo);
        Assert.NotEqual(Guid.Empty, resultado.UsuarioId);

        _repositorioUsuarios.Verify(r => r.AgregarAsync(It.Is<Usuario>(u => u.Correo.Valor == "ana@correo.com"), It.IsAny<CancellationToken>()), Times.Once);
        _servicioIdentidad.Verify(s => s.CrearAsync(resultado.UsuarioId, "ana@correo.com", "password123", It.IsAny<CancellationToken>()), Times.Once);
        _servicioCorreo.Verify(
            c => c.EnviarConfirmacionCorreoAsync("ana@correo.com", "token-confirmacion", It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task EjecutarAsync_SiIdentidadFalla_LanzaExcepcionAplicacionDeRegistroInvalido()
    {
        _repositorioUsuarios.Setup(r => r.ExisteCorreoAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);
        _servicioIdentidad.Setup(s => s.ExisteCorreoAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);
        _servicioIdentidad
            .Setup(s => s.CrearAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ResultadoIdentidad.Fallo(["La contraseña es demasiado débil."]));

        var casoDeUso = CrearCasoDeUso();
        var comando = new RegistrarUsuarioComando("Ana Perez", "ana@correo.com", null, "123");

        var excepcion = await Assert.ThrowsAsync<ExcepcionAplicacion>(() => casoDeUso.EjecutarAsync(comando, CancellationToken.None));

        Assert.Equal(CodigosError.RegistroInvalido, excepcion.Codigo);
    }
}
