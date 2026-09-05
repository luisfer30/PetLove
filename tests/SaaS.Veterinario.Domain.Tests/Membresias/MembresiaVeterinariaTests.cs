using SaaS.Veterinario.Domain.Errores;
using SaaS.Veterinario.Domain.Membresias;

namespace SaaS.Veterinario.Domain.Tests.Membresias;

public class MembresiaVeterinariaTests
{
    [Fact]
    public void Crear_InicializaEnInvitada()
    {
        var membresia = MembresiaVeterinaria.Crear(Guid.NewGuid(), Guid.NewGuid());

        Assert.Equal(EstadoMembresiaVeterinaria.Invitada, membresia.Estado);
        Assert.Null(membresia.FechaFinalizacion);
        Assert.Empty(membresia.Roles);
    }

    [Fact]
    public void Crear_ConUsuarioIdVacio_LanzaExcepcionDominio()
    {
        Assert.Throws<ExcepcionDominio>(() => MembresiaVeterinaria.Crear(Guid.Empty, Guid.NewGuid()));
    }

    [Fact]
    public void Activar_DesdeInvitada_CambiaEstadoAActiva()
    {
        var membresia = MembresiaVeterinaria.Crear(Guid.NewGuid(), Guid.NewGuid());

        membresia.Activar(DateTimeOffset.UtcNow);

        Assert.Equal(EstadoMembresiaVeterinaria.Activa, membresia.Estado);
    }

    [Fact]
    public void Finalizar_DesdeActiva_RegistraFechaFinalizacion()
    {
        var membresia = MembresiaVeterinaria.Crear(Guid.NewGuid(), Guid.NewGuid());
        membresia.Activar(DateTimeOffset.UtcNow);

        var momentoFinalizacion = DateTimeOffset.UtcNow;
        membresia.Finalizar(momentoFinalizacion);

        Assert.Equal(EstadoMembresiaVeterinaria.Finalizada, membresia.Estado);
        Assert.Equal(momentoFinalizacion, membresia.FechaFinalizacion);
    }

    [Fact]
    public void Activar_DesdeFinalizada_LanzaExcepcionDominio()
    {
        var membresia = MembresiaVeterinaria.Crear(Guid.NewGuid(), Guid.NewGuid());
        membresia.Activar(DateTimeOffset.UtcNow);
        membresia.Finalizar(DateTimeOffset.UtcNow);

        Assert.Throws<ExcepcionDominio>(() => membresia.Activar(DateTimeOffset.UtcNow));
    }

    [Fact]
    public void Suspender_DesdeActiva_CambiaEstadoASuspendida()
    {
        var membresia = MembresiaVeterinaria.Crear(Guid.NewGuid(), Guid.NewGuid());
        membresia.Activar(DateTimeOffset.UtcNow);

        membresia.Suspender(DateTimeOffset.UtcNow);

        Assert.Equal(EstadoMembresiaVeterinaria.Suspendida, membresia.Estado);
    }

    [Fact]
    public void AsignarRol_ConRolesDistintos_PermiteMultiplesRolesSimultaneos()
    {
        var membresia = MembresiaVeterinaria.Crear(Guid.NewGuid(), Guid.NewGuid());
        var rolVeterinario = Guid.NewGuid();
        var rolAdministrador = Guid.NewGuid();

        membresia.AsignarRol(rolVeterinario);
        membresia.AsignarRol(rolAdministrador);

        Assert.Equal(2, membresia.Roles.Count);
        Assert.Contains(membresia.Roles, r => r.RolId == rolVeterinario);
        Assert.Contains(membresia.Roles, r => r.RolId == rolAdministrador);
    }

    [Fact]
    public void AsignarRol_ConRolYaAsignado_LanzaExcepcionDominio()
    {
        var membresia = MembresiaVeterinaria.Crear(Guid.NewGuid(), Guid.NewGuid());
        var rolId = Guid.NewGuid();
        membresia.AsignarRol(rolId);

        Assert.Throws<ExcepcionDominio>(() => membresia.AsignarRol(rolId));
    }

    [Fact]
    public void QuitarRol_ConRolAsignado_LoRemueveDeLaColeccion()
    {
        var membresia = MembresiaVeterinaria.Crear(Guid.NewGuid(), Guid.NewGuid());
        var rolId = Guid.NewGuid();
        membresia.AsignarRol(rolId);

        membresia.QuitarRol(rolId);

        Assert.Empty(membresia.Roles);
    }
}
