using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SaaS.Veterinario.Api.Autenticacion;
using SaaS.Veterinario.Application.Abstracciones;
using SaaS.Veterinario.Domain.ObjetosValor;
using SaaS.Veterinario.Domain.Usuarios;
using SaaS.Veterinario.IntegrationTests.Infraestructura;

namespace SaaS.Veterinario.IntegrationTests.Api;

/// <summary>
/// Verifica contra PostgreSQL real que el registro (Usuario de dominio + identidad de
/// Identity) es realmente atomico: si cualquiera de los dos pasos falla, o si algo falla
/// DESPUES de que ambos ya escribieron pero antes de confirmar la transaccion, no debe
/// quedar ni Usuario ni UsuarioIdentidad -- nunca una escritura parcial.
/// </summary>
[Collection(ColeccionPostgres.Nombre)]
public sealed class RegistroTransaccionalTests : IClassFixture<SaaSVeterinarioApiFactory>
{
    private const string PasswordValido = "password123";

    private readonly PostgresContainerFixture postgres;
    private readonly SaaSVeterinarioApiFactory factory;

    public RegistroTransaccionalTests(PostgresContainerFixture postgres, SaaSVeterinarioApiFactory factory)
    {
        this.postgres = postgres;
        this.factory = factory;
        factory.UsarPostgres(postgres);
    }

    private string MotivoOmision() => $"Docker no disponible en este entorno: {postgres.MotivoNoDisponible}";

    private static string CorreoUnico() => $"prueba.tx.{Guid.NewGuid():N}@correo.com";

    /// <summary>
    /// Escenario 1: el Usuario de dominio se inserta correctamente dentro de la transaccion,
    /// pero la creacion de la identidad falla (password mas corto que el minimo configurado
    /// en Identity, RequiredLength = 8) ANTES de confirmar. RegistrarUsuario convierte ese
    /// fallo en ExcepcionAplicacion dentro del delegate de IUnidadDeTrabajo, lo que dispara
    /// el rollback de la transaccion real -- incluyendo el INSERT de Usuario ya ejecutado.
    /// </summary>
    [SkippableFact]
    public async Task Registrar_ConPasswordQueFallaEnIdentity_HaceRollbackCompletoYNoDejaUsuarioNiIdentidad()
    {
        Skip.IfNot(postgres.EstaDisponible, MotivoOmision());

        using var cliente = factory.CreateClient();
        var correo = CorreoUnico();

        var respuesta = await cliente.PostAsJsonAsync(
            "/api/v1/autenticacion/registrar",
            new RegistrarUsuarioRequest("Ana Perez", correo, null, "1234567")); // 7 caracteres: falla RequiredLength = 8

        Assert.Equal(HttpStatusCode.BadRequest, respuesta.StatusCode);

        await using var contexto = postgres.CrearDbContext();
        Assert.False(await contexto.Usuarios.AnyAsync(u => u.Correo == Correo.Crear(correo)));
        Assert.False(await contexto.Users.AnyAsync(u => u.Email == correo));
    }

    /// <summary>
    /// Escenario 2: ambas escrituras (Usuario e identidad) se ejecutan correctamente dentro
    /// de la transaccion, y DESPUES ocurre un fallo no relacionado antes del commit. Usa los
    /// mismos componentes reales que RegistrarUsuario (IRepositorioUsuarios, IServicioIdentidad,
    /// IUnidadDeTrabajo resueltos del contenedor de DI de la Api real) para reproducir
    /// exactamente el mismo mecanismo transaccional, ya que el caso de uso en si no tiene un
    /// tercer paso natural donde forzar un fallo posterior a un registro exitoso.
    /// </summary>
    [SkippableFact]
    public async Task EjecutarEnTransaccion_ConFalloPosteriorAAmbasEscrituras_HaceRollbackCompletoYNoDejaUsuarioNiIdentidad()
    {
        Skip.IfNot(postgres.EstaDisponible, MotivoOmision());

        using var scope = factory.Services.CreateScope();
        var repositorioUsuarios = scope.ServiceProvider.GetRequiredService<IRepositorioUsuarios>();
        var servicioIdentidad = scope.ServiceProvider.GetRequiredService<IServicioIdentidad>();
        var unidadDeTrabajo = scope.ServiceProvider.GetRequiredService<IUnidadDeTrabajo>();

        var correo = CorreoUnico();
        var usuario = Usuario.Crear("Ana Perez", correo);
        var identidadFueExitosa = false;

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            unidadDeTrabajo.EjecutarEnTransaccionAsync(
                async ct =>
                {
                    await repositorioUsuarios.AgregarAsync(usuario, ct);

                    var resultadoIdentidad = await servicioIdentidad.CrearAsync(usuario.Id, usuario.Correo.Valor, PasswordValido, ct);
                    identidadFueExitosa = resultadoIdentidad.Exitoso;

                    // Fallo simulado DESPUES de que ambas escrituras ya se ejecutaron dentro
                    // de la transaccion, pero antes de que EjecutarEnTransaccionAsync confirme.
                    throw new InvalidOperationException("Fallo simulado posterior a ambas escrituras.");
                },
                CancellationToken.None));

        // Confirma que, en efecto, ambas escrituras se habian completado antes del fallo --
        // si esto fuera false, el rollback de abajo no probaria nada (nunca hubo nada que
        // revertir).
        Assert.True(identidadFueExitosa);

        await using var contexto = postgres.CrearDbContext();
        Assert.False(await contexto.Usuarios.AnyAsync(u => u.Id == usuario.Id));
        Assert.False(await contexto.Users.AnyAsync(u => u.UsuarioId == usuario.Id));
    }
}
