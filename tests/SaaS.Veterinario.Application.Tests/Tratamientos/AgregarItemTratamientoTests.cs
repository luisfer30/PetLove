using Moq;
using SaaS.Veterinario.Application.Abstracciones;
using SaaS.Veterinario.Application.Errores;
using SaaS.Veterinario.Application.Tratamientos;
using SaaS.Veterinario.Domain.Tratamientos;
using SaaS.Veterinario.Domain.Veterinarias;

namespace SaaS.Veterinario.Application.Tests.Tratamientos;

public class AgregarItemTratamientoTests
{
    private readonly Mock<IContextoVeterinaria> _contexto = new();
    private readonly Mock<IRepositorioPlanesTratamiento> _repositorioPlanes = new();
    private readonly Mock<IRepositorioItemsTratamiento> _repositorioItems = new();
    private readonly Mock<IRepositorioProgramacionesTratamiento> _repositorioProgramaciones = new();
    private readonly Mock<IRepositorioVeterinarias> _repositorioVeterinarias = new();
    private readonly Mock<IGeneradorProgramacionesTratamiento> _generador = new();
    private readonly Mock<IUnidadDeTrabajo> _unidadDeTrabajo = new();

    private readonly Guid _veterinariaId = Guid.NewGuid();

    private static readonly DateTimeOffset Inicio = new(2026, 1, 10, 9, 0, 0, TimeSpan.Zero);

    public AgregarItemTratamientoTests()
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
    }

    private AgregarItemTratamiento CrearCasoDeUso() => new(
        _contexto.Object, _repositorioPlanes.Object, _repositorioItems.Object, _repositorioProgramaciones.Object,
        _repositorioVeterinarias.Object, _generador.Object, _unidadDeTrabajo.Object);

    private AgregarItemTratamientoComando ComandoValido(Guid planId) => new(
        planId, "Medicamento", "Amoxicilina", null, 1m, "tableta", "Oral",
        "IntervaloHoras", 8, null, [], Inicio, Inicio.AddDays(5), null, "Con comida", 0);

    [Fact]
    public async Task EjecutarAsync_ConPlanPendiente_AgregaElItemSinGenerarProgramaciones()
    {
        var plan = PlanTratamiento.Crear(_veterinariaId, Guid.NewGuid(), null, "Tratamiento", null, Inicio, null, Guid.NewGuid());
        _repositorioPlanes.Setup(r => r.ObtenerPorIdAsync(plan.Id, _veterinariaId, It.IsAny<CancellationToken>())).ReturnsAsync(plan);

        var casoDeUso = CrearCasoDeUso();

        var resultado = await casoDeUso.EjecutarAsync(ComandoValido(plan.Id), CancellationToken.None);

        Assert.NotEqual(Guid.Empty, resultado.ItemTratamientoId);
        _repositorioItems.Verify(r => r.AgregarAsync(It.IsAny<ItemTratamiento>(), It.IsAny<CancellationToken>()), Times.Once);
        _generador.Verify(g => g.Generar(It.IsAny<ParametrosGeneracionProgramaciones>()), Times.Never);
    }

    [Fact]
    public async Task EjecutarAsync_ConPlanActivo_AgregaElItemYGeneraSusProgramaciones()
    {
        var plan = PlanTratamiento.Crear(_veterinariaId, Guid.NewGuid(), null, "Tratamiento", null, Inicio, null, Guid.NewGuid());
        plan.Activar(Guid.NewGuid(), DateTimeOffset.UtcNow);
        _repositorioPlanes.Setup(r => r.ObtenerPorIdAsync(plan.Id, _veterinariaId, It.IsAny<CancellationToken>())).ReturnsAsync(plan);

        _generador.Setup(g => g.Generar(It.IsAny<ParametrosGeneracionProgramaciones>())).Returns([Inicio, Inicio.AddHours(8)]);
        _repositorioProgramaciones
            .Setup(r => r.ExisteEnFechaAsync(It.IsAny<Guid>(), It.IsAny<DateTimeOffset>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var casoDeUso = CrearCasoDeUso();

        await casoDeUso.EjecutarAsync(ComandoValido(plan.Id), CancellationToken.None);

        _repositorioProgramaciones.Verify(
            r => r.AgregarRangoAsync(It.Is<IReadOnlyCollection<Domain.Tratamientos.ProgramacionTratamiento>>(p => p.Count == 2), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task EjecutarAsync_ConPlanCompletado_LanzaExcepcionAplicacion()
    {
        var plan = PlanTratamiento.Crear(_veterinariaId, Guid.NewGuid(), null, "Tratamiento", null, Inicio, null, Guid.NewGuid());
        plan.Activar(Guid.NewGuid(), DateTimeOffset.UtcNow);
        plan.Completar(Guid.NewGuid(), DateTimeOffset.UtcNow);
        _repositorioPlanes.Setup(r => r.ObtenerPorIdAsync(plan.Id, _veterinariaId, It.IsAny<CancellationToken>())).ReturnsAsync(plan);

        var casoDeUso = CrearCasoDeUso();

        var excepcion = await Assert.ThrowsAsync<ExcepcionAplicacion>(() => casoDeUso.EjecutarAsync(ComandoValido(plan.Id), CancellationToken.None));

        Assert.Equal(CodigosError.TratamientoEstadoInvalido, excepcion.Codigo);
    }

    [Fact]
    public async Task EjecutarAsync_ConHorariosFaltantesParaHoraFija_LanzaExcepcionAplicacion()
    {
        var plan = PlanTratamiento.Crear(_veterinariaId, Guid.NewGuid(), null, "Tratamiento", null, Inicio, null, Guid.NewGuid());
        _repositorioPlanes.Setup(r => r.ObtenerPorIdAsync(plan.Id, _veterinariaId, It.IsAny<CancellationToken>())).ReturnsAsync(plan);

        var comando = new AgregarItemTratamientoComando(
            plan.Id, "Cuidado", "Limpieza", null, null, null, null, "HoraFija", null, null, [], Inicio, Inicio.AddDays(1), null, null, 0);

        var casoDeUso = CrearCasoDeUso();

        var excepcion = await Assert.ThrowsAsync<ExcepcionAplicacion>(() => casoDeUso.EjecutarAsync(comando, CancellationToken.None));

        Assert.Equal(CodigosError.TratamientoHorarioInvalido, excepcion.Codigo);
    }
}
