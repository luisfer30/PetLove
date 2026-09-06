using Moq;
using SaaS.Veterinario.Application.Abstracciones;
using SaaS.Veterinario.Application.Seguimientos;
using SaaS.Veterinario.Domain.Mascotas;
using SaaS.Veterinario.Domain.Seguimientos;

namespace SaaS.Veterinario.Application.Tests.Seguimientos;

public class CrearSeguimientoManualTests
{
    private readonly Mock<IContextoVeterinaria> _contexto = new();
    private readonly Mock<IRepositorioMascotasVeterinarias> _repositorioMascotasVeterinarias = new();
    private readonly Mock<IRepositorioSeguimientos> _repositorioSeguimientos = new();

    private readonly Guid _veterinariaId = Guid.NewGuid();
    private readonly Guid _mascotaVeterinariaId = Guid.NewGuid();

    public CrearSeguimientoManualTests()
    {
        _contexto.SetupGet(c => c.VeterinariaId).Returns(_veterinariaId);
        _contexto.SetupGet(c => c.UsuarioId).Returns(Guid.NewGuid());
        _repositorioMascotasVeterinarias
            .Setup(r => r.ObtenerPorIdAsync(_mascotaVeterinariaId, _veterinariaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(MascotaVeterinaria.Crear(_veterinariaId, Guid.NewGuid()));
    }

    [Fact]
    public async Task EjecutarAsync_ConDatosValidos_CreaElSeguimientoManual()
    {
        var casoDeUso = new CrearSeguimientoManual(_contexto.Object, _repositorioMascotasVeterinarias.Object, _repositorioSeguimientos.Object);
        var comando = new CrearSeguimientoManualComando(
            _mascotaVeterinariaId, null, null, "Control", DateTimeOffset.UtcNow.AddDays(30), "Revisión de rutina", null);

        var resultado = await casoDeUso.EjecutarAsync(comando, CancellationToken.None);

        Assert.NotEqual(Guid.Empty, resultado.SeguimientoId);
        _repositorioSeguimientos.Verify(
            r => r.AgregarAsync(It.Is<SeguimientoClinico>(s => !s.EsGeneradoDesdeProximoControl && s.MascotaVeterinariaId == _mascotaVeterinariaId), It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
