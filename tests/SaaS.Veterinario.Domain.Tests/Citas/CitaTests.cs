using SaaS.Veterinario.Domain.Citas;
using SaaS.Veterinario.Domain.Errores;

namespace SaaS.Veterinario.Domain.Tests.Citas;

public class CitaTests
{
    private static readonly DateTimeOffset Inicio = new(2026, 1, 10, 9, 0, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset Fin = Inicio.AddMinutes(30);

    private static Cita CrearCitaValida()
        => Cita.Crear(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Inicio, Fin, "Control anual", null, Guid.NewGuid());

    [Fact]
    public void Crear_ConDatosValidos_InicializaEnProgramada()
    {
        var cita = CrearCitaValida();

        Assert.Equal(EstadoCita.Programada, cita.Estado);
        Assert.Equal("Control anual", cita.Motivo);
    }

    [Fact]
    public void Crear_ConFinAntesDeInicio_LanzaExcepcionDominio()
    {
        Assert.Throws<ExcepcionDominio>(() => Cita.Crear(
            Guid.NewGuid(), Guid.NewGuid(), null, null, Inicio, Inicio.AddMinutes(-1), "Motivo", null, Guid.NewGuid()));
    }

    [Fact]
    public void Crear_ConFinIgualAInicio_LanzaExcepcionDominio()
    {
        Assert.Throws<ExcepcionDominio>(() => Cita.Crear(
            Guid.NewGuid(), Guid.NewGuid(), null, null, Inicio, Inicio, "Motivo", null, Guid.NewGuid()));
    }

    [Fact]
    public void Crear_SinMotivo_LanzaExcepcionDominio()
    {
        Assert.Throws<ExcepcionDominio>(() => Cita.Crear(
            Guid.NewGuid(), Guid.NewGuid(), null, null, Inicio, Fin, "   ", null, Guid.NewGuid()));
    }

    [Fact]
    public void Crear_SinCliente_EsValida()
    {
        var cita = Cita.Crear(Guid.NewGuid(), Guid.NewGuid(), null, null, Inicio, Fin, "Motivo", null, Guid.NewGuid());

        Assert.Null(cita.ClienteVeterinariaId);
        Assert.Null(cita.VeterinarioUsuarioId);
    }

    [Fact]
    public void Confirmar_DesdeProgramada_CambiaAConfirmada()
    {
        var cita = CrearCitaValida();
        var usuarioId = Guid.NewGuid();
        var momento = DateTimeOffset.UtcNow;

        cita.Confirmar(usuarioId, momento);

        Assert.Equal(EstadoCita.Confirmada, cita.Estado);
        Assert.Equal(usuarioId, cita.ActualizadoPorUsuarioId);
        Assert.Equal(momento, cita.FechaActualizacion);
    }

    [Fact]
    public void MarcarEnEspera_DesdeProgramada_CambiaAEnEspera()
    {
        var cita = CrearCitaValida();

        cita.MarcarEnEspera(Guid.NewGuid(), DateTimeOffset.UtcNow);

        Assert.Equal(EstadoCita.EnEspera, cita.Estado);
    }

    [Fact]
    public void MarcarAtendida_DesdeEnEspera_CambiaAAtendida()
    {
        var cita = CrearCitaValida();
        cita.MarcarEnEspera(Guid.NewGuid(), DateTimeOffset.UtcNow);

        cita.MarcarAtendida(Guid.NewGuid(), DateTimeOffset.UtcNow);

        Assert.Equal(EstadoCita.Atendida, cita.Estado);
    }

    // Ajuste post-Etapa 7 (seccion "AJUSTE 1"): en el flujo real de la veterinaria, una cita
    // puede atenderse directamente sin pasar explicitamente por CONFIRMADA/EN_ESPERA.
    [Fact]
    public void MarcarAtendida_DesdeProgramada_CambiaAAtendida()
    {
        var cita = CrearCitaValida();

        cita.MarcarAtendida(Guid.NewGuid(), DateTimeOffset.UtcNow);

        Assert.Equal(EstadoCita.Atendida, cita.Estado);
    }

    [Fact]
    public void MarcarAtendida_DesdeConfirmada_CambiaAAtendida()
    {
        var cita = CrearCitaValida();
        cita.Confirmar(Guid.NewGuid(), DateTimeOffset.UtcNow);

        cita.MarcarAtendida(Guid.NewGuid(), DateTimeOffset.UtcNow);

        Assert.Equal(EstadoCita.Atendida, cita.Estado);
    }

    [Fact]
    public void Cancelar_DesdeProgramada_CambiaACancelada()
    {
        var cita = CrearCitaValida();

        cita.Cancelar(Guid.NewGuid(), DateTimeOffset.UtcNow);

        Assert.Equal(EstadoCita.Cancelada, cita.Estado);
    }

    [Fact]
    public void MarcarAtendida_DesdeCancelada_LanzaExcepcionDominio()
    {
        var cita = CrearCitaValida();
        cita.Cancelar(Guid.NewGuid(), DateTimeOffset.UtcNow);

        Assert.Throws<ExcepcionDominio>(() => cita.MarcarAtendida(Guid.NewGuid(), DateTimeOffset.UtcNow));
    }

    [Fact]
    public void MarcarAtendida_DesdeNoAsistio_LanzaExcepcionDominio()
    {
        var cita = CrearCitaValida();
        cita.MarcarNoAsistio(Guid.NewGuid(), DateTimeOffset.UtcNow);

        Assert.Throws<ExcepcionDominio>(() => cita.MarcarAtendida(Guid.NewGuid(), DateTimeOffset.UtcNow));
    }

    [Fact]
    public void MarcarAtendida_DesdeAtendida_LanzaExcepcionDominio()
    {
        var cita = CrearCitaValida();
        cita.MarcarAtendida(Guid.NewGuid(), DateTimeOffset.UtcNow);

        Assert.Throws<ExcepcionDominio>(() => cita.MarcarAtendida(Guid.NewGuid(), DateTimeOffset.UtcNow));
    }

    [Fact]
    public void ActualizarDatos_DesdeAtendida_LanzaExcepcionDominio()
    {
        var cita = CrearCitaValida();
        cita.MarcarEnEspera(Guid.NewGuid(), DateTimeOffset.UtcNow);
        cita.MarcarAtendida(Guid.NewGuid(), DateTimeOffset.UtcNow);

        Assert.Throws<ExcepcionDominio>(() => cita.ActualizarDatos(
            Inicio, Fin, "Otro motivo", null, null, Guid.NewGuid(), DateTimeOffset.UtcNow));
    }
}
