using Moq;
using SaaS.Veterinario.Application.Abstracciones;
using SaaS.Veterinario.Application.Citas;
using SaaS.Veterinario.Application.Errores;
using SaaS.Veterinario.Domain.Clientes;
using SaaS.Veterinario.Domain.Mascotas;

namespace SaaS.Veterinario.Application.Tests.Citas;

public class CrearCitaTests
{
    private readonly Mock<IContextoVeterinaria> _contexto = new();
    private readonly Mock<IRepositorioCitas> _repositorioCitas = new();
    private readonly Mock<IRepositorioMascotasVeterinarias> _repositorioMascotasVeterinarias = new();
    private readonly Mock<IRepositorioClientes> _repositorioClientes = new();
    private readonly Mock<IRepositorioMembresias> _repositorioMembresias = new();

    private readonly Guid _veterinariaId = Guid.NewGuid();
    private readonly Guid _usuarioId = Guid.NewGuid();
    private readonly Guid _mascotaVeterinariaId = Guid.NewGuid();

    private static readonly DateTimeOffset Inicio = new(2026, 1, 10, 9, 0, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset Fin = Inicio.AddMinutes(30);

    public CrearCitaTests()
    {
        _contexto.SetupGet(c => c.VeterinariaId).Returns(_veterinariaId);
        _contexto.SetupGet(c => c.UsuarioId).Returns(_usuarioId);

        _repositorioMascotasVeterinarias
            .Setup(r => r.ObtenerPorIdAsync(_mascotaVeterinariaId, _veterinariaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(MascotaVeterinaria.Crear(_veterinariaId, Guid.NewGuid()));
    }

    private CrearCita CrearCasoDeUso() => new(
        _contexto.Object, _repositorioCitas.Object, _repositorioMascotasVeterinarias.Object, _repositorioClientes.Object, _repositorioMembresias.Object);

    private CrearCitaComando ComandoValido() => new(_mascotaVeterinariaId, null, null, Inicio, Fin, "Control anual", null);

    [Fact]
    public async Task EjecutarAsync_ConDatosValidos_CreaLaCitaProgramada()
    {
        var casoDeUso = CrearCasoDeUso();

        var resultado = await casoDeUso.EjecutarAsync(ComandoValido(), CancellationToken.None);

        Assert.Equal(Domain.Citas.EstadoCita.Programada, resultado.Estado);
        _repositorioCitas.Verify(r => r.AgregarAsync(It.IsAny<Domain.Citas.Cita>(), It.IsAny<CancellationToken>()), Times.Once);
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
        _repositorioCitas.Verify(r => r.AgregarAsync(It.IsAny<Domain.Citas.Cita>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task EjecutarAsync_ConClienteDeOtraVeterinaria_LanzaExcepcionAplicacion()
    {
        var clienteId = Guid.NewGuid();
        _repositorioClientes
            .Setup(r => r.ObtenerPorIdAsync(clienteId, _veterinariaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ClienteVeterinaria?)null);

        var casoDeUso = CrearCasoDeUso();
        var comando = ComandoValido() with { ClienteVeterinariaId = clienteId };

        var excepcion = await Assert.ThrowsAsync<ExcepcionAplicacion>(() => casoDeUso.EjecutarAsync(comando, CancellationToken.None));

        Assert.Equal(CodigosError.ClienteInvalido, excepcion.Codigo);
    }

    [Fact]
    public async Task EjecutarAsync_ConVeterinarioSinMembresiaActivaEnLaVeterinaria_LanzaExcepcionAplicacion()
    {
        var veterinarioId = Guid.NewGuid();
        _repositorioMembresias
            .Setup(r => r.ObtenerContextoActivoAsync(veterinarioId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((InfoContextoMembresia?)null);

        var casoDeUso = CrearCasoDeUso();
        var comando = ComandoValido() with { VeterinarioUsuarioId = veterinarioId };

        var excepcion = await Assert.ThrowsAsync<ExcepcionAplicacion>(() => casoDeUso.EjecutarAsync(comando, CancellationToken.None));

        Assert.Equal(CodigosError.CitaVeterinarioInvalido, excepcion.Codigo);
    }

    [Fact]
    public async Task EjecutarAsync_ConVeterinarioDeOtraVeterinaria_LanzaExcepcionAplicacion()
    {
        var veterinarioId = Guid.NewGuid();
        _repositorioMembresias
            .Setup(r => r.ObtenerContextoActivoAsync(veterinarioId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new InfoContextoMembresia(Guid.NewGuid(), Guid.NewGuid(), ["VETERINARIO"], ["clinica.ejercer_veterinario"]));

        var casoDeUso = CrearCasoDeUso();
        var comando = ComandoValido() with { VeterinarioUsuarioId = veterinarioId };

        var excepcion = await Assert.ThrowsAsync<ExcepcionAplicacion>(() => casoDeUso.EjecutarAsync(comando, CancellationToken.None));

        Assert.Equal(CodigosError.CitaVeterinarioInvalido, excepcion.Codigo);
    }

    [Fact]
    public async Task EjecutarAsync_ConHorarioSuperpuestoParaElMismoVeterinario_LanzaExcepcionAplicacion()
    {
        var veterinarioId = Guid.NewGuid();
        _repositorioMembresias
            .Setup(r => r.ObtenerContextoActivoAsync(veterinarioId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new InfoContextoMembresia(Guid.NewGuid(), _veterinariaId, ["VETERINARIO"], ["clinica.ejercer_veterinario"]));
        _repositorioCitas
            .Setup(r => r.ExisteSolapamientoAsync(_veterinariaId, veterinarioId, Inicio, Fin, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var casoDeUso = CrearCasoDeUso();
        var comando = ComandoValido() with { VeterinarioUsuarioId = veterinarioId };

        var excepcion = await Assert.ThrowsAsync<ExcepcionAplicacion>(() => casoDeUso.EjecutarAsync(comando, CancellationToken.None));

        Assert.Equal(CodigosError.CitaHorarioNoDisponible, excepcion.Codigo);
    }

    // ---------- AJUSTE 2: permiso dedicado clinica.ejercer_veterinario ----------

    [Fact]
    public async Task EjecutarAsync_ConVeterinarioQueTieneElPermisoDedicado_LoAsigna()
    {
        var veterinarioId = Guid.NewGuid();
        _repositorioMembresias
            .Setup(r => r.ObtenerContextoActivoAsync(veterinarioId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new InfoContextoMembresia(Guid.NewGuid(), _veterinariaId, ["VETERINARIO"], ["clinica.ejercer_veterinario"]));

        var casoDeUso = CrearCasoDeUso();
        var comando = ComandoValido() with { VeterinarioUsuarioId = veterinarioId };

        var resultado = await casoDeUso.EjecutarAsync(comando, CancellationToken.None);

        Assert.NotEqual(Guid.Empty, resultado.CitaId);
    }

    [Fact]
    public async Task EjecutarAsync_ConAdministradorSinRolVeterinario_LanzaExcepcionAplicacion()
    {
        var administradorId = Guid.NewGuid();
        _repositorioMembresias
            .Setup(r => r.ObtenerContextoActivoAsync(administradorId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new InfoContextoMembresia(Guid.NewGuid(), _veterinariaId, ["ADMINISTRADOR_VETERINARIA"], ["citas.crear", "consultas.ver"]));

        var casoDeUso = CrearCasoDeUso();
        var comando = ComandoValido() with { VeterinarioUsuarioId = administradorId };

        var excepcion = await Assert.ThrowsAsync<ExcepcionAplicacion>(() => casoDeUso.EjecutarAsync(comando, CancellationToken.None));

        Assert.Equal(CodigosError.CitaVeterinarioInvalido, excepcion.Codigo);
    }

    [Fact]
    public async Task EjecutarAsync_ConAsistente_LanzaExcepcionAplicacion()
    {
        var asistenteId = Guid.NewGuid();
        _repositorioMembresias
            .Setup(r => r.ObtenerContextoActivoAsync(asistenteId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new InfoContextoMembresia(Guid.NewGuid(), _veterinariaId, ["ASISTENTE"], ["citas.crear", "citas.ver"]));

        var casoDeUso = CrearCasoDeUso();
        var comando = ComandoValido() with { VeterinarioUsuarioId = asistenteId };

        var excepcion = await Assert.ThrowsAsync<ExcepcionAplicacion>(() => casoDeUso.EjecutarAsync(comando, CancellationToken.None));

        Assert.Equal(CodigosError.CitaVeterinarioInvalido, excepcion.Codigo);
    }

    [Fact]
    public async Task EjecutarAsync_ConAdministradorQueTambienEsVeterinario_LoAsigna()
    {
        var administradorVeterinarioId = Guid.NewGuid();
        _repositorioMembresias
            .Setup(r => r.ObtenerContextoActivoAsync(administradorVeterinarioId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new InfoContextoMembresia(
                Guid.NewGuid(), _veterinariaId, ["ADMINISTRADOR_VETERINARIA", "VETERINARIO"], ["citas.crear", "clinica.ejercer_veterinario"]));

        var casoDeUso = CrearCasoDeUso();
        var comando = ComandoValido() with { VeterinarioUsuarioId = administradorVeterinarioId };

        var resultado = await casoDeUso.EjecutarAsync(comando, CancellationToken.None);

        Assert.NotEqual(Guid.Empty, resultado.CitaId);
    }
}
