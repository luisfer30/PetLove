using SaaS.Veterinario.Domain.Errores;
using SaaS.Veterinario.Domain.Seguimientos;

namespace SaaS.Veterinario.Domain.Tests.Seguimientos;

public class SeguimientoClinicoTests
{
    private static readonly DateTimeOffset FechaObjetivo = new(2026, 2, 1, 9, 0, 0, TimeSpan.Zero);

    private static SeguimientoClinico CrearSeguimientoValido()
        => SeguimientoClinico.Crear(
            Guid.NewGuid(), Guid.NewGuid(), null, null, TipoSeguimientoClinico.Control, FechaObjetivo, "Control post-tratamiento", null, Guid.NewGuid());

    [Fact]
    public void Crear_ConDatosValidos_InicializaEnPendiente()
    {
        var seguimiento = CrearSeguimientoValido();

        Assert.Equal(EstadoSeguimientoClinico.Pendiente, seguimiento.Estado);
        Assert.False(seguimiento.EsGeneradoDesdeProximoControl);
    }

    [Fact]
    public void Crear_SinMotivo_LanzaExcepcionDominio()
    {
        Assert.Throws<ExcepcionDominio>(() => SeguimientoClinico.Crear(
            Guid.NewGuid(), Guid.NewGuid(), null, null, TipoSeguimientoClinico.Control, FechaObjetivo, "   ", null, Guid.NewGuid()));
    }

    [Fact]
    public void MarcarAgendado_DesdePendiente_RegistraCitaYFecha()
    {
        var seguimiento = CrearSeguimientoValido();
        var citaId = Guid.NewGuid();
        var usuarioId = Guid.NewGuid();
        var fechaAgendada = FechaObjetivo.AddDays(-1);

        seguimiento.MarcarAgendado(citaId, fechaAgendada, usuarioId, DateTimeOffset.UtcNow);

        Assert.Equal(EstadoSeguimientoClinico.Agendado, seguimiento.Estado);
        Assert.Equal(citaId, seguimiento.CitaId);
        Assert.Equal(fechaAgendada, seguimiento.FechaAgendada);
    }

    [Fact]
    public void MarcarRealizado_DesdeAgendado_CambiaEstado()
    {
        var seguimiento = CrearSeguimientoValido();
        seguimiento.MarcarAgendado(Guid.NewGuid(), FechaObjetivo, Guid.NewGuid(), DateTimeOffset.UtcNow);

        seguimiento.MarcarRealizado(Guid.NewGuid(), DateTimeOffset.UtcNow);

        Assert.Equal(EstadoSeguimientoClinico.Realizado, seguimiento.Estado);
    }

    [Fact]
    public void MarcarVencido_DesdePendiente_CambiaEstado()
    {
        var seguimiento = CrearSeguimientoValido();

        seguimiento.MarcarVencido(DateTimeOffset.UtcNow);

        Assert.Equal(EstadoSeguimientoClinico.Vencido, seguimiento.Estado);
    }

    [Fact]
    public void Cancelar_EsTerminal()
    {
        var seguimiento = CrearSeguimientoValido();
        seguimiento.Cancelar(Guid.NewGuid(), DateTimeOffset.UtcNow);

        Assert.Throws<ExcepcionDominio>(() => seguimiento.MarcarRealizado(Guid.NewGuid(), DateTimeOffset.UtcNow));
    }
}
