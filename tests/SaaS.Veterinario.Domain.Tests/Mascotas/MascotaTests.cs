using SaaS.Veterinario.Domain.Errores;
using SaaS.Veterinario.Domain.Mascotas;

namespace SaaS.Veterinario.Domain.Tests.Mascotas;

public class MascotaTests
{
    private static Mascota CrearMascotaValida()
        => Mascota.Crear(
            codigoPublico: "PET-7K29QF",
            nombre: "Toby",
            especieId: Guid.NewGuid(),
            razaId: null,
            sexo: SexoMascota.Macho,
            fechaNacimiento: new DateOnly(2020, 1, 1),
            fechaNacimientoAproximada: false,
            color: "Café",
            numeroMicrochip: null,
            esterilizado: null);

    [Fact]
    public void Crear_ConDatosValidos_InicializaEnActiva()
    {
        var mascota = CrearMascotaValida();

        Assert.Equal("PET-7K29QF", mascota.CodigoPublico.Valor);
        Assert.Equal("Toby", mascota.Nombre);
        Assert.Equal(EstadoMascota.Activa, mascota.Estado);
        Assert.Null(mascota.FechaEliminacion);
    }

    [Fact]
    public void Crear_SinNombre_LanzaExcepcionDominio()
    {
        Assert.Throws<ExcepcionDominio>(() => Mascota.Crear(
            "PET-7K29QF", "   ", Guid.NewGuid(), null, SexoMascota.Macho, null, false, null, null, null));
    }

    [Fact]
    public void Crear_SinEspecie_LanzaExcepcionDominio()
    {
        Assert.Throws<ExcepcionDominio>(() => Mascota.Crear(
            "PET-7K29QF", "Toby", Guid.Empty, null, SexoMascota.Macho, null, false, null, null, null));
    }

    [Fact]
    public void Crear_ConFechaNacimientoEnElFuturo_LanzaExcepcionDominio()
    {
        var manana = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1));

        Assert.Throws<ExcepcionDominio>(() => Mascota.Crear(
            "PET-7K29QF", "Toby", Guid.NewGuid(), null, SexoMascota.Macho, manana, false, null, null, null));
    }

    [Fact]
    public void Crear_ConCodigoPublicoDePrefijoInvalido_LanzaExcepcionDominio()
    {
        Assert.Throws<ExcepcionDominio>(() => Mascota.Crear(
            "VET-7K29QF", "Toby", Guid.NewGuid(), null, SexoMascota.Macho, null, false, null, null, null));
    }

    [Fact]
    public void Crear_ConMicrochipVacio_LoNormalizaANull()
    {
        var mascota = Mascota.Crear(
            "PET-7K29QF", "Toby", Guid.NewGuid(), null, SexoMascota.Macho, null, false, null, "   ", null);

        Assert.Null(mascota.NumeroMicrochip);
    }

    [Fact]
    public void Desactivar_DesdeActiva_CambiaEstadoAInactiva()
    {
        var mascota = CrearMascotaValida();
        var momento = DateTimeOffset.UtcNow;

        mascota.Desactivar(momento);

        Assert.Equal(EstadoMascota.Inactiva, mascota.Estado);
        Assert.Equal(momento, mascota.FechaActualizacion);
    }

    [Fact]
    public void MarcarFallecida_DesdeActiva_CambiaEstadoAFallecida()
    {
        var mascota = CrearMascotaValida();

        mascota.MarcarFallecida(DateTimeOffset.UtcNow);

        Assert.Equal(EstadoMascota.Fallecida, mascota.Estado);
    }

    [Fact]
    public void MarcarFallecida_DesdeInactiva_CambiaEstadoAFallecida()
    {
        var mascota = CrearMascotaValida();
        mascota.Desactivar(DateTimeOffset.UtcNow);

        mascota.MarcarFallecida(DateTimeOffset.UtcNow);

        Assert.Equal(EstadoMascota.Fallecida, mascota.Estado);
    }

    [Fact]
    public void Activar_DesdeFallecida_LanzaExcepcionDominio()
    {
        var mascota = CrearMascotaValida();
        mascota.MarcarFallecida(DateTimeOffset.UtcNow);

        Assert.Throws<ExcepcionDominio>(() => mascota.Activar(DateTimeOffset.UtcNow));
    }
}
