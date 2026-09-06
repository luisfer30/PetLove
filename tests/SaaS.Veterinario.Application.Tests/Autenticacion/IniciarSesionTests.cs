using Moq;
using SaaS.Veterinario.Application.Abstracciones;
using SaaS.Veterinario.Application.Autenticacion;
using SaaS.Veterinario.Application.Errores;
using SaaS.Veterinario.Domain.Usuarios;

namespace SaaS.Veterinario.Application.Tests.Autenticacion;

public class IniciarSesionTests
{
    private readonly Mock<IServicioIdentidad> _servicioIdentidad = new();
    private readonly Mock<IRepositorioUsuarios> _repositorioUsuarios = new();
    private readonly Mock<IServicioTokens> _servicioTokens = new();

    private IniciarSesion CrearCasoDeUso()
        => new(_servicioIdentidad.Object, _repositorioUsuarios.Object, _servicioTokens.Object);

    private static readonly ParTokens TokensDePrueba = new("access", "refresh", DateTimeOffset.UtcNow, DateTimeOffset.UtcNow);

    [Fact]
    public async Task EjecutarAsync_ConCredencialesInvalidas_LanzaExcepcionAplicacion()
    {
        _servicioIdentidad
            .Setup(s => s.ValidarCredencialesAsync("ana@correo.com", "malo", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ResultadoInicioSesion(EstadoInicioSesion.CredencialesInvalidas, null));

        var casoDeUso = CrearCasoDeUso();
        var comando = new IniciarSesionComando("ana@correo.com", "malo", null, null);

        var excepcion = await Assert.ThrowsAsync<ExcepcionAplicacion>(() => casoDeUso.EjecutarAsync(comando, CancellationToken.None));

        Assert.Equal(CodigosError.CredencialesInvalidas, excepcion.Codigo);
    }

    [Fact]
    public async Task EjecutarAsync_ConUsuarioBloqueadoEnIdentidad_LanzaExcepcionAplicacion()
    {
        var usuario = Usuario.Crear("Ana Perez", "ana@correo.com");
        _servicioIdentidad
            .Setup(s => s.ValidarCredencialesAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ResultadoInicioSesion(EstadoInicioSesion.Bloqueado, usuario.Id));

        var casoDeUso = CrearCasoDeUso();
        var comando = new IniciarSesionComando("ana@correo.com", "password123", null, null);

        var excepcion = await Assert.ThrowsAsync<ExcepcionAplicacion>(() => casoDeUso.EjecutarAsync(comando, CancellationToken.None));

        Assert.Equal(CodigosError.UsuarioBloqueado, excepcion.Codigo);
    }

    [Fact]
    public async Task EjecutarAsync_ConUsuarioPendienteDeVerificacion_LanzaExcepcionDeCorreoNoVerificado()
    {
        var usuario = Usuario.Crear("Ana Perez", "ana@correo.com");

        _servicioIdentidad
            .Setup(s => s.ValidarCredencialesAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ResultadoInicioSesion(EstadoInicioSesion.Exitoso, usuario.Id));
        _repositorioUsuarios.Setup(r => r.ObtenerPorIdAsync(usuario.Id, It.IsAny<CancellationToken>())).ReturnsAsync(usuario);

        var casoDeUso = CrearCasoDeUso();
        var comando = new IniciarSesionComando("ana@correo.com", "password123", null, null);

        var excepcion = await Assert.ThrowsAsync<ExcepcionAplicacion>(() => casoDeUso.EjecutarAsync(comando, CancellationToken.None));

        Assert.Equal(CodigosError.CorreoNoVerificado, excepcion.Codigo);
    }

    [Fact]
    public async Task EjecutarAsync_ConUsuarioActivoYCredencialesValidas_GeneraTokens()
    {
        var usuario = Usuario.Crear("Ana Perez", "ana@correo.com");
        usuario.Activar(DateTimeOffset.UtcNow);

        _servicioIdentidad
            .Setup(s => s.ValidarCredencialesAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ResultadoInicioSesion(EstadoInicioSesion.Exitoso, usuario.Id));
        _repositorioUsuarios.Setup(r => r.ObtenerPorIdAsync(usuario.Id, It.IsAny<CancellationToken>())).ReturnsAsync(usuario);
        _servicioTokens
            .Setup(t => t.GenerarParAsync(usuario.Id, "ana@correo.com", null, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(TokensDePrueba);

        var casoDeUso = CrearCasoDeUso();
        var comando = new IniciarSesionComando("ana@correo.com", "password123", null, null);

        var resultado = await casoDeUso.EjecutarAsync(comando, CancellationToken.None);

        Assert.Equal(TokensDePrueba, resultado);
    }
}
