using SaaS.Veterinario.Domain.Errores;
using SaaS.Veterinario.Domain.Mascotas;

namespace SaaS.Veterinario.Domain.Tests.Mascotas;

public class ResponsableMascotaTests
{
    [Fact]
    public void Crear_ConDatosValidos_QuedaActivoYNoVinculado()
    {
        var responsable = ResponsableMascota.Crear(
            Guid.NewGuid(), Guid.NewGuid(), null, "Ana Perez", "ana@correo.com", null,
            TipoRelacionResponsable.Propietario, esPrincipal: true, puedeVerHistorial: true, puedeGestionarTratamientos: false);

        Assert.Equal(EstadoResponsableMascota.Activo, responsable.Estado);
        Assert.Equal(EstadoVinculacionResponsable.NoVinculado, responsable.EstadoVinculacion);
        Assert.Null(responsable.UsuarioId);
        Assert.Null(responsable.ClienteVeterinariaId);
        Assert.True(responsable.EsPrincipal);
    }

    [Fact]
    public void Crear_SinClienteVeterinariaNiUsuario_EsValido()
    {
        var responsable = ResponsableMascota.Crear(
            Guid.NewGuid(), Guid.NewGuid(), null, "Ana Perez", null, null,
            TipoRelacionResponsable.Familiar, esPrincipal: false, puedeVerHistorial: false, puedeGestionarTratamientos: false);

        Assert.Null(responsable.ClienteVeterinariaId);
        Assert.Null(responsable.UsuarioId);
        Assert.Equal(TipoRelacionResponsable.Familiar, responsable.TipoRelacion);
    }

    [Fact]
    public void Crear_SinNombre_LanzaExcepcionDominio()
    {
        Assert.Throws<ExcepcionDominio>(() => ResponsableMascota.Crear(
            Guid.NewGuid(), Guid.NewGuid(), null, "   ", null, null,
            TipoRelacionResponsable.Propietario, false, false, false));
    }

    [Fact]
    public void Crear_ConMascotaVeterinariaIdVacio_LanzaExcepcionDominio()
    {
        Assert.Throws<ExcepcionDominio>(() => ResponsableMascota.Crear(
            Guid.NewGuid(), Guid.Empty, null, "Ana Perez", null, null,
            TipoRelacionResponsable.Propietario, false, false, false));
    }
}
