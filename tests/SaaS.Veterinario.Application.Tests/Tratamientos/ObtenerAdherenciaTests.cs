using Moq;
using SaaS.Veterinario.Application.Abstracciones;
using SaaS.Veterinario.Application.Errores;
using SaaS.Veterinario.Application.Tratamientos;
using SaaS.Veterinario.Domain.Tratamientos;

namespace SaaS.Veterinario.Application.Tests.Tratamientos;

public class ObtenerAdherenciaTests
{
    private readonly Mock<IContextoVeterinaria> _contexto = new();
    private readonly Mock<IRepositorioPlanesTratamiento> _repositorioPlanes = new();
    private readonly Mock<IRepositorioProgramacionesTratamiento> _repositorioProgramaciones = new();

    private readonly Guid _veterinariaId = Guid.NewGuid();
    private readonly Guid _planId = Guid.NewGuid();

    public ObtenerAdherenciaTests()
    {
        _contexto.SetupGet(c => c.VeterinariaId).Returns(_veterinariaId);
        _repositorioPlanes
            .Setup(r => r.ObtenerPorIdAsync(_planId, _veterinariaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(PlanTratamiento.Crear(_veterinariaId, Guid.NewGuid(), null, "Tratamiento", null, DateTimeOffset.UtcNow, null, Guid.NewGuid()));
    }

    private ObtenerAdherencia CrearCasoDeUso() => new(_contexto.Object, _repositorioPlanes.Object, _repositorioProgramaciones.Object);

    [Fact]
    public async Task EjecutarAsync_ConProgramacionesEvaluables_CalculaElPorcentajeCorrectamente()
    {
        _repositorioProgramaciones
            .Setup(r => r.ContarPorEstadoAsync(_planId, _veterinariaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ConteoProgramacionesPlan(Realizadas: 7, Omitidas: 2, Vencidas: 1, Pendientes: 3, Canceladas: 1));

        var casoDeUso = CrearCasoDeUso();

        var resultado = await casoDeUso.EjecutarAsync(_planId, CancellationToken.None);

        // 7 / (7+2+1) * 100 = 70
        Assert.Equal(70m, resultado.PorcentajeAdherencia);
        Assert.Equal(3, resultado.Pendientes);
        Assert.Equal(1, resultado.Canceladas);
    }

    [Fact]
    public async Task EjecutarAsync_SinProgramacionesEvaluables_RetornaPorcentajeNulo()
    {
        _repositorioProgramaciones
            .Setup(r => r.ContarPorEstadoAsync(_planId, _veterinariaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ConteoProgramacionesPlan(Realizadas: 0, Omitidas: 0, Vencidas: 0, Pendientes: 5, Canceladas: 0));

        var casoDeUso = CrearCasoDeUso();

        var resultado = await casoDeUso.EjecutarAsync(_planId, CancellationToken.None);

        Assert.Null(resultado.PorcentajeAdherencia);
    }

    [Fact]
    public async Task EjecutarAsync_ConPlanInexistente_LanzaExcepcionAplicacion()
    {
        var planInexistente = Guid.NewGuid();
        _repositorioPlanes.Setup(r => r.ObtenerPorIdAsync(planInexistente, _veterinariaId, It.IsAny<CancellationToken>())).ReturnsAsync((PlanTratamiento?)null);

        var casoDeUso = CrearCasoDeUso();

        var excepcion = await Assert.ThrowsAsync<ExcepcionAplicacion>(() => casoDeUso.EjecutarAsync(planInexistente, CancellationToken.None));

        Assert.Equal(CodigosError.TratamientoNoEncontrado, excepcion.Codigo);
    }
}
