using SaaS.Veterinario.Domain.Errores;
using SaaS.Veterinario.Domain.Roles;

namespace SaaS.Veterinario.Domain.Tests.Roles;

public class RolTests
{
    [Fact]
    public void Crear_ConDatosValidos_NormalizaCodigoYQuedaActivo()
    {
        var rol = Rol.Crear("  administrador_veterinaria  ", "Administrador de veterinaria");

        Assert.Equal("ADMINISTRADOR_VETERINARIA", rol.Codigo);
        Assert.Equal(EstadoRol.Activo, rol.Estado);
        Assert.Empty(rol.Permisos);
    }

    [Fact]
    public void Crear_SinCodigo_LanzaExcepcionDominio()
    {
        Assert.Throws<ExcepcionDominio>(() => Rol.Crear("   ", "Veterinario"));
    }

    [Fact]
    public void AsignarPermiso_ConPermisosDistintos_PermiteMultiplesPermisos()
    {
        var rol = Rol.Crear("VETERINARIO", "Veterinario");
        var permisoVer = Guid.NewGuid();
        var permisoEditar = Guid.NewGuid();

        rol.AsignarPermiso(permisoVer);
        rol.AsignarPermiso(permisoEditar);

        Assert.Equal(2, rol.Permisos.Count);
    }

    [Fact]
    public void AsignarPermiso_ConPermisoYaAsignado_LanzaExcepcionDominio()
    {
        var rol = Rol.Crear("VETERINARIO", "Veterinario");
        var permisoId = Guid.NewGuid();
        rol.AsignarPermiso(permisoId);

        Assert.Throws<ExcepcionDominio>(() => rol.AsignarPermiso(permisoId));
    }

    [Fact]
    public void Desactivar_CambiaEstadoAInactivo()
    {
        var rol = Rol.Crear("ASISTENTE", "Asistente");

        rol.Desactivar();

        Assert.Equal(EstadoRol.Inactivo, rol.Estado);
    }
}
