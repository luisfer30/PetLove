using SaaS.Veterinario.Domain.Errores;
using SaaS.Veterinario.Domain.Veterinarias;

namespace SaaS.Veterinario.Domain.Tests.Veterinarias;

public class VeterinariaTests
{
    private static Veterinaria CrearVeterinariaValida()
        => Veterinaria.Crear(
            codigoPublico: "VET-8K2L9Q",
            nombreComercial: "Clinica Central",
            correo: "contacto@clinicacentral.com",
            pais: "Ecuador",
            zonaHoraria: "America/Guayaquil");

    [Fact]
    public void Crear_ConDatosValidos_InicializaEnPendienteActivacion()
    {
        var veterinaria = CrearVeterinariaValida();

        Assert.Equal("VET-8K2L9Q", veterinaria.CodigoPublico.Valor);
        Assert.Equal("Clinica Central", veterinaria.NombreComercial);
        Assert.Equal("contacto@clinicacentral.com", veterinaria.Correo.Valor);
        Assert.Equal(EstadoVeterinaria.PendienteActivacion, veterinaria.Estado);
        Assert.Null(veterinaria.FechaEliminacion);
    }

    [Fact]
    public void Crear_SinNombreComercial_LanzaExcepcionDominio()
    {
        Assert.Throws<ExcepcionDominio>(() => Veterinaria.Crear(
            codigoPublico: "VET-8K2L9Q",
            nombreComercial: "   ",
            correo: "contacto@clinicacentral.com",
            pais: "Ecuador",
            zonaHoraria: "America/Guayaquil"));
    }

    [Fact]
    public void Crear_ConCodigoPublicoConPrefijoInvalido_LanzaExcepcionDominio()
    {
        Assert.Throws<ExcepcionDominio>(() => Veterinaria.Crear(
            codigoPublico: "PET-8K2L9Q",
            nombreComercial: "Clinica Central",
            correo: "contacto@clinicacentral.com",
            pais: "Ecuador",
            zonaHoraria: "America/Guayaquil"));
    }

    [Fact]
    public void Activar_DesdePendienteActivacion_CambiaEstadoAActiva()
    {
        var veterinaria = CrearVeterinariaValida();
        var momento = DateTimeOffset.UtcNow;

        veterinaria.Activar(momento);

        Assert.Equal(EstadoVeterinaria.Activa, veterinaria.Estado);
        Assert.Equal(momento, veterinaria.FechaActualizacion);
    }

    [Fact]
    public void Suspender_DesdePendienteActivacion_LanzaExcepcionDominio()
    {
        var veterinaria = CrearVeterinariaValida();

        Assert.Throws<ExcepcionDominio>(() => veterinaria.Suspender(DateTimeOffset.UtcNow));
    }

    [Fact]
    public void Desactivar_DesdeActiva_CambiaEstadoAInactiva()
    {
        var veterinaria = CrearVeterinariaValida();
        veterinaria.Activar(DateTimeOffset.UtcNow);

        veterinaria.Desactivar(DateTimeOffset.UtcNow);

        Assert.Equal(EstadoVeterinaria.Inactiva, veterinaria.Estado);
    }

    [Fact]
    public void Activar_DesdeInactiva_LanzaExcepcionDominio()
    {
        var veterinaria = CrearVeterinariaValida();
        veterinaria.Activar(DateTimeOffset.UtcNow);
        veterinaria.Desactivar(DateTimeOffset.UtcNow);

        Assert.Throws<ExcepcionDominio>(() => veterinaria.Activar(DateTimeOffset.UtcNow));
    }

    [Fact]
    public void ActualizarInformacion_ConDatosValidos_ActualizaCamposYNoTocaCorreoNiCodigoPublico()
    {
        var veterinaria = CrearVeterinariaValida();
        var momento = DateTimeOffset.UtcNow;

        veterinaria.ActualizarInformacion(
            nombreComercial: "Clinica Central Renovada",
            razonSocial: "Clinica Central S.A.",
            ruc: "1234567890001",
            telefono: "0999999999",
            direccion: "Av. Siempre Viva 123",
            ciudad: "Quito",
            pais: "Ecuador",
            zonaHoraria: "America/Guayaquil",
            momento: momento);

        Assert.Equal("Clinica Central Renovada", veterinaria.NombreComercial);
        Assert.Equal("Clinica Central S.A.", veterinaria.RazonSocial);
        Assert.Equal("Quito", veterinaria.Ciudad);
        Assert.Equal(momento, veterinaria.FechaActualizacion);
        Assert.Equal("contacto@clinicacentral.com", veterinaria.Correo.Valor);
        Assert.Equal("VET-8K2L9Q", veterinaria.CodigoPublico.Valor);
    }

    [Fact]
    public void ActualizarInformacion_SinNombreComercial_LanzaExcepcionDominio()
    {
        var veterinaria = CrearVeterinariaValida();

        Assert.Throws<ExcepcionDominio>(() => veterinaria.ActualizarInformacion(
            nombreComercial: "   ",
            razonSocial: null,
            ruc: null,
            telefono: null,
            direccion: null,
            ciudad: null,
            pais: "Ecuador",
            zonaHoraria: "America/Guayaquil",
            momento: DateTimeOffset.UtcNow));
    }
}
