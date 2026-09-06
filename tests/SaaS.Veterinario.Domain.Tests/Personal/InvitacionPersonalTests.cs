using SaaS.Veterinario.Domain.Errores;
using SaaS.Veterinario.Domain.Personal;

namespace SaaS.Veterinario.Domain.Tests.Personal;

public class InvitacionPersonalTests
{
    private static InvitacionPersonal CrearInvitacionValida()
        => InvitacionPersonal.Crear(
            veterinariaId: Guid.NewGuid(),
            correo: "nueva.persona@correo.com",
            tokenHash: "hash-de-prueba",
            creadoPorUsuarioId: Guid.NewGuid(),
            fechaExpiracion: DateTimeOffset.UtcNow.AddDays(7));

    [Fact]
    public void Crear_ConDatosValidos_QuedaPendienteYSinRoles()
    {
        var invitacion = CrearInvitacionValida();

        Assert.Equal(EstadoInvitacionPersonal.Pendiente, invitacion.Estado);
        Assert.Equal("nueva.persona@correo.com", invitacion.Correo.Valor);
        Assert.Null(invitacion.FechaAceptacion);
        Assert.Empty(invitacion.Roles);
    }

    [Fact]
    public void Crear_ConFechaExpiracionAnteriorOIgualALaCreacion_LanzaExcepcionDominio()
    {
        var ahora = DateTimeOffset.UtcNow;

        Assert.Throws<ExcepcionDominio>(() => InvitacionPersonal.Crear(
            veterinariaId: Guid.NewGuid(),
            correo: "nueva.persona@correo.com",
            tokenHash: "hash-de-prueba",
            creadoPorUsuarioId: Guid.NewGuid(),
            fechaExpiracion: ahora,
            fechaCreacion: ahora));
    }

    [Fact]
    public void Crear_SinTokenHash_LanzaExcepcionDominio()
    {
        Assert.Throws<ExcepcionDominio>(() => InvitacionPersonal.Crear(
            veterinariaId: Guid.NewGuid(),
            correo: "nueva.persona@correo.com",
            tokenHash: "   ",
            creadoPorUsuarioId: Guid.NewGuid(),
            fechaExpiracion: DateTimeOffset.UtcNow.AddDays(7)));
    }

    [Fact]
    public void AsignarRol_ConRolesDistintos_PermiteMultiplesRoles()
    {
        var invitacion = CrearInvitacionValida();
        var rolVeterinario = Guid.NewGuid();
        var rolAdministrador = Guid.NewGuid();

        invitacion.AsignarRol(rolVeterinario);
        invitacion.AsignarRol(rolAdministrador);

        Assert.Equal(2, invitacion.Roles.Count);
    }

    [Fact]
    public void AsignarRol_ConRolYaAsignado_LanzaExcepcionDominio()
    {
        var invitacion = CrearInvitacionValida();
        var rolId = Guid.NewGuid();
        invitacion.AsignarRol(rolId);

        Assert.Throws<ExcepcionDominio>(() => invitacion.AsignarRol(rolId));
    }
}
