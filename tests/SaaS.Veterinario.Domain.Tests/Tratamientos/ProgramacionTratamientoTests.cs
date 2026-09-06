using SaaS.Veterinario.Domain.Errores;
using SaaS.Veterinario.Domain.Tratamientos;

namespace SaaS.Veterinario.Domain.Tests.Tratamientos;

public class ProgramacionTratamientoTests
{
    private static ProgramacionTratamiento CrearProgramacionValida()
        => ProgramacionTratamiento.Crear(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), DateTimeOffset.UtcNow.AddHours(1));

    [Fact]
    public void Crear_ConDatosValidos_InicializaEnPendiente()
    {
        var programacion = CrearProgramacionValida();

        Assert.Equal(EstadoProgramacionTratamiento.Pendiente, programacion.Estado);
    }

    [Fact]
    public void MarcarRealizada_DesdePendiente_RegistraDatos()
    {
        var programacion = CrearProgramacionValida();
        var usuarioId = Guid.NewGuid();
        var momento = DateTimeOffset.UtcNow;

        programacion.MarcarRealizada(usuarioId, "Toleró bien la dosis", momento);

        Assert.Equal(EstadoProgramacionTratamiento.Realizada, programacion.Estado);
        Assert.Equal(momento, programacion.FechaRealizacion);
        Assert.Equal(usuarioId, programacion.RegistradoPorUsuarioId);
        Assert.Equal("Toleró bien la dosis", programacion.ObservacionRealizacion);
    }

    [Fact]
    public void MarcarOmitida_DesdePendiente_CambiaEstado()
    {
        var programacion = CrearProgramacionValida();

        programacion.MarcarOmitida(Guid.NewGuid(), "Mascota vomitó", DateTimeOffset.UtcNow);

        Assert.Equal(EstadoProgramacionTratamiento.Omitida, programacion.Estado);
    }

    [Fact]
    public void MarcarVencida_DesdePendiente_CambiaEstado()
    {
        var programacion = CrearProgramacionValida();

        programacion.MarcarVencida(DateTimeOffset.UtcNow);

        Assert.Equal(EstadoProgramacionTratamiento.Vencida, programacion.Estado);
    }

    [Fact]
    public void Cancelar_DesdePendiente_CambiaEstado()
    {
        var programacion = CrearProgramacionValida();

        programacion.Cancelar(DateTimeOffset.UtcNow);

        Assert.Equal(EstadoProgramacionTratamiento.Cancelada, programacion.Estado);
    }

    [Fact]
    public void MarcarRealizada_DesdeRealizada_LanzaExcepcionDominio()
    {
        var programacion = CrearProgramacionValida();
        programacion.MarcarRealizada(Guid.NewGuid(), null, DateTimeOffset.UtcNow);

        Assert.Throws<ExcepcionDominio>(() => programacion.MarcarRealizada(Guid.NewGuid(), null, DateTimeOffset.UtcNow));
    }

    [Fact]
    public void MarcarRealizada_DesdeCancelada_LanzaExcepcionDominio()
    {
        var programacion = CrearProgramacionValida();
        programacion.Cancelar(DateTimeOffset.UtcNow);

        Assert.Throws<ExcepcionDominio>(() => programacion.MarcarRealizada(Guid.NewGuid(), null, DateTimeOffset.UtcNow));
    }
}
