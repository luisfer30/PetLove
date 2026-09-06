using Moq;
using SaaS.Veterinario.Application.Abstracciones;
using SaaS.Veterinario.Application.Errores;
using SaaS.Veterinario.Application.Tratamientos;
using SaaS.Veterinario.Domain.Consultas;
using SaaS.Veterinario.Domain.Mascotas;
using SaaS.Veterinario.Domain.Tratamientos;
using SaaS.Veterinario.Domain.Veterinarias;

namespace SaaS.Veterinario.Application.Tests.Tratamientos;

public class ActivarPlanTratamientoTests
{
    private readonly Mock<IContextoVeterinaria> _contexto = new();
    private readonly Mock<IRepositorioPlanesTratamiento> _repositorioPlanes = new();
    private readonly Mock<IRepositorioItemsTratamiento> _repositorioItems = new();
    private readonly Mock<IRepositorioProgramacionesTratamiento> _repositorioProgramaciones = new();
    private readonly Mock<IRepositorioConsultas> _repositorioConsultas = new();
    private readonly Mock<IRepositorioVeterinarias> _repositorioVeterinarias = new();
    private readonly Mock<IGeneradorProgramacionesTratamiento> _generador = new();
    private readonly Mock<IUnidadDeTrabajo> _unidadDeTrabajo = new();

    private readonly Guid _veterinariaId = Guid.NewGuid();

    private static readonly DateTimeOffset Inicio = new(2026, 1, 10, 9, 0, 0, TimeSpan.Zero);

    public ActivarPlanTratamientoTests()
    {
        _contexto.SetupGet(c => c.VeterinariaId).Returns(_veterinariaId);
        _contexto.SetupGet(c => c.UsuarioId).Returns(Guid.NewGuid());
        _contexto.Setup(c => c.TienePermiso("clinica.ejercer_veterinario")).Returns(true);

        _unidadDeTrabajo
            .Setup(u => u.EjecutarEnTransaccionAsync(It.IsAny<Func<CancellationToken, Task>>(), It.IsAny<CancellationToken>()))
            .Returns((Func<CancellationToken, Task> operacion, CancellationToken ct) => operacion(ct));

        _repositorioVeterinarias
            .Setup(r => r.ObtenerPorIdAsync(_veterinariaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Veterinaria.Crear("VET-ABC123", "Clinica", "contacto@clinica.com", "Ecuador", "America/Guayaquil"));

        _repositorioItems.Setup(r => r.ListarPorPlanAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync([]);
    }

    private ActivarPlanTratamiento CrearCasoDeUso() => new(
        _contexto.Object, _repositorioPlanes.Object, _repositorioItems.Object, _repositorioProgramaciones.Object,
        _repositorioConsultas.Object, _repositorioVeterinarias.Object, _generador.Object, _unidadDeTrabajo.Object);

    private static PlanTratamiento CrearPlanPendiente(Guid veterinariaId, Guid? consultaId = null)
        => PlanTratamiento.Crear(veterinariaId, Guid.NewGuid(), consultaId, "Tratamiento", null, Inicio, null, Guid.NewGuid());

    [Fact]
    public async Task EjecutarAsync_SinConsultaAsociada_ActivaElPlan()
    {
        var plan = CrearPlanPendiente(_veterinariaId);
        _repositorioPlanes.Setup(r => r.ObtenerPorIdAsync(plan.Id, _veterinariaId, It.IsAny<CancellationToken>())).ReturnsAsync(plan);

        var casoDeUso = CrearCasoDeUso();

        await casoDeUso.EjecutarAsync(plan.Id, CancellationToken.None);

        Assert.Equal(EstadoPlanTratamiento.Activo, plan.Estado);
    }

    [Fact]
    public async Task EjecutarAsync_ConConsultaBorrador_LanzaExcepcionAplicacionYNoActiva()
    {
        var consulta = ConsultaVeterinaria.Crear(
            _veterinariaId, Guid.NewGuid(), null, Guid.NewGuid(), Inicio, "Motivo",
            null, null, null, null, null, null, null, Guid.NewGuid());
        var plan = CrearPlanPendiente(_veterinariaId, consulta.Id);
        _repositorioPlanes.Setup(r => r.ObtenerPorIdAsync(plan.Id, _veterinariaId, It.IsAny<CancellationToken>())).ReturnsAsync(plan);
        _repositorioConsultas.Setup(r => r.ObtenerPorIdAsync(consulta.Id, _veterinariaId, It.IsAny<CancellationToken>())).ReturnsAsync(consulta);

        var casoDeUso = CrearCasoDeUso();

        var excepcion = await Assert.ThrowsAsync<ExcepcionAplicacion>(() => casoDeUso.EjecutarAsync(plan.Id, CancellationToken.None));

        Assert.Equal(CodigosError.TratamientoRequiereConsultaFinalizada, excepcion.Codigo);
        Assert.Equal(EstadoPlanTratamiento.Pendiente, plan.Estado);
    }

    [Fact]
    public async Task EjecutarAsync_ConConsultaFinalizada_ActivaElPlan()
    {
        var consulta = ConsultaVeterinaria.Crear(
            _veterinariaId, Guid.NewGuid(), null, Guid.NewGuid(), Inicio, "Motivo",
            null, null, null, null, null, null, null, Guid.NewGuid());
        consulta.Finalizar(Guid.NewGuid(), DateTimeOffset.UtcNow);
        var plan = CrearPlanPendiente(_veterinariaId, consulta.Id);
        _repositorioPlanes.Setup(r => r.ObtenerPorIdAsync(plan.Id, _veterinariaId, It.IsAny<CancellationToken>())).ReturnsAsync(plan);
        _repositorioConsultas.Setup(r => r.ObtenerPorIdAsync(consulta.Id, _veterinariaId, It.IsAny<CancellationToken>())).ReturnsAsync(consulta);

        var casoDeUso = CrearCasoDeUso();

        await casoDeUso.EjecutarAsync(plan.Id, CancellationToken.None);

        Assert.Equal(EstadoPlanTratamiento.Activo, plan.Estado);
    }

    [Fact]
    public async Task EjecutarAsync_ConItemsActivos_GeneraProgramaciones()
    {
        var plan = CrearPlanPendiente(_veterinariaId);
        _repositorioPlanes.Setup(r => r.ObtenerPorIdAsync(plan.Id, _veterinariaId, It.IsAny<CancellationToken>())).ReturnsAsync(plan);

        var item = ItemTratamiento.Crear(
            plan.Id, TipoItemTratamiento.Medicamento, "Amoxicilina", null, 1m, "tableta", null,
            TipoFrecuenciaTratamiento.IntervaloHoras, 8, null, [], Inicio, Inicio.AddDays(1), null, null, 0);
        _repositorioItems.Setup(r => r.ListarPorPlanAsync(plan.Id, It.IsAny<CancellationToken>())).ReturnsAsync([item]);

        var instantes = new List<DateTimeOffset> { Inicio, Inicio.AddHours(8), Inicio.AddHours(16) };
        _generador.Setup(g => g.Generar(It.IsAny<ParametrosGeneracionProgramaciones>())).Returns(instantes);
        _repositorioProgramaciones
            .Setup(r => r.ExisteEnFechaAsync(item.Id, It.IsAny<DateTimeOffset>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var casoDeUso = CrearCasoDeUso();

        await casoDeUso.EjecutarAsync(plan.Id, CancellationToken.None);

        _repositorioProgramaciones.Verify(
            r => r.AgregarRangoAsync(It.Is<IReadOnlyCollection<Domain.Tratamientos.ProgramacionTratamiento>>(p => p.Count == 3), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task EjecutarAsync_ConPlanYaActivo_LanzaExcepcionAplicacion()
    {
        var plan = CrearPlanPendiente(_veterinariaId);
        plan.Activar(Guid.NewGuid(), DateTimeOffset.UtcNow);
        _repositorioPlanes.Setup(r => r.ObtenerPorIdAsync(plan.Id, _veterinariaId, It.IsAny<CancellationToken>())).ReturnsAsync(plan);

        var casoDeUso = CrearCasoDeUso();

        var excepcion = await Assert.ThrowsAsync<ExcepcionAplicacion>(() => casoDeUso.EjecutarAsync(plan.Id, CancellationToken.None));

        Assert.Equal(CodigosError.TratamientoEstadoInvalido, excepcion.Codigo);
    }
}
