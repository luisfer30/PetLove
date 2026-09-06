using Moq;
using SaaS.Veterinario.Application.Abstracciones;
using SaaS.Veterinario.Application.Consultas;
using SaaS.Veterinario.Application.Errores;
using SaaS.Veterinario.Domain.Citas;
using SaaS.Veterinario.Domain.Consultas;

namespace SaaS.Veterinario.Application.Tests.Consultas;

public class ObtenerHistorialMascotaTests
{
    private readonly Mock<IContextoVeterinaria> _contexto = new();
    private readonly Mock<IRepositorioMascotasVeterinarias> _repositorioMascotasVeterinarias = new();
    private readonly Mock<IRepositorioCitas> _repositorioCitas = new();
    private readonly Mock<IRepositorioConsultas> _repositorioConsultas = new();
    private readonly Mock<IRepositorioDiagnosticos> _repositorioDiagnosticos = new();

    private readonly Guid _veterinariaId = Guid.NewGuid();
    private readonly Guid _mascotaVeterinariaId = Guid.NewGuid();

    public ObtenerHistorialMascotaTests()
    {
        _contexto.SetupGet(c => c.VeterinariaId).Returns(_veterinariaId);
    }

    private ObtenerHistorialMascota CrearCasoDeUso() => new(
        _contexto.Object, _repositorioMascotasVeterinarias.Object, _repositorioCitas.Object, _repositorioConsultas.Object, _repositorioDiagnosticos.Object);

    [Fact]
    public async Task EjecutarAsync_ConFichaInexistenteEnLaVeterinaria_LanzaExcepcionAplicacion()
    {
        _repositorioMascotasVeterinarias
            .Setup(r => r.ObtenerFichaPorIdAsync(_mascotaVeterinariaId, _veterinariaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((InfoMascotaFicha?)null);

        var casoDeUso = CrearCasoDeUso();

        var excepcion = await Assert.ThrowsAsync<ExcepcionAplicacion>(() => casoDeUso.EjecutarAsync(_mascotaVeterinariaId, CancellationToken.None));

        Assert.Equal(CodigosError.MascotaVeterinariaNoEncontrada, excepcion.Codigo);
    }

    [Fact]
    public async Task EjecutarAsync_ConsultaSoloElTenantDelContextoYOrdenaDeMasRecienteAPrimero()
    {
        var ficha = new InfoMascotaFicha(_mascotaVeterinariaId, Guid.NewGuid(), "PET-ABC123", "Toby", "Perro", null, "Macho", null, "ACTIVA");
        _repositorioMascotasVeterinarias
            .Setup(r => r.ObtenerFichaPorIdAsync(_mascotaVeterinariaId, _veterinariaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ficha);

        var citaAntigua = Cita.Crear(
            _veterinariaId, _mascotaVeterinariaId, null, null,
            new DateTimeOffset(2026, 1, 1, 9, 0, 0, TimeSpan.Zero), new DateTimeOffset(2026, 1, 1, 9, 30, 0, TimeSpan.Zero),
            "Primera visita", null, Guid.NewGuid());

        var consultaReciente = ConsultaVeterinaria.Crear(
            _veterinariaId, _mascotaVeterinariaId, null, Guid.NewGuid(),
            new DateTimeOffset(2026, 2, 1, 9, 0, 0, TimeSpan.Zero), "Control", null, null, null, null, null, null, null, Guid.NewGuid());

        _repositorioCitas
            .Setup(r => r.ListarPorMascotaVeterinariaAsync(_mascotaVeterinariaId, _veterinariaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync([citaAntigua]);
        _repositorioConsultas
            .Setup(r => r.ListarPorMascotaVeterinariaAsync(_mascotaVeterinariaId, _veterinariaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync([consultaReciente]);
        _repositorioDiagnosticos
            .Setup(r => r.ListarPorConsultasAsync(It.Is<IReadOnlyCollection<Guid>>(ids => ids.Contains(consultaReciente.Id)), It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        var casoDeUso = CrearCasoDeUso();

        var resultado = await casoDeUso.EjecutarAsync(_mascotaVeterinariaId, CancellationToken.None);

        Assert.Equal("Toby", resultado.NombreMascota);
        Assert.Equal(2, resultado.Eventos.Count);
        Assert.Equal("Consulta", resultado.Eventos.First().Tipo);
        Assert.Equal("Cita", resultado.Eventos.Last().Tipo);

        _repositorioCitas.Verify(r => r.ListarPorMascotaVeterinariaAsync(_mascotaVeterinariaId, _veterinariaId, It.IsAny<CancellationToken>()), Times.Once);
        _repositorioConsultas.Verify(r => r.ListarPorMascotaVeterinariaAsync(_mascotaVeterinariaId, _veterinariaId, It.IsAny<CancellationToken>()), Times.Once);
    }
}
