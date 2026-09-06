using Moq;
using SaaS.Veterinario.Application.Abstracciones;
using SaaS.Veterinario.Application.Tratamientos;
using SaaS.Veterinario.Domain.Tratamientos;

namespace SaaS.Veterinario.Application.Tests.Tratamientos;

public class MarcarProgramacionOmitidaTests
{
    private readonly Mock<IContextoVeterinaria> _contexto = new();
    private readonly Mock<IRepositorioProgramacionesTratamiento> _repositorioProgramaciones = new();

    private readonly Guid _veterinariaId = Guid.NewGuid();

    public MarcarProgramacionOmitidaTests()
    {
        _contexto.SetupGet(c => c.VeterinariaId).Returns(_veterinariaId);
        _contexto.SetupGet(c => c.UsuarioId).Returns(Guid.NewGuid());
    }

    [Fact]
    public async Task EjecutarAsync_ConProgramacionPendiente_LaMarcaOmitida()
    {
        var programacion = ProgramacionTratamiento.Crear(_veterinariaId, Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), DateTimeOffset.UtcNow);
        _repositorioProgramaciones.Setup(r => r.ObtenerPorIdAsync(programacion.Id, _veterinariaId, It.IsAny<CancellationToken>())).ReturnsAsync(programacion);

        var casoDeUso = new MarcarProgramacionOmitida(_contexto.Object, _repositorioProgramaciones.Object);

        await casoDeUso.EjecutarAsync(new MarcarProgramacionOmitidaComando(programacion.Id, "Mascota vomitó"), CancellationToken.None);

        Assert.Equal(EstadoProgramacionTratamiento.Omitida, programacion.Estado);
    }
}
