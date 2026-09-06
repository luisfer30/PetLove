using SaaS.Veterinario.Domain.Errores;
using SaaS.Veterinario.Domain.Usuarios;

namespace SaaS.Veterinario.Domain.Tests.Usuarios;

public class UsuarioTests
{
    [Fact]
    public void Crear_ConDatosValidos_InicializaEnPendienteVerificacion()
    {
        var usuario = Usuario.Crear("Ana Perez", "ANA@Correo.com");

        Assert.Equal("Ana Perez", usuario.Nombre);
        Assert.Equal("ana@correo.com", usuario.Correo.Valor);
        Assert.Equal(EstadoUsuario.PendienteVerificacion, usuario.Estado);
        Assert.False(usuario.CorreoVerificado);
        Assert.False(usuario.TelefonoVerificado);
        Assert.NotEqual(Guid.Empty, usuario.Id);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Crear_SinCorreo_LanzaExcepcionDominio(string? correoInvalido)
    {
        Assert.Throws<ExcepcionDominio>(() => Usuario.Crear("Ana Perez", correoInvalido!));
    }

    [Fact]
    public void Crear_SinNombre_LanzaExcepcionDominio()
    {
        Assert.Throws<ExcepcionDominio>(() => Usuario.Crear("   ", "ana@correo.com"));
    }

    [Theory]
    [InlineData("USUARIO@CORREO.COM", "usuario@correo.com")]
    [InlineData("  usuario@Correo.Com  ", "usuario@correo.com")]
    public void Crear_NormalizaCorreo(string correoOriginal, string correoEsperado)
    {
        var usuario = Usuario.Crear("Ana Perez", correoOriginal);

        Assert.Equal(correoEsperado, usuario.Correo.Valor);
    }

    [Fact]
    public void Activar_DesdePendienteVerificacion_CambiaEstadoAActivo()
    {
        var usuario = Usuario.Crear("Ana Perez", "ana@correo.com");
        var momento = DateTimeOffset.UtcNow;

        usuario.Activar(momento);

        Assert.Equal(EstadoUsuario.Activo, usuario.Estado);
        Assert.Equal(momento, usuario.FechaActualizacion);
    }

    [Fact]
    public void Bloquear_DesdePendienteVerificacion_LanzaExcepcionDominio()
    {
        var usuario = Usuario.Crear("Ana Perez", "ana@correo.com");

        Assert.Throws<ExcepcionDominio>(() => usuario.Bloquear(DateTimeOffset.UtcNow));
    }

    [Fact]
    public void Suspender_DesdeActivo_CambiaEstadoASuspendido()
    {
        var usuario = Usuario.Crear("Ana Perez", "ana@correo.com");
        usuario.Activar(DateTimeOffset.UtcNow);

        usuario.Suspender(DateTimeOffset.UtcNow);

        Assert.Equal(EstadoUsuario.Suspendido, usuario.Estado);
    }

    [Fact]
    public void Activar_DesdeSuspendido_CambiaEstadoAActivo()
    {
        var usuario = Usuario.Crear("Ana Perez", "ana@correo.com");
        usuario.Activar(DateTimeOffset.UtcNow);
        usuario.Suspender(DateTimeOffset.UtcNow);

        usuario.Activar(DateTimeOffset.UtcNow);

        Assert.Equal(EstadoUsuario.Activo, usuario.Estado);
    }

    [Fact]
    public void ConfirmarCorreo_MarcaCorreoComoVerificado()
    {
        var usuario = Usuario.Crear("Ana Perez", "ana@correo.com");

        usuario.ConfirmarCorreo();

        Assert.True(usuario.CorreoVerificado);
    }

    [Fact]
    public void ConfirmarTelefono_ConTelefonoRegistrado_MarcaTelefonoComoVerificado()
    {
        var usuario = Usuario.Crear("Ana Perez", "ana@correo.com", telefono: "0999999999");

        usuario.ConfirmarTelefono();

        Assert.True(usuario.TelefonoVerificado);
    }

    [Fact]
    public void ConfirmarTelefono_SinTelefonoRegistrado_LanzaExcepcionDominio()
    {
        var usuario = Usuario.Crear("Ana Perez", "ana@correo.com");

        Assert.Throws<ExcepcionDominio>(() => usuario.ConfirmarTelefono());
    }
}
