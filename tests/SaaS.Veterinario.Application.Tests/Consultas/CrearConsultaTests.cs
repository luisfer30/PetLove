using Moq;
using SaaS.Veterinario.Application.Abstracciones;
using SaaS.Veterinario.Application.Consultas;
using SaaS.Veterinario.Application.Errores;
using SaaS.Veterinario.Domain.Citas;
using SaaS.Veterinario.Domain.Mascotas;

namespace SaaS.Veterinario.Application.Tests.Consultas;

public class CrearConsultaTests
{
    private readonly Mock<IContextoVeterinaria> _contexto = new();
    private readonly Mock<IRepositorioConsultas> _repositorioConsultas = new();
    private readonly Mock<IRepositorioMascotasVeterinarias> _repositorioMascotasVeterinarias = new();
    private readonly Mock<IRepositorioCitas> _repositorioCitas = new();
    private readonly Mock<IRepositorioMembresias> _repositorioMembresias = new();

    private readonly Guid _veterinariaId = Guid.NewGuid();
    private readonly Guid _usuarioId = Guid.NewGuid();
    private readonly Guid _mascotaVeterinariaId = Guid.NewGuid();
    private readonly Guid _veterinarioId = Guid.NewGuid();

    private static readonly DateTimeOffset FechaHora = new(2026, 1, 10, 9, 0, 0, TimeSpan.Zero);

    public CrearConsultaTests()
    {
        _contexto.SetupGet(c => c.VeterinariaId).Returns(_veterinariaId);
        _contexto.SetupGet(c => c.UsuarioId).Returns(_usuarioId);

        _repositorioMascotasVeterinarias
            .Setup(r => r.ObtenerPorIdAsync(_mascotaVeterinariaId, _veterinariaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(MascotaVeterinaria.Crear(_veterinariaId, Guid.NewGuid()));

        _repositorioMembresias
            .Setup(r => r.ObtenerContextoActivoAsync(_veterinarioId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new InfoContextoMembresia(Guid.NewGuid(), _veterinariaId, ["VETERINARIO"], ["clinica.ejercer_veterinario"]));
    }

    private CrearConsulta CrearCasoDeUso() => new(
        _contexto.Object, _repositorioConsultas.Object, _repositorioMascotasVeterinarias.Object, _repositorioCitas.Object, _repositorioMembresias.Object);

    private CrearConsultaComando ComandoValido(Guid? citaId = null) => new(
        _mascotaVeterinariaId, citaId, _veterinarioId, FechaHora, "Chequeo general",
        null, null, null, null, null, null, null);

    [Fact]
    public async Task EjecutarAsync_SinCitaPrevia_CreaLaConsultaEnBorrador()
    {
        var casoDeUso = CrearCasoDeUso();

        var resultado = await casoDeUso.EjecutarAsync(ComandoValido(), CancellationToken.None);

        Assert.Equal(Domain.Consultas.EstadoConsulta.Borrador, resultado.Estado);
        _repositorioConsultas.Verify(r => r.AgregarAsync(It.IsAny<Domain.Consultas.ConsultaVeterinaria>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task EjecutarAsync_ConCitaValidaDeLaMismaMascota_CreaLaConsulta()
    {
        var cita = Cita.Crear(_veterinariaId, _mascotaVeterinariaId, null, null, FechaHora, FechaHora.AddMinutes(30), "Motivo", null, Guid.NewGuid());
        _repositorioCitas.Setup(r => r.ObtenerPorIdAsync(cita.Id, _veterinariaId, It.IsAny<CancellationToken>())).ReturnsAsync(cita);

        var casoDeUso = CrearCasoDeUso();

        var resultado = await casoDeUso.EjecutarAsync(ComandoValido(cita.Id), CancellationToken.None);

        Assert.Equal(Domain.Consultas.EstadoConsulta.Borrador, resultado.Estado);
    }

    [Fact]
    public async Task EjecutarAsync_ConCitaDeOtraMascota_LanzaExcepcionAplicacion()
    {
        var cita = Cita.Crear(_veterinariaId, Guid.NewGuid(), null, null, FechaHora, FechaHora.AddMinutes(30), "Motivo", null, Guid.NewGuid());
        _repositorioCitas.Setup(r => r.ObtenerPorIdAsync(cita.Id, _veterinariaId, It.IsAny<CancellationToken>())).ReturnsAsync(cita);

        var casoDeUso = CrearCasoDeUso();

        var excepcion = await Assert.ThrowsAsync<ExcepcionAplicacion>(() => casoDeUso.EjecutarAsync(ComandoValido(cita.Id), CancellationToken.None));

        Assert.Equal(CodigosError.ConsultaCitaInvalida, excepcion.Codigo);
    }

    [Fact]
    public async Task EjecutarAsync_ConCitaCancelada_LanzaExcepcionAplicacion()
    {
        var cita = Cita.Crear(_veterinariaId, _mascotaVeterinariaId, null, null, FechaHora, FechaHora.AddMinutes(30), "Motivo", null, Guid.NewGuid());
        cita.Cancelar(Guid.NewGuid(), DateTimeOffset.UtcNow);
        _repositorioCitas.Setup(r => r.ObtenerPorIdAsync(cita.Id, _veterinariaId, It.IsAny<CancellationToken>())).ReturnsAsync(cita);

        var casoDeUso = CrearCasoDeUso();

        var excepcion = await Assert.ThrowsAsync<ExcepcionAplicacion>(() => casoDeUso.EjecutarAsync(ComandoValido(cita.Id), CancellationToken.None));

        Assert.Equal(CodigosError.ConsultaCitaInvalida, excepcion.Codigo);
    }

    [Fact]
    public async Task EjecutarAsync_ConVeterinarioInvalido_LanzaExcepcionAplicacion()
    {
        _repositorioMembresias
            .Setup(r => r.ObtenerContextoActivoAsync(_veterinarioId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((InfoContextoMembresia?)null);

        var casoDeUso = CrearCasoDeUso();

        var excepcion = await Assert.ThrowsAsync<ExcepcionAplicacion>(() => casoDeUso.EjecutarAsync(ComandoValido(), CancellationToken.None));

        Assert.Equal(CodigosError.ConsultaVeterinarioInvalido, excepcion.Codigo);
    }
}
