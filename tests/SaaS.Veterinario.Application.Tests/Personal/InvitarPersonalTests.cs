using Moq;
using SaaS.Veterinario.Application.Abstracciones;
using SaaS.Veterinario.Application.Errores;
using SaaS.Veterinario.Application.Personal;
using SaaS.Veterinario.Domain.Roles;

namespace SaaS.Veterinario.Application.Tests.Personal;

public class InvitarPersonalTests
{
    private readonly Mock<IContextoVeterinaria> _contexto = new();
    private readonly Mock<IRepositorioRoles> _repositorioRoles = new();
    private readonly Mock<IRepositorioInvitaciones> _repositorioInvitaciones = new();
    private readonly Mock<IServicioInvitaciones> _servicioInvitaciones = new();
    private readonly Mock<IServicioCorreo> _servicioCorreo = new();

    private readonly Guid _veterinariaId = Guid.NewGuid();

    public InvitarPersonalTests()
    {
        _contexto.SetupGet(c => c.VeterinariaId).Returns(_veterinariaId);
        _contexto.SetupGet(c => c.UsuarioId).Returns(Guid.NewGuid());
    }

    private InvitarPersonal CrearCasoDeUso() => new(
        _contexto.Object,
        _repositorioRoles.Object,
        _repositorioInvitaciones.Object,
        _servicioInvitaciones.Object,
        _servicioCorreo.Object);

    [Fact]
    public async Task EjecutarAsync_ConInvitacionPendienteExistente_LanzaExcepcionAplicacionYNoCreaNada()
    {
        _repositorioRoles
            .Setup(r => r.ObtenerPorCodigosAsync(It.IsAny<IReadOnlyCollection<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([Rol.Crear("VETERINARIO", "Veterinario")]);
        _repositorioInvitaciones
            .Setup(r => r.ExisteInvitacionPendienteAsync(_veterinariaId, "nueva@correo.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var casoDeUso = CrearCasoDeUso();
        var comando = new InvitarPersonalComando("nueva@correo.com", ["VETERINARIO"]);

        var excepcion = await Assert.ThrowsAsync<ExcepcionAplicacion>(() => casoDeUso.EjecutarAsync(comando, CancellationToken.None));

        Assert.Equal(CodigosError.PersonalInvitacionPendienteExistente, excepcion.Codigo);
        _servicioInvitaciones.Verify(
            s => s.CrearAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<IReadOnlyCollection<Guid>>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task EjecutarAsync_SinInvitacionPendienteExistente_CreaLaInvitacion()
    {
        var rol = Rol.Crear("VETERINARIO", "Veterinario");
        _repositorioRoles
            .Setup(r => r.ObtenerPorCodigosAsync(It.IsAny<IReadOnlyCollection<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([rol]);
        _repositorioInvitaciones
            .Setup(r => r.ExisteInvitacionPendienteAsync(_veterinariaId, "nueva@correo.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _servicioInvitaciones
            .Setup(s => s.CrearAsync(_veterinariaId, It.IsAny<Guid>(), "nueva@correo.com", It.IsAny<IReadOnlyCollection<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new InvitacionCreada(Guid.NewGuid(), "token-plano"));

        var casoDeUso = CrearCasoDeUso();
        var comando = new InvitarPersonalComando("nueva@correo.com", ["VETERINARIO"]);

        var resultado = await casoDeUso.EjecutarAsync(comando, CancellationToken.None);

        Assert.NotEqual(Guid.Empty, resultado.InvitacionId);
        _servicioCorreo.Verify(
            s => s.EnviarInvitacionPersonalAsync("nueva@correo.com", "token-plano", It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
