using Moq;
using SaaS.Veterinario.Application.Abstracciones;
using SaaS.Veterinario.Application.Tratamientos;
using SaaS.Veterinario.Domain.Tratamientos;

namespace SaaS.Veterinario.Application.Tests.Tratamientos;

public class CompletarPlanTratamientoTests
{
    private readonly Mock<IContextoVeterinaria> _contexto = new();
    private readonly Mock<IRepositorioPlanesTratamiento> _repositorioPlanes = new();
    private readonly Mock<IRepositorioItemsTratamiento> _repositorioItems = new();
    private readonly Mock<IRepositorioProgramacionesTratamiento> _repositorioProgramaciones = new();
    private readonly Mock<IUnidadDeTrabajo> _unidadDeTrabajo = new();

    private readonly Guid _veterinariaId = Guid.NewGuid();

    public CompletarPlanTratamientoTests()
    {
        _contexto.SetupGet(c => c.VeterinariaId).Returns(_veterinariaId);
        _contexto.SetupGet(c => c.UsuarioId).Returns(Guid.NewGuid());
        _contexto.Setup(c => c.TienePermiso("clinica.ejercer_veterinario")).Returns(true);

        _unidadDeTrabajo
            .Setup(u => u.EjecutarEnTransaccionAsync(It.IsAny<Func<CancellationToken, Task>>(), It.IsAny<CancellationToken>()))
            .Returns((Func<CancellationToken, Task> operacion, CancellationToken ct) => operacion(ct));

        _repositorioItems.Setup(r => r.ListarPorPlanAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync([]);
    }

    private CompletarPlanTratamiento CrearCasoDeUso() => new(
        _contexto.Object, _repositorioPlanes.Object, _repositorioItems.Object, _repositorioProgramaciones.Object, _unidadDeTrabajo.Object);

    [Fact]
    public async Task EjecutarAsync_ConPlanActivo_CompletaYCancelaProgramacionesFuturasPendientes()
    {
        var plan = PlanTratamiento.Crear(_veterinariaId, Guid.NewGuid(), null, "Tratamiento", null, DateTimeOffset.UtcNow, null, Guid.NewGuid());
        plan.Activar(Guid.NewGuid(), DateTimeOffset.UtcNow);
        _repositorioPlanes.Setup(r => r.ObtenerPorIdAsync(plan.Id, _veterinariaId, It.IsAny<CancellationToken>())).ReturnsAsync(plan);

        var pendiente = Domain.Tratamientos.ProgramacionTratamiento.Crear(_veterinariaId, plan.MascotaVeterinariaId, plan.Id, Guid.NewGuid(), DateTimeOffset.UtcNow.AddDays(1));
        _repositorioProgramaciones.Setup(r => r.ListarPendientesPorPlanAsync(plan.Id, It.IsAny<CancellationToken>())).ReturnsAsync([pendiente]);

        var casoDeUso = CrearCasoDeUso();

        await casoDeUso.EjecutarAsync(plan.Id, CancellationToken.None);

        Assert.Equal(EstadoPlanTratamiento.Completado, plan.Estado);
        Assert.Equal(EstadoProgramacionTratamiento.Cancelada, pendiente.Estado);
    }

    [Fact]
    public async Task EjecutarAsync_ConItemsActivos_LosCompletaTambien()
    {
        var plan = PlanTratamiento.Crear(_veterinariaId, Guid.NewGuid(), null, "Tratamiento", null, DateTimeOffset.UtcNow, null, Guid.NewGuid());
        plan.Activar(Guid.NewGuid(), DateTimeOffset.UtcNow);
        _repositorioPlanes.Setup(r => r.ObtenerPorIdAsync(plan.Id, _veterinariaId, It.IsAny<CancellationToken>())).ReturnsAsync(plan);
        _repositorioProgramaciones.Setup(r => r.ListarPendientesPorPlanAsync(plan.Id, It.IsAny<CancellationToken>())).ReturnsAsync([]);

        var item = ItemTratamiento.Crear(
            plan.Id, TipoItemTratamiento.Reposo, "Reposo", null, null, null, null,
            TipoFrecuenciaTratamiento.Unica, null, null, [], DateTimeOffset.UtcNow, null, null, null, 0);
        _repositorioItems.Setup(r => r.ListarPorPlanAsync(plan.Id, It.IsAny<CancellationToken>())).ReturnsAsync([item]);

        var casoDeUso = CrearCasoDeUso();

        await casoDeUso.EjecutarAsync(plan.Id, CancellationToken.None);

        Assert.Equal(EstadoItemTratamiento.Completado, item.Estado);
    }
}
