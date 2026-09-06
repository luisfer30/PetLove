using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SaaS.Veterinario.Api.Autenticacion;
using SaaS.Veterinario.Api.Personal;
using SaaS.Veterinario.Domain.Membresias;
using SaaS.Veterinario.Domain.Veterinarias;
using SaaS.Veterinario.IntegrationTests.Infraestructura;

namespace SaaS.Veterinario.IntegrationTests.Api;

/// <summary>
/// Ajuste solicitado sobre la Etapa 5: una invitacion PENDIENTE duplicada (misma
/// veterinaria + mismo correo normalizado) debe rechazarse de forma controlada con
/// PERSONAL_INVITACION_PENDIENTE_EXISTENTE. El indice unico parcial en BD
/// (veterinaria_id, correo) WHERE estado = 'PENDIENTE' se mantiene como segunda barrera.
/// </summary>
[Collection(ColeccionPostgres.Nombre)]
public sealed class InvitacionPendienteDuplicadaTests : IClassFixture<SaaSVeterinarioApiFactory>
{
    private const string Password = "password123";

    private readonly PostgresContainerFixture postgres;
    private readonly SaaSVeterinarioApiFactory factory;

    public InvitacionPendienteDuplicadaTests(PostgresContainerFixture postgres, SaaSVeterinarioApiFactory factory)
    {
        this.postgres = postgres;
        this.factory = factory;
        factory.UsarPostgres(postgres);
    }

    private string MotivoOmision() => $"Docker no disponible en este entorno: {postgres.MotivoNoDisponible}";

    private static string CorreoUnico() => $"prueba.inv.{Guid.NewGuid():N}@correo.com";

    private static string GenerarCodigoPublico() => $"VET-{Guid.NewGuid():N}"[..10].ToUpperInvariant();

    private async Task<(Guid UsuarioId, string Correo)> RegistrarYConfirmarAsync(HttpClient cliente)
    {
        var correo = CorreoUnico();
        var respuesta = await cliente.PostAsJsonAsync(
            "/api/v1/autenticacion/registrar",
            new RegistrarUsuarioRequest("Persona de Prueba", correo, null, Password));
        respuesta.EnsureSuccessStatusCode();
        var cuerpo = (await respuesta.Content.ReadFromJsonAsync<RegistrarUsuarioResponse>())!;

        using var scope = factory.Services.CreateScope();
        var userManager = scope.ServiceProvider
            .GetRequiredService<Microsoft.AspNetCore.Identity.UserManager<SaaS.Veterinario.Infrastructure.Identidad.UsuarioIdentidad>>();
        var identidad = await userManager.Users.FirstAsync(u => u.UsuarioId == cuerpo.UsuarioId);
        var token = await userManager.GenerateEmailConfirmationTokenAsync(identidad);

        var respuestaConfirmar = await cliente.PostAsJsonAsync(
            "/api/v1/autenticacion/confirmar-correo",
            new ConfirmarCorreoRequest(cuerpo.UsuarioId, token));
        respuestaConfirmar.EnsureSuccessStatusCode();

        return (cuerpo.UsuarioId, cuerpo.Correo);
    }

    private async Task<Guid> CrearVeterinariaActivaAsync()
    {
        await using var contexto = postgres.CrearDbContext();
        var veterinaria = Veterinaria.Crear(
            GenerarCodigoPublico(), "Clinica de Prueba", $"contacto.{Guid.NewGuid():N}@clinica.com", "Ecuador", "America/Guayaquil");
        veterinaria.Activar(DateTimeOffset.UtcNow);
        contexto.Veterinarias.Add(veterinaria);
        await contexto.SaveChangesAsync();
        return veterinaria.Id;
    }

    private async Task CrearMembresiaActivaAsync(Guid usuarioId, Guid veterinariaId, params string[] codigosRoles)
    {
        await using var contexto = postgres.CrearDbContext();
        var membresia = MembresiaVeterinaria.Crear(usuarioId, veterinariaId);
        membresia.Activar(DateTimeOffset.UtcNow);

        var roles = await contexto.Roles.Where(r => codigosRoles.Contains(r.Codigo)).ToListAsync();
        foreach (var rol in roles)
        {
            membresia.AsignarRol(rol.Id);
        }

        contexto.MembresiasVeterinaria.Add(membresia);
        await contexto.SaveChangesAsync();
    }

    private async Task<(HttpClient Cliente, Guid VeterinariaId)> CrearClienteAdministradorAsync()
    {
        var cliente = factory.CreateClient();
        var (usuarioId, correo) = await RegistrarYConfirmarAsync(cliente);
        var veterinariaId = await CrearVeterinariaActivaAsync();
        await CrearMembresiaActivaAsync(usuarioId, veterinariaId, "ADMINISTRADOR_VETERINARIA");

        var respuestaLogin = await cliente.PostAsJsonAsync(
            "/api/v1/autenticacion/iniciar-sesion",
            new IniciarSesionRequest(correo, Password, null));
        respuestaLogin.EnsureSuccessStatusCode();
        var tokens = (await respuestaLogin.Content.ReadFromJsonAsync<TokenResponse>())!;
        cliente.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokens.AccessToken);

        return (cliente, veterinariaId);
    }

    private static Task<HttpResponseMessage> InvitarAsync(HttpClient cliente, string correo)
        => cliente.PostAsJsonAsync("/api/v1/personal/invitaciones", new InvitarPersonalRequest(correo, ["VETERINARIO"]));

    // 1. Crear primera invitacion pendiente -> exito.
    [SkippableFact]
    public async Task Invitar_PrimeraInvitacionPendiente_Exito()
    {
        Skip.IfNot(postgres.EstaDisponible, MotivoOmision());

        var administrador = await CrearClienteAdministradorAsync();
        using var cliente = administrador.Cliente;
        var correo = CorreoUnico();

        var respuesta = await InvitarAsync(cliente, correo);

        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
    }

    // 2. Segunda invitacion pendiente con el mismo correo y la misma veterinaria -> rechazada.
    [SkippableFact]
    public async Task Invitar_SegundaInvitacionPendienteMismoCorreoYVeterinaria_RetornaPersonalInvitacionPendienteExistente()
    {
        Skip.IfNot(postgres.EstaDisponible, MotivoOmision());

        var administrador = await CrearClienteAdministradorAsync();
        using var cliente = administrador.Cliente;
        var correo = CorreoUnico();

        var primeraRespuesta = await InvitarAsync(cliente, correo);
        primeraRespuesta.EnsureSuccessStatusCode();

        // Mismo correo, distinta capitalizacion: debe seguir contando como el mismo correo
        // normalizado.
        var segundaRespuesta = await InvitarAsync(cliente, correo.ToUpperInvariant());

        Assert.Equal(HttpStatusCode.Conflict, segundaRespuesta.StatusCode);
        var problema = await segundaRespuesta.Content.ReadFromJsonAsync<Dictionary<string, object>>();
        Assert.Equal("PERSONAL_INVITACION_PENDIENTE_EXISTENTE", problema!["codigo"].ToString());
    }

    // 3. El mismo correo en otra veterinaria no debe considerarse duplicado por esta regla.
    [SkippableFact]
    public async Task Invitar_MismoCorreoEnOtraVeterinaria_NoSeConsideraDuplicado()
    {
        Skip.IfNot(postgres.EstaDisponible, MotivoOmision());

        var administradorA = await CrearClienteAdministradorAsync();
        var administradorB = await CrearClienteAdministradorAsync();
        using var clienteA = administradorA.Cliente;
        using var clienteB = administradorB.Cliente;
        Assert.NotEqual(administradorA.VeterinariaId, administradorB.VeterinariaId);

        var correoCompartido = CorreoUnico();

        var respuestaA = await InvitarAsync(clienteA, correoCompartido);
        var respuestaB = await InvitarAsync(clienteB, correoCompartido);

        Assert.Equal(HttpStatusCode.OK, respuestaA.StatusCode);
        Assert.Equal(HttpStatusCode.OK, respuestaB.StatusCode);
    }

    // 4. Una invitacion anterior no-PENDIENTE no debe bloquear una nueva. El dominio todavia
    // no expone Aceptar/Expirar/Cancelar (no hay caso de uso que los necesite en esta etapa),
    // asi que el estado se fuerza directamente en BD para poder ejercitar este caso -- no es
    // un cambio de arquitectura, es la unica forma de preparar el dato para la prueba.
    [SkippableFact]
    public async Task Invitar_ConInvitacionAnteriorNoPendiente_NoBloqueaLaNueva()
    {
        Skip.IfNot(postgres.EstaDisponible, MotivoOmision());

        var administrador = await CrearClienteAdministradorAsync();
        using var cliente = administrador.Cliente;
        var correo = CorreoUnico();

        var primeraRespuesta = await InvitarAsync(cliente, correo);
        primeraRespuesta.EnsureSuccessStatusCode();
        var primeraInvitacion = (await primeraRespuesta.Content.ReadFromJsonAsync<InvitarPersonalResponse>())!;

        await using (var contexto = postgres.CrearDbContext())
        {
            await contexto.Database.ExecuteSqlInterpolatedAsync(
                $"UPDATE invitaciones_personal SET estado = 'CANCELADA' WHERE id = {primeraInvitacion.InvitacionId}");
        }

        var segundaRespuesta = await InvitarAsync(cliente, correo);

        Assert.Equal(HttpStatusCode.OK, segundaRespuesta.StatusCode);
    }
}
