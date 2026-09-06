using Moq;
using SaaS.Veterinario.Application.Abstracciones;
using SaaS.Veterinario.Application.Consultas;
using SaaS.Veterinario.Application.Errores;
using SaaS.Veterinario.Domain.Consultas;

namespace SaaS.Veterinario.Application.Tests.Consultas;

public class AgregarDiagnosticoTests
{
    private readonly Mock<IContextoVeterinaria> _contexto = new();
    private readonly Mock<IRepositorioConsultas> _repositorioConsultas = new();
    private readonly Mock<IRepositorioDiagnosticos> _repositorioDiagnosticos = new();

    private readonly Guid _veterinariaId = Guid.NewGuid();

    private static readonly DateTimeOffset FechaHora = new(2026, 1, 10, 9, 0, 0, TimeSpan.Zero);

    public AgregarDiagnosticoTests()
    {
        _contexto.SetupGet(c => c.VeterinariaId).Returns(_veterinariaId);
        _repositorioDiagnosticos
            .Setup(r => r.ExistePrincipalAsync(It.IsAny<Guid>(), It.IsAny<Guid?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
    }

    private AgregarDiagnostico CrearCasoDeUso() => new(_contexto.Object, _repositorioConsultas.Object, _repositorioDiagnosticos.Object);

    private static ConsultaVeterinaria CrearConsultaBorrador(Guid veterinariaId)
        => ConsultaVeterinaria.Crear(
            veterinariaId, Guid.NewGuid(), null, Guid.NewGuid(), FechaHora, "Motivo",
            null, null, null, null, null, null, null, Guid.NewGuid());

    [Fact]
    public async Task EjecutarAsync_ConConsultaEnBorrador_AgregaElDiagnostico()
    {
        var consulta = CrearConsultaBorrador(_veterinariaId);
        _repositorioConsultas.Setup(r => r.ObtenerPorIdAsync(consulta.Id, _veterinariaId, It.IsAny<CancellationToken>())).ReturnsAsync(consulta);

        var casoDeUso = CrearCasoDeUso();
        var comando = new AgregarDiagnosticoComando(consulta.Id, "Otitis externa", "Presuntivo", true);

        var resultado = await casoDeUso.EjecutarAsync(comando, CancellationToken.None);

        Assert.NotEqual(Guid.Empty, resultado.DiagnosticoId);
        _repositorioDiagnosticos.Verify(r => r.AgregarAsync(It.IsAny<DiagnosticoConsulta>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task EjecutarAsync_ConConsultaFinalizada_LanzaExcepcionAplicacionYNoCreaNada()
    {
        var consulta = CrearConsultaBorrador(_veterinariaId);
        consulta.Finalizar(Guid.NewGuid(), DateTimeOffset.UtcNow);
        _repositorioConsultas.Setup(r => r.ObtenerPorIdAsync(consulta.Id, _veterinariaId, It.IsAny<CancellationToken>())).ReturnsAsync(consulta);

        var casoDeUso = CrearCasoDeUso();
        var comando = new AgregarDiagnosticoComando(consulta.Id, "Otitis externa", "Presuntivo", false);

        var excepcion = await Assert.ThrowsAsync<ExcepcionAplicacion>(() => casoDeUso.EjecutarAsync(comando, CancellationToken.None));

        Assert.Equal(CodigosError.DiagnosticoConsultaNoEditable, excepcion.Codigo);
        _repositorioDiagnosticos.Verify(r => r.AgregarAsync(It.IsAny<DiagnosticoConsulta>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task EjecutarAsync_ConDiagnosticoPrincipalYaExistente_LanzaExcepcionAplicacion()
    {
        var consulta = CrearConsultaBorrador(_veterinariaId);
        _repositorioConsultas.Setup(r => r.ObtenerPorIdAsync(consulta.Id, _veterinariaId, It.IsAny<CancellationToken>())).ReturnsAsync(consulta);
        _repositorioDiagnosticos
            .Setup(r => r.ExistePrincipalAsync(consulta.Id, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var casoDeUso = CrearCasoDeUso();
        var comando = new AgregarDiagnosticoComando(consulta.Id, "Segundo diagnóstico principal", "Confirmado", true);

        var excepcion = await Assert.ThrowsAsync<ExcepcionAplicacion>(() => casoDeUso.EjecutarAsync(comando, CancellationToken.None));

        Assert.Equal(CodigosError.DiagnosticoPrincipalExistente, excepcion.Codigo);
    }

    [Fact]
    public async Task EjecutarAsync_ConTipoInvalido_LanzaExcepcionAplicacion()
    {
        var consulta = CrearConsultaBorrador(_veterinariaId);
        _repositorioConsultas.Setup(r => r.ObtenerPorIdAsync(consulta.Id, _veterinariaId, It.IsAny<CancellationToken>())).ReturnsAsync(consulta);

        var casoDeUso = CrearCasoDeUso();
        var comando = new AgregarDiagnosticoComando(consulta.Id, "Descripción", "NoExiste", false);

        var excepcion = await Assert.ThrowsAsync<ExcepcionAplicacion>(() => casoDeUso.EjecutarAsync(comando, CancellationToken.None));

        Assert.Equal(CodigosError.DiagnosticoTipoInvalido, excepcion.Codigo);
    }
}
