using Moq;
using SaaS.Veterinario.Application.Abstracciones;
using SaaS.Veterinario.Application.Errores;
using SaaS.Veterinario.Application.Tratamientos;
using SaaS.Veterinario.Domain.Consultas;
using SaaS.Veterinario.Domain.Mascotas;

namespace SaaS.Veterinario.Application.Tests.Tratamientos;

public class CrearPlanTratamientoTests
{
    private readonly Mock<IContextoVeterinaria> _contexto = new();
    private readonly Mock<IRepositorioPlanesTratamiento> _repositorioPlanes = new();
    private readonly Mock<IRepositorioMascotasVeterinarias> _repositorioMascotasVeterinarias = new();
    private readonly Mock<IRepositorioConsultas> _repositorioConsultas = new();

    private readonly Guid _veterinariaId = Guid.NewGuid();
    private readonly Guid _mascotaVeterinariaId = Guid.NewGuid();

    private static readonly DateTimeOffset Inicio = new(2026, 1, 10, 9, 0, 0, TimeSpan.Zero);

    public CrearPlanTratamientoTests()
    {
        _contexto.SetupGet(c => c.VeterinariaId).Returns(_veterinariaId);
        _contexto.SetupGet(c => c.UsuarioId).Returns(Guid.NewGuid());
        _contexto.Setup(c => c.TienePermiso("clinica.ejercer_veterinario")).Returns(true);

        _repositorioMascotasVeterinarias
            .Setup(r => r.ObtenerPorIdAsync(_mascotaVeterinariaId, _veterinariaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(MascotaVeterinaria.Crear(_veterinariaId, Guid.NewGuid()));
    }

    private CrearPlanTratamiento CrearCasoDeUso() => new(_contexto.Object, _repositorioPlanes.Object, _repositorioMascotasVeterinarias.Object, _repositorioConsultas.Object);

    private CrearPlanTratamientoComando ComandoValido(Guid? consultaId = null) => new(_mascotaVeterinariaId, consultaId, "Tratamiento post-cirugía", null, Inicio, null);

    [Fact]
    public async Task EjecutarAsync_ConDatosValidos_CreaElPlanPendiente()
    {
        var casoDeUso = CrearCasoDeUso();

        var resultado = await casoDeUso.EjecutarAsync(ComandoValido(), CancellationToken.None);

        Assert.Equal(Domain.Tratamientos.EstadoPlanTratamiento.Pendiente, resultado.Estado);
        _repositorioPlanes.Verify(r => r.AgregarAsync(It.IsAny<Domain.Tratamientos.PlanTratamiento>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task EjecutarAsync_SinPermisoClinico_LanzaExcepcionAplicacion()
    {
        _contexto.Setup(c => c.TienePermiso("clinica.ejercer_veterinario")).Returns(false);
        var casoDeUso = CrearCasoDeUso();

        var excepcion = await Assert.ThrowsAsync<ExcepcionAplicacion>(() => casoDeUso.EjecutarAsync(ComandoValido(), CancellationToken.None));

        Assert.Equal(CodigosError.AutorizacionPermisoRequerido, excepcion.Codigo);
    }

    [Fact]
    public async Task EjecutarAsync_ConMascotaVeterinariaAjena_LanzaExcepcionAplicacion()
    {
        _repositorioMascotasVeterinarias
            .Setup(r => r.ObtenerPorIdAsync(_mascotaVeterinariaId, _veterinariaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((MascotaVeterinaria?)null);

        var casoDeUso = CrearCasoDeUso();

        var excepcion = await Assert.ThrowsAsync<ExcepcionAplicacion>(() => casoDeUso.EjecutarAsync(ComandoValido(), CancellationToken.None));

        Assert.Equal(CodigosError.MascotaVeterinariaNoEncontrada, excepcion.Codigo);
    }

    [Fact]
    public async Task EjecutarAsync_ConConsultaInexistente_LanzaExcepcionAplicacion()
    {
        var consultaId = Guid.NewGuid();
        _repositorioConsultas
            .Setup(r => r.ObtenerPorIdAsync(consultaId, _veterinariaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ConsultaVeterinaria?)null);

        var casoDeUso = CrearCasoDeUso();

        var excepcion = await Assert.ThrowsAsync<ExcepcionAplicacion>(() => casoDeUso.EjecutarAsync(ComandoValido(consultaId), CancellationToken.None));

        Assert.Equal(CodigosError.TratamientoConsultaInvalida, excepcion.Codigo);
    }

    [Fact]
    public async Task EjecutarAsync_ConConsultaDeOtraMascota_LanzaExcepcionAplicacion()
    {
        var consulta = ConsultaVeterinaria.Crear(
            _veterinariaId, Guid.NewGuid(), null, Guid.NewGuid(), Inicio, "Motivo",
            null, null, null, null, null, null, null, Guid.NewGuid());
        _repositorioConsultas.Setup(r => r.ObtenerPorIdAsync(consulta.Id, _veterinariaId, It.IsAny<CancellationToken>())).ReturnsAsync(consulta);

        var casoDeUso = CrearCasoDeUso();

        var excepcion = await Assert.ThrowsAsync<ExcepcionAplicacion>(() => casoDeUso.EjecutarAsync(ComandoValido(consulta.Id), CancellationToken.None));

        Assert.Equal(CodigosError.TratamientoConsultaInvalida, excepcion.Codigo);
    }

    [Fact]
    public async Task EjecutarAsync_ConConsultaBorrador_CreaElPlanPendiente()
    {
        var consulta = ConsultaVeterinaria.Crear(
            _veterinariaId, _mascotaVeterinariaId, null, Guid.NewGuid(), Inicio, "Motivo",
            null, null, null, null, null, null, null, Guid.NewGuid());
        _repositorioConsultas.Setup(r => r.ObtenerPorIdAsync(consulta.Id, _veterinariaId, It.IsAny<CancellationToken>())).ReturnsAsync(consulta);

        var casoDeUso = CrearCasoDeUso();

        var resultado = await casoDeUso.EjecutarAsync(ComandoValido(consulta.Id), CancellationToken.None);

        Assert.Equal(Domain.Tratamientos.EstadoPlanTratamiento.Pendiente, resultado.Estado);
    }

    [Fact]
    public async Task EjecutarAsync_ConConsultaAnulada_LanzaExcepcionAplicacion()
    {
        var consulta = ConsultaVeterinaria.Crear(
            _veterinariaId, _mascotaVeterinariaId, null, Guid.NewGuid(), Inicio, "Motivo",
            null, null, null, null, null, null, null, Guid.NewGuid());
        consulta.Anular(Guid.NewGuid(), "Motivo", DateTimeOffset.UtcNow);
        _repositorioConsultas.Setup(r => r.ObtenerPorIdAsync(consulta.Id, _veterinariaId, It.IsAny<CancellationToken>())).ReturnsAsync(consulta);

        var casoDeUso = CrearCasoDeUso();

        var excepcion = await Assert.ThrowsAsync<ExcepcionAplicacion>(() => casoDeUso.EjecutarAsync(ComandoValido(consulta.Id), CancellationToken.None));

        Assert.Equal(CodigosError.TratamientoConsultaInvalida, excepcion.Codigo);
    }
}
