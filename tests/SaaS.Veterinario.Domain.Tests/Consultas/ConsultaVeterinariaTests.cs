using SaaS.Veterinario.Domain.Consultas;
using SaaS.Veterinario.Domain.Errores;

namespace SaaS.Veterinario.Domain.Tests.Consultas;

public class ConsultaVeterinariaTests
{
    private static readonly DateTimeOffset FechaHora = new(2026, 1, 10, 9, 0, 0, TimeSpan.Zero);

    private static ConsultaVeterinaria CrearConsultaValida(Guid? citaId = null)
        => ConsultaVeterinaria.Crear(
            Guid.NewGuid(), Guid.NewGuid(), citaId, Guid.NewGuid(), FechaHora, "Chequeo general",
            peso: 12.5m, temperatura: 38.2m, frecuenciaCardiaca: 90,
            observacionesClinicas: "Nota interna", indicacionesPropietario: "Dar de comer",
            proximaFechaControl: null, motivoProximoControl: null, creadoPorUsuarioId: Guid.NewGuid());

    [Fact]
    public void Crear_ConDatosValidos_InicializaEnBorrador()
    {
        var consulta = CrearConsultaValida();

        Assert.Equal(EstadoConsulta.Borrador, consulta.Estado);
        Assert.Null(consulta.CitaId);
    }

    [Fact]
    public void Crear_ConCitaId_LaAsocia()
    {
        var citaId = Guid.NewGuid();
        var consulta = CrearConsultaValida(citaId);

        Assert.Equal(citaId, consulta.CitaId);
    }

    [Fact]
    public void Crear_SinVeterinario_LanzaExcepcionDominio()
    {
        Assert.Throws<ExcepcionDominio>(() => ConsultaVeterinaria.Crear(
            Guid.NewGuid(), Guid.NewGuid(), null, Guid.Empty, FechaHora, "Motivo",
            null, null, null, null, null, null, null, Guid.NewGuid()));
    }

    [Fact]
    public void Crear_SinMotivo_LanzaExcepcionDominio()
    {
        Assert.Throws<ExcepcionDominio>(() => ConsultaVeterinaria.Crear(
            Guid.NewGuid(), Guid.NewGuid(), null, Guid.NewGuid(), FechaHora, "   ",
            null, null, null, null, null, null, null, Guid.NewGuid()));
    }

    [Fact]
    public void Crear_ConPesoNegativo_LanzaExcepcionDominio()
    {
        Assert.Throws<ExcepcionDominio>(() => ConsultaVeterinaria.Crear(
            Guid.NewGuid(), Guid.NewGuid(), null, Guid.NewGuid(), FechaHora, "Motivo",
            peso: -1m, temperatura: null, frecuenciaCardiaca: null,
            observacionesClinicas: null, indicacionesPropietario: null,
            proximaFechaControl: null, motivoProximoControl: null, creadoPorUsuarioId: Guid.NewGuid()));
    }

    [Fact]
    public void Crear_ConProximoControlAnteriorALaConsulta_LanzaExcepcionDominio()
    {
        Assert.Throws<ExcepcionDominio>(() => ConsultaVeterinaria.Crear(
            Guid.NewGuid(), Guid.NewGuid(), null, Guid.NewGuid(), FechaHora, "Motivo",
            null, null, null, null, null,
            proximaFechaControl: FechaHora.AddDays(-1), motivoProximoControl: null, creadoPorUsuarioId: Guid.NewGuid()));
    }

    [Fact]
    public void ActualizarBorrador_EnBorrador_ModificaDatosYAuditoria()
    {
        var consulta = CrearConsultaValida();
        var usuarioId = Guid.NewGuid();
        var momento = DateTimeOffset.UtcNow;

        consulta.ActualizarBorrador(
            consulta.VeterinarioUsuarioId, FechaHora, "Motivo actualizado",
            13m, 38.5m, 95, "Nueva nota interna", "Nueva indicación",
            null, null, usuarioId, momento);

        Assert.Equal("Motivo actualizado", consulta.MotivoConsulta);
        Assert.Equal(usuarioId, consulta.ActualizadoPorUsuarioId);
        Assert.Equal(momento, consulta.FechaActualizacion);
    }

    [Fact]
    public void Finalizar_DesdeBorrador_EstableceFechaYUsuarioDeFinalizacion()
    {
        var consulta = CrearConsultaValida();
        var usuarioId = Guid.NewGuid();
        var momento = DateTimeOffset.UtcNow;

        consulta.Finalizar(usuarioId, momento);

        Assert.Equal(EstadoConsulta.Finalizada, consulta.Estado);
        Assert.Equal(momento, consulta.FechaFinalizacion);
        Assert.Equal(usuarioId, consulta.FinalizadoPorUsuarioId);
    }

    [Fact]
    public void ActualizarBorrador_DespuesDeFinalizada_LanzaExcepcionDominio()
    {
        var consulta = CrearConsultaValida();
        consulta.Finalizar(Guid.NewGuid(), DateTimeOffset.UtcNow);

        Assert.Throws<ExcepcionDominio>(() => consulta.ActualizarBorrador(
            consulta.VeterinarioUsuarioId, FechaHora, "Otro motivo", null, null, null, null, null, null, null,
            Guid.NewGuid(), DateTimeOffset.UtcNow));
    }

    [Fact]
    public void Finalizar_DesdeAnulada_LanzaExcepcionDominio()
    {
        var consulta = CrearConsultaValida();
        consulta.Anular(Guid.NewGuid(), "Motivo de anulación", DateTimeOffset.UtcNow);

        Assert.Throws<ExcepcionDominio>(() => consulta.Finalizar(Guid.NewGuid(), DateTimeOffset.UtcNow));
    }

    [Fact]
    public void Anular_DesdeBorrador_RegistraMotivoYUsuario()
    {
        var consulta = CrearConsultaValida();
        var usuarioId = Guid.NewGuid();

        consulta.Anular(usuarioId, "Registro duplicado", DateTimeOffset.UtcNow);

        Assert.Equal(EstadoConsulta.Anulada, consulta.Estado);
        Assert.Equal("Registro duplicado", consulta.MotivoAnulacion);
        Assert.Equal(usuarioId, consulta.AnuladoPorUsuarioId);
    }

    [Fact]
    public void Anular_DesdeFinalizada_EsPermitido()
    {
        var consulta = CrearConsultaValida();
        consulta.Finalizar(Guid.NewGuid(), DateTimeOffset.UtcNow);

        consulta.Anular(Guid.NewGuid(), "Error clínico detectado", DateTimeOffset.UtcNow);

        Assert.Equal(EstadoConsulta.Anulada, consulta.Estado);
    }

    [Fact]
    public void Anular_DesdeAnulada_LanzaExcepcionDominio()
    {
        var consulta = CrearConsultaValida();
        consulta.Anular(Guid.NewGuid(), "Motivo", DateTimeOffset.UtcNow);

        Assert.Throws<ExcepcionDominio>(() => consulta.Anular(Guid.NewGuid(), "Otro motivo", DateTimeOffset.UtcNow));
    }

    [Fact]
    public void Anular_SinMotivo_LanzaExcepcionDominio()
    {
        var consulta = CrearConsultaValida();

        Assert.Throws<ExcepcionDominio>(() => consulta.Anular(Guid.NewGuid(), "   ", DateTimeOffset.UtcNow));
    }
}
