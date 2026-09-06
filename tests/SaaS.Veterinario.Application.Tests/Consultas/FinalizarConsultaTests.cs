using Moq;
using SaaS.Veterinario.Application.Abstracciones;
using SaaS.Veterinario.Application.Consultas;
using SaaS.Veterinario.Application.Errores;
using SaaS.Veterinario.Domain.Citas;
using SaaS.Veterinario.Domain.Consultas;

namespace SaaS.Veterinario.Application.Tests.Consultas;

public class FinalizarConsultaTests
{
    private readonly Mock<IContextoVeterinaria> _contexto = new();
    private readonly Mock<IRepositorioConsultas> _repositorioConsultas = new();
    private readonly Mock<IRepositorioCitas> _repositorioCitas = new();
    private readonly Mock<IUnidadDeTrabajo> _unidadDeTrabajo = new();

    private readonly Guid _veterinariaId = Guid.NewGuid();

    private static readonly DateTimeOffset FechaHora = new(2026, 1, 10, 9, 0, 0, TimeSpan.Zero);

    public FinalizarConsultaTests()
    {
        _contexto.SetupGet(c => c.VeterinariaId).Returns(_veterinariaId);
        _contexto.SetupGet(c => c.UsuarioId).Returns(Guid.NewGuid());

        _unidadDeTrabajo
            .Setup(u => u.EjecutarEnTransaccionAsync(It.IsAny<Func<CancellationToken, Task>>(), It.IsAny<CancellationToken>()))
            .Returns((Func<CancellationToken, Task> operacion, CancellationToken ct) => operacion(ct));
    }

    private FinalizarConsulta CrearCasoDeUso() => new(_contexto.Object, _repositorioConsultas.Object, _repositorioCitas.Object, _unidadDeTrabajo.Object);

    private static ConsultaVeterinaria CrearConsultaBorrador(Guid veterinariaId, Guid? citaId = null)
        => ConsultaVeterinaria.Crear(
            veterinariaId, Guid.NewGuid(), citaId, Guid.NewGuid(), FechaHora, "Motivo",
            null, null, null, null, null, null, null, Guid.NewGuid());

    [Fact]
    public async Task EjecutarAsync_SinCitaVinculada_FinalizaLaConsulta()
    {
        var consulta = CrearConsultaBorrador(_veterinariaId);
        _repositorioConsultas.Setup(r => r.ObtenerPorIdAsync(consulta.Id, _veterinariaId, It.IsAny<CancellationToken>())).ReturnsAsync(consulta);

        var casoDeUso = CrearCasoDeUso();

        await casoDeUso.EjecutarAsync(consulta.Id, CancellationToken.None);

        Assert.Equal(EstadoConsulta.Finalizada, consulta.Estado);
        _repositorioCitas.Verify(r => r.ObtenerPorIdAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task EjecutarAsync_ConCitaEnEspera_FinalizaLaConsultaYMarcaLaCitaAtendida()
    {
        var cita = Cita.Crear(_veterinariaId, Guid.NewGuid(), null, null, FechaHora, FechaHora.AddMinutes(30), "Motivo", null, Guid.NewGuid());
        cita.MarcarEnEspera(Guid.NewGuid(), DateTimeOffset.UtcNow);
        var consulta = CrearConsultaBorrador(_veterinariaId, cita.Id);

        _repositorioConsultas.Setup(r => r.ObtenerPorIdAsync(consulta.Id, _veterinariaId, It.IsAny<CancellationToken>())).ReturnsAsync(consulta);
        _repositorioCitas.Setup(r => r.ObtenerPorIdAsync(cita.Id, _veterinariaId, It.IsAny<CancellationToken>())).ReturnsAsync(cita);

        var casoDeUso = CrearCasoDeUso();

        await casoDeUso.EjecutarAsync(consulta.Id, CancellationToken.None);

        Assert.Equal(EstadoConsulta.Finalizada, consulta.Estado);
        Assert.Equal(EstadoCita.Atendida, cita.Estado);
    }

    // Ajuste post-Etapa 7 (AJUSTE 1): ATENDIDA ahora es alcanzable desde PROGRAMADA, CONFIRMADA
    // o EN_ESPERA -- solo un estado realmente terminal (CANCELADA/NO_ASISTIO/ya ATENDIDA) hace
    // fallar la transicion y debe propagar/revertir toda la operacion (seccion 42/50).
    [Fact]
    public async Task EjecutarAsync_ConCitaProgramada_FinalizaLaConsultaYMarcaLaCitaAtendida()
    {
        var cita = Cita.Crear(_veterinariaId, Guid.NewGuid(), null, null, FechaHora, FechaHora.AddMinutes(30), "Motivo", null, Guid.NewGuid());
        var consulta = CrearConsultaBorrador(_veterinariaId, cita.Id);

        _repositorioConsultas.Setup(r => r.ObtenerPorIdAsync(consulta.Id, _veterinariaId, It.IsAny<CancellationToken>())).ReturnsAsync(consulta);
        _repositorioCitas.Setup(r => r.ObtenerPorIdAsync(cita.Id, _veterinariaId, It.IsAny<CancellationToken>())).ReturnsAsync(cita);

        var casoDeUso = CrearCasoDeUso();

        await casoDeUso.EjecutarAsync(consulta.Id, CancellationToken.None);

        Assert.Equal(EstadoConsulta.Finalizada, consulta.Estado);
        Assert.Equal(EstadoCita.Atendida, cita.Estado);
    }

    [Fact]
    public async Task EjecutarAsync_ConCitaConfirmada_FinalizaLaConsultaYMarcaLaCitaAtendida()
    {
        var cita = Cita.Crear(_veterinariaId, Guid.NewGuid(), null, null, FechaHora, FechaHora.AddMinutes(30), "Motivo", null, Guid.NewGuid());
        cita.Confirmar(Guid.NewGuid(), DateTimeOffset.UtcNow);
        var consulta = CrearConsultaBorrador(_veterinariaId, cita.Id);

        _repositorioConsultas.Setup(r => r.ObtenerPorIdAsync(consulta.Id, _veterinariaId, It.IsAny<CancellationToken>())).ReturnsAsync(consulta);
        _repositorioCitas.Setup(r => r.ObtenerPorIdAsync(cita.Id, _veterinariaId, It.IsAny<CancellationToken>())).ReturnsAsync(cita);

        var casoDeUso = CrearCasoDeUso();

        await casoDeUso.EjecutarAsync(consulta.Id, CancellationToken.None);

        Assert.Equal(EstadoConsulta.Finalizada, consulta.Estado);
        Assert.Equal(EstadoCita.Atendida, cita.Estado);
    }

    [Fact]
    public async Task EjecutarAsync_ConCitaCancelada_PropagaLaExcepcion()
    {
        // CANCELADA es un estado realmente terminal: MarcarAtendida debe fallar y propagar.
        var cita = Cita.Crear(_veterinariaId, Guid.NewGuid(), null, null, FechaHora, FechaHora.AddMinutes(30), "Motivo", null, Guid.NewGuid());
        cita.Cancelar(Guid.NewGuid(), DateTimeOffset.UtcNow);
        var consulta = CrearConsultaBorrador(_veterinariaId, cita.Id);

        _repositorioConsultas.Setup(r => r.ObtenerPorIdAsync(consulta.Id, _veterinariaId, It.IsAny<CancellationToken>())).ReturnsAsync(consulta);
        _repositorioCitas.Setup(r => r.ObtenerPorIdAsync(cita.Id, _veterinariaId, It.IsAny<CancellationToken>())).ReturnsAsync(cita);

        var casoDeUso = CrearCasoDeUso();

        await Assert.ThrowsAsync<ExcepcionAplicacion>(() => casoDeUso.EjecutarAsync(consulta.Id, CancellationToken.None));
    }

    [Fact]
    public async Task EjecutarAsync_ConConsultaYaFinalizada_LanzaExcepcionAplicacion()
    {
        var consulta = CrearConsultaBorrador(_veterinariaId);
        consulta.Finalizar(Guid.NewGuid(), DateTimeOffset.UtcNow);
        _repositorioConsultas.Setup(r => r.ObtenerPorIdAsync(consulta.Id, _veterinariaId, It.IsAny<CancellationToken>())).ReturnsAsync(consulta);

        var casoDeUso = CrearCasoDeUso();

        var excepcion = await Assert.ThrowsAsync<ExcepcionAplicacion>(() => casoDeUso.EjecutarAsync(consulta.Id, CancellationToken.None));

        Assert.Equal(CodigosError.ConsultaEstadoInvalido, excepcion.Codigo);
    }
}
