using SaaS.Veterinario.Domain.Errores;
using SaaS.Veterinario.Domain.Permisos;

namespace SaaS.Veterinario.Domain.Tests.Permisos;

public class PermisoTests
{
    [Fact]
    public void Crear_ConDatosValidos_NormalizaCodigo()
    {
        var permiso = Permiso.Crear("  PERSONAL.INVITAR  ", "Invitar personal");

        Assert.Equal("personal.invitar", permiso.Codigo);
        Assert.Equal("Invitar personal", permiso.Nombre);
    }

    [Fact]
    public void Crear_SinCodigo_LanzaExcepcionDominio()
    {
        Assert.Throws<ExcepcionDominio>(() => Permiso.Crear("   ", "Invitar personal"));
    }
}
