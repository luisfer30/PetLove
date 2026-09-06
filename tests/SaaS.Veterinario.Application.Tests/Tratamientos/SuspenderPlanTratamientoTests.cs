using Moq;
using SaaS.Veterinario.Application.Abstracciones;
using SaaS.Veterinario.Application.Tratamientos;
using SaaS.Veterinario.Domain.Tratamientos;

namespace SaaS.Veterinario.Application.Tests.Tratamientos;

public class SuspenderPlanTratamientoTests
{
    private readonly Mock<IContextoVeterinaria> _contexto = new();
    private readonly Mock<IRepositorioPlanesTratamiento> _repositorioPlanes = new();
    private readonly Mock<IRepositorioProgramacionesTratamiento> _repositorioProgramaciones = new();
    private readonly Mock<IUnidadDeTrabajo> _unidadDeTrabajo = new();

    private readonly Guid _veterinariaId = Guid.NewGuid();

    public SuspenderPlanTratamientoTests()
    {
        _contexto.SetupGet(c => c.VeterinariaId).Returns(_veterinariaId);
        _contexto.SetupGet(c => c.UsuarioId).Returns(Guid.NewGuid());
        _contexto.Setup(c => c.TienePermiso("clinica.ejercer_veterinario")).Returns(true);

        _unidadDeTrabajo
            .Setup(u => u.EjecutarEnTransaccionAsync(It.IsAny<Func<CancellationToken, Task>>(), It.IsAny<CancellationToken>()))
            .Returns((Func<CancellationToken, Task> operacion, CancellationToken ct) => operacion(ct));
    }

    private SuspenderPlanTratamiento CrearCasoDeUso() => new(_contexto.Object, _repositorioPlanes.Object, _repositorioProgramaciones.Object, _unidadDeTrabajo.Object);

    [Fact]
    public async Task EjecutarAsync_ConPlanActivo_SuspendeYCancelaProgramacionesFuturasPendientes()
    {
        var plan = PlanTratamiento.Crear(_veterinariaId, Guid.NewGuid(), null, "Tratamiento", null, DateTimeOffset.UtcNow, null, Guid.NewGuid());
        plan.Activar(Guid.NewGuid(), DateTimeOffset.UtcNow);
        _repositorioPlanes.Setup(r => r.ObtenerPorIdAsync(plan.Id, _veterinariaId, It.IsAny<CancellationToken>())).ReturnsAsync(plan);

        var pendiente = Domain.Tratamientos.ProgramacionTratamiento.Crear(_veterinariaId, plan.MascotaVeterinariaId, plan.Id, Guid.NewGuid(), DateTimeOffset.UtcNow.AddDays(1));
        _repositorioProgramaciones.Setup(r => r.ListarPendientesPorPlanAsync(plan.Id, It.IsAny<CancellationToken>())).ReturnsAsync([pendiente]);

        var casoDeUso = CrearCasoDeUso();

        await casoDeUso.EjecutarAsync(new SuspenderPlanTratamientoComando(plan.Id, "Efectos adversos"), CancellationToken.None);

        Assert.Equal(EstadoPlanTratamiento.Suspendido, plan.Estado);
        Assert.Equal(EstadoProgramacionTratamiento.Cancelada, pendiente.Estado);
        _repositorioProgramaciones.Verify(r => r.GuardarCambiosAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
