using Moq;
using SaaS.Veterinario.Application.Abstracciones;
using SaaS.Veterinario.Application.Citas;
using SaaS.Veterinario.Application.Errores;
using SaaS.Veterinario.Domain.Citas;

namespace SaaS.Veterinario.Application.Tests.Citas;

public class ActualizarCitaTests
{
    private readonly Mock<IContextoVeterinaria> _contexto = new();
    private readonly Mock<IRepositorioCitas> _repositorioCitas = new();
    private readonly Mock<IRepositorioMembresias> _repositorioMembresias = new();

    private readonly Guid _veterinariaId = Guid.NewGuid();

    private static readonly DateTimeOffset Inicio = new(2026, 1, 10, 9, 0, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset Fin = Inicio.AddMinutes(30);

    public ActualizarCitaTests()
    {
        _contexto.SetupGet(c => c.VeterinariaId).Returns(_veterinariaId);
        _contexto.SetupGet(c => c.UsuarioId).Returns(Guid.NewGuid());
    }

    private ActualizarCita CrearCasoDeUso() => new(_contexto.Object, _repositorioCitas.Object, _repositorioMembresias.Object);

    private static Cita CrearCitaProgramada(Guid veterinariaId)
        => Cita.Crear(veterinariaId, Guid.NewGuid(), null, null, Inicio, Fin, "Motivo original", null, Guid.NewGuid());

    [Fact]
    public async Task EjecutarAsync_ConCitaProgramada_ActualizaLosDatos()
    {
        var cita = CrearCitaProgramada(_veterinariaId);
        _repositorioCitas.Setup(r => r.ObtenerPorIdAsync(cita.Id, _veterinariaId, It.IsAny<CancellationToken>())).ReturnsAsync(cita);

        var casoDeUso = CrearCasoDeUso();
        var comando = new ActualizarCitaComando(cita.Id, Inicio, Fin.AddMinutes(15), "Motivo actualizado", null, null);

        await casoDeUso.EjecutarAsync(comando, CancellationToken.None);

        Assert.Equal("Motivo actualizado", cita.Motivo);
        _repositorioCitas.Verify(r => r.GuardarCambiosAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task EjecutarAsync_ConCitaInexistente_LanzaExcepcionAplicacion()
    {
        _repositorioCitas
            .Setup(r => r.ObtenerPorIdAsync(It.IsAny<Guid>(), _veterinariaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Cita?)null);

        var casoDeUso = CrearCasoDeUso();
        var comando = new ActualizarCitaComando(Guid.NewGuid(), Inicio, Fin, "Motivo", null, null);

        var excepcion = await Assert.ThrowsAsync<ExcepcionAplicacion>(() => casoDeUso.EjecutarAsync(comando, CancellationToken.None));

        Assert.Equal(CodigosError.CitaNoEncontrada, excepcion.Codigo);
    }

    [Fact]
    public async Task EjecutarAsync_ConCitaAtendida_LanzaExcepcionAplicacionYNoModificaNada()
    {
        var cita = CrearCitaProgramada(_veterinariaId);
        cita.MarcarEnEspera(Guid.NewGuid(), DateTimeOffset.UtcNow);
        cita.MarcarAtendida(Guid.NewGuid(), DateTimeOffset.UtcNow);
        _repositorioCitas.Setup(r => r.ObtenerPorIdAsync(cita.Id, _veterinariaId, It.IsAny<CancellationToken>())).ReturnsAsync(cita);

        var casoDeUso = CrearCasoDeUso();
        var comando = new ActualizarCitaComando(cita.Id, Inicio, Fin, "Otro motivo", null, null);

        var excepcion = await Assert.ThrowsAsync<ExcepcionAplicacion>(() => casoDeUso.EjecutarAsync(comando, CancellationToken.None));

        Assert.Equal(CodigosError.CitaEstadoInvalido, excepcion.Codigo);
        Assert.Equal("Motivo original", cita.Motivo);
    }
}
