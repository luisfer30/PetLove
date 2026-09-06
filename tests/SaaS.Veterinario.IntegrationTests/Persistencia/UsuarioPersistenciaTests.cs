using Microsoft.EntityFrameworkCore;
using SaaS.Veterinario.Domain.Usuarios;
using SaaS.Veterinario.IntegrationTests.Infraestructura;

namespace SaaS.Veterinario.IntegrationTests.Persistencia;

[Collection(ColeccionPostgres.Nombre)]
public sealed class UsuarioPersistenciaTests(PostgresContainerFixture fixture)
{
    [SkippableFact]
    public async Task GuardarUsuario_PersisteYRecuperaCorreoNormalizado()
    {
        Skip.IfNot(fixture.EstaDisponible, $"Docker no disponible en este entorno: {fixture.MotivoNoDisponible}");

        var correoOriginal = $"Usuario.{Guid.NewGuid():N}@Correo.COM";
        var usuario = Usuario.Crear("Ana Perez", correoOriginal);

        await using (var contexto = fixture.CrearDbContext())
        {
            contexto.Usuarios.Add(usuario);
            await contexto.SaveChangesAsync();
        }

        await using (var contexto = fixture.CrearDbContext())
        {
            var usuarioGuardado = await contexto.Usuarios.SingleAsync(u => u.Id == usuario.Id);

            Assert.Equal(correoOriginal.Trim().ToLowerInvariant(), usuarioGuardado.Correo.Valor);
            Assert.Equal("Ana Perez", usuarioGuardado.Nombre);
            Assert.Equal(EstadoUsuario.PendienteVerificacion, usuarioGuardado.Estado);
        }
    }

    [SkippableFact]
    public async Task GuardarUsuario_ConCorreoDuplicado_LanzaExcepcionDePersistencia()
    {
        Skip.IfNot(fixture.EstaDisponible, $"Docker no disponible en este entorno: {fixture.MotivoNoDisponible}");

        var correo = $"duplicado.{Guid.NewGuid():N}@correo.com";

        await using (var contexto = fixture.CrearDbContext())
        {
            contexto.Usuarios.Add(Usuario.Crear("Primer Usuario", correo));
            await contexto.SaveChangesAsync();
        }

        await using var otroContexto = fixture.CrearDbContext();

        // Distinta capitalizacion: el indice UNIQUE protege sobre el valor ya normalizado
        // por el dominio, asi que igual debe fallar.
        otroContexto.Usuarios.Add(Usuario.Crear("Segundo Usuario", correo.ToUpperInvariant()));

        await Assert.ThrowsAsync<DbUpdateException>(() => otroContexto.SaveChangesAsync());
    }
}
