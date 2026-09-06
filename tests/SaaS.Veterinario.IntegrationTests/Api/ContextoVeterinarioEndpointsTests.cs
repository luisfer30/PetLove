using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SaaS.Veterinario.Api.Autenticacion;
using SaaS.Veterinario.Api.Personal;
using SaaS.Veterinario.Api.Veterinarias;
using SaaS.Veterinario.Domain.Membresias;
using SaaS.Veterinario.Domain.Veterinarias;
using SaaS.Veterinario.IntegrationTests.Infraestructura;

namespace SaaS.Veterinario.IntegrationTests.Api;

/// <summary>
/// Cubre la Etapa 5: resolucion de ContextoVeterinaria, autorizacion por permisos, y
/// aislamiento multi-tenant. Los roles usados (ADMINISTRADOR_VETERINARIA, VETERINARIO,
/// ASISTENTE) y sus permisos son los sembrados por la migracion inicial (Etapa 3) --
/// ADMINISTRADOR_VETERINARIA tiene los 6 permisos, VETERINARIO y ASISTENTE solo veterinaria.ver.
/// </summary>
[Collection(ColeccionPostgres.Nombre)]
public sealed class ContextoVeterinarioEndpointsTests : IClassFixture<SaaSVeterinarioApiFactory>
{
    private const string Password = "password123";

    private readonly PostgresContainerFixture postgres;
    private readonly SaaSVeterinarioApiFactory factory;

    public ContextoVeterinarioEndpointsTests(PostgresContainerFixture postgres, SaaSVeterinarioApiFactory factory)
    {
        this.postgres = postgres;
        this.factory = factory;
        factory.UsarPostgres(postgres);
    }

    private string MotivoOmision() => $"Docker no disponible en este entorno: {postgres.MotivoNoDisponible}";

    private static string CorreoUnico() => $"prueba.ctx.{Guid.NewGuid():N}@correo.com";

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
        var userManager = scope.ServiceProvider.GetRequiredService<Microsoft.AspNetCore.Identity.UserManager<SaaS.Veterinario.Infrastructure.Identidad.UsuarioIdentidad>>();
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

    private async Task<Guid> CrearMembresiaActivaAsync(Guid usuarioId, Guid veterinariaId, params string[] codigosRoles)
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
        return membresia.Id;
    }

    private async Task<TokenResponse> LoginAsync(HttpClient cliente, string correo)
    {
        var respuesta = await cliente.PostAsJsonAsync(
            "/api/v1/autenticacion/iniciar-sesion",
            new IniciarSesionRequest(correo, Password, null));
        respuesta.EnsureSuccessStatusCode();
        return (await respuesta.Content.ReadFromJsonAsync<TokenResponse>())!;
    }

    /// <summary>Registra, confirma, crea Veterinaria+MembresiaVeterinaria ACTIVA con los roles indicados, e inicia sesión.</summary>
    private async Task<(Guid UsuarioId, TokenResponse Tokens, Guid VeterinariaId, Guid MembresiaId)> CrearUsuarioConMembresiaAsync(
        HttpClient cliente, params string[] codigosRoles)
    {
        var (usuarioId, correo) = await RegistrarYConfirmarAsync(cliente);
        var veterinariaId = await CrearVeterinariaActivaAsync();
        var membresiaId = await CrearMembresiaActivaAsync(usuarioId, veterinariaId, codigosRoles);
        var tokens = await LoginAsync(cliente, correo);
        return (usuarioId, tokens, veterinariaId, membresiaId);
    }

    private static HttpClient ConToken(HttpClient cliente, string accessToken)
    {
        cliente.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        return cliente;
    }

    // ---------- Escenario A ----------

    [SkippableFact]
    public async Task EscenarioA_UsuarioConPermisoVeterinariaVer_ObtieneSuPropiaVeterinaria()
    {
        Skip.IfNot(postgres.EstaDisponible, MotivoOmision());

        using var cliente = factory.CreateClient();
        var (_, tokens, veterinariaId, _) = await CrearUsuarioConMembresiaAsync(cliente, "VETERINARIO");
        ConToken(cliente, tokens.AccessToken);

        var respuesta = await cliente.GetAsync("/api/v1/veterinarias/actual");

        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
        var veterinaria = await respuesta.Content.ReadFromJsonAsync<VeterinariaActualResponse>();
        Assert.Equal(veterinariaId, veterinaria!.Id);
    }

    // ---------- Escenario B ----------

    [SkippableFact]
    public async Task EscenarioB_UsuarioSinMembresia_Retorna403ContextoRequerido()
    {
        Skip.IfNot(postgres.EstaDisponible, MotivoOmision());

        using var cliente = factory.CreateClient();
        var (_, correo) = await RegistrarYConfirmarAsync(cliente);
        var tokens = await LoginAsync(cliente, correo);
        ConToken(cliente, tokens.AccessToken);

        var respuesta = await cliente.GetAsync("/api/v1/veterinarias/actual");

        Assert.Equal(HttpStatusCode.Forbidden, respuesta.StatusCode);
        var problema = await respuesta.Content.ReadFromJsonAsync<Dictionary<string, object>>();
        Assert.Equal("CONTEXTO_VETERINARIA_REQUERIDO", problema!["codigo"].ToString());
    }

    // ---------- Escenario C / prueba multi-tenant critica ----------

    [SkippableFact]
    public async Task UsuarioDeVeterinariaA_NoPuedeAccederADatosDeVeterinariaB()
    {
        Skip.IfNot(postgres.EstaDisponible, MotivoOmision());

        using var clienteA = factory.CreateClient();
        using var clienteB = factory.CreateClient();

        var (usuarioA, tokensA, veterinariaA, _) = await CrearUsuarioConMembresiaAsync(clienteA, "ADMINISTRADOR_VETERINARIA");
        var (_, tokensB, veterinariaB, _) = await CrearUsuarioConMembresiaAsync(clienteB, "ADMINISTRADOR_VETERINARIA");

        ConToken(clienteA, tokensA.AccessToken);

        // GET /veterinarias/actual con el JWT de A: nunca puede devolver la veterinaria de B.
        var respuestaActual = await clienteA.GetAsync("/api/v1/veterinarias/actual");
        Assert.Equal(HttpStatusCode.OK, respuestaActual.StatusCode);
        var veterinariaDevuelta = await respuestaActual.Content.ReadFromJsonAsync<VeterinariaActualResponse>();
        Assert.Equal(veterinariaA, veterinariaDevuelta!.Id);
        Assert.NotEqual(veterinariaB, veterinariaDevuelta.Id);

        // GET /personal con el JWT de A: nunca puede listar personal de B.
        var respuestaPersonal = await clienteA.GetAsync("/api/v1/personal");
        Assert.Equal(HttpStatusCode.OK, respuestaPersonal.StatusCode);
        var personal = await respuestaPersonal.Content.ReadFromJsonAsync<List<MiembroPersonalResponse>>();
        Assert.Single(personal!);
        Assert.Equal(usuarioA, personal![0].UsuarioId);

        // No existe ningun endpoint que acepte VeterinariaId como parametro -- el contexto se
        // resuelve exclusivamente desde la membresia del token, nunca desde algo manipulable
        // por el cliente. Esto ya lo demuestran las dos aserciones de arriba: con el MISMO
        // JWT de A, es estructuralmente imposible obtener datos de B.
    }

    // ---------- Veterinaria suspendida (seccion 5) ----------

    [SkippableFact]
    public async Task GetVeterinariaActual_ConVeterinariaSuspendida_Retorna403VeterinariaNoDisponible()
    {
        Skip.IfNot(postgres.EstaDisponible, MotivoOmision());

        using var cliente = factory.CreateClient();
        var (_, tokens, veterinariaId, _) = await CrearUsuarioConMembresiaAsync(cliente, "VETERINARIO");

        await using (var contexto = postgres.CrearDbContext())
        {
            var veterinaria = await contexto.Veterinarias.SingleAsync(v => v.Id == veterinariaId);
            veterinaria.Suspender(DateTimeOffset.UtcNow);
            await contexto.SaveChangesAsync();
        }

        ConToken(cliente, tokens.AccessToken);
        var respuesta = await cliente.GetAsync("/api/v1/veterinarias/actual");

        Assert.Equal(HttpStatusCode.Forbidden, respuesta.StatusCode);
        var problema = await respuesta.Content.ReadFromJsonAsync<Dictionary<string, object>>();
        Assert.Equal("VETERINARIA_NO_DISPONIBLE", problema!["codigo"].ToString());
    }

    // ---------- Escenario D ----------

    [SkippableFact]
    public async Task EscenarioD_UsuarioSinPersonalInvitar_Retorna403AlInvitar()
    {
        Skip.IfNot(postgres.EstaDisponible, MotivoOmision());

        using var cliente = factory.CreateClient();
        var (_, tokens, _, _) = await CrearUsuarioConMembresiaAsync(cliente, "ASISTENTE");
        ConToken(cliente, tokens.AccessToken);

        var respuesta = await cliente.PostAsJsonAsync(
            "/api/v1/personal/invitaciones",
            new InvitarPersonalRequest(CorreoUnico(), ["ASISTENTE"]));

        Assert.Equal(HttpStatusCode.Forbidden, respuesta.StatusCode);
        var problema = await respuesta.Content.ReadFromJsonAsync<Dictionary<string, object>>();
        Assert.Equal("AUTORIZACION_PERMISO_REQUERIDO", problema!["codigo"].ToString());
    }

    // ---------- Escenario E: agregar rol/permiso aplica sin emitir JWT nuevo ----------

    [SkippableFact]
    public async Task EscenarioE_AgregarRolConPermisoNuevo_AplicaInmediatamenteConElMismoJwt()
    {
        Skip.IfNot(postgres.EstaDisponible, MotivoOmision());

        using var cliente = factory.CreateClient();
        var (_, tokens, _, membresiaId) = await CrearUsuarioConMembresiaAsync(cliente, "VETERINARIO");
        ConToken(cliente, tokens.AccessToken);

        var respuestaAntes = await cliente.PostAsJsonAsync(
            "/api/v1/personal/invitaciones",
            new InvitarPersonalRequest(CorreoUnico(), ["ASISTENTE"]));
        Assert.Equal(HttpStatusCode.Forbidden, respuestaAntes.StatusCode);

        await using (var contexto = postgres.CrearDbContext())
        {
            var membresia = await contexto.MembresiasVeterinaria
                .Include(m => m.Roles)
                .SingleAsync(m => m.Id == membresiaId);
            var rolAdmin = await contexto.Roles.SingleAsync(r => r.Codigo == "ADMINISTRADOR_VETERINARIA");
            membresia.AsignarRol(rolAdmin.Id);
            await contexto.SaveChangesAsync();
        }

        // Mismo AccessToken de antes -- nunca se emitio un JWT nuevo.
        var respuestaDespues = await cliente.PostAsJsonAsync(
            "/api/v1/personal/invitaciones",
            new InvitarPersonalRequest(CorreoUnico(), ["ASISTENTE"]));

        Assert.Equal(HttpStatusCode.OK, respuestaDespues.StatusCode);
    }

    // ---------- Escenario F: quitar rol/permiso pierde acceso de inmediato con el mismo JWT ----------

    [SkippableFact]
    public async Task EscenarioF_QuitarRolConPermiso_PierdeAccesoInmediatamenteConElMismoJwt()
    {
        Skip.IfNot(postgres.EstaDisponible, MotivoOmision());

        using var cliente = factory.CreateClient();
        var (_, tokens, _, membresiaId) = await CrearUsuarioConMembresiaAsync(cliente, "ADMINISTRADOR_VETERINARIA");
        ConToken(cliente, tokens.AccessToken);

        var respuestaAntes = await cliente.PostAsJsonAsync(
            "/api/v1/personal/invitaciones",
            new InvitarPersonalRequest(CorreoUnico(), ["ASISTENTE"]));
        Assert.Equal(HttpStatusCode.OK, respuestaAntes.StatusCode);

        await using (var contexto = postgres.CrearDbContext())
        {
            var membresia = await contexto.MembresiasVeterinaria
                .Include(m => m.Roles)
                .SingleAsync(m => m.Id == membresiaId);
            var rolAdmin = await contexto.Roles.SingleAsync(r => r.Codigo == "ADMINISTRADOR_VETERINARIA");
            membresia.QuitarRol(rolAdmin.Id);
            await contexto.SaveChangesAsync();
        }

        // Mismo AccessToken de antes.
        var respuestaDespues = await cliente.PostAsJsonAsync(
            "/api/v1/personal/invitaciones",
            new InvitarPersonalRequest(CorreoUnico(), ["ASISTENTE"]));

        Assert.Equal(HttpStatusCode.Forbidden, respuestaDespues.StatusCode);
    }

    // ---------- PUT /veterinarias/actual ----------

    [SkippableFact]
    public async Task PutVeterinariaActual_ConPermisoVeterinariaEditar_ActualizaLosDatos()
    {
        Skip.IfNot(postgres.EstaDisponible, MotivoOmision());

        using var cliente = factory.CreateClient();
        var (_, tokens, veterinariaId, _) = await CrearUsuarioConMembresiaAsync(cliente, "ADMINISTRADOR_VETERINARIA");
        ConToken(cliente, tokens.AccessToken);

        var respuesta = await cliente.PutAsJsonAsync(
            "/api/v1/veterinarias/actual",
            new ActualizarVeterinariaRequest("Clinica Renovada", null, null, "0999999999", null, "Quito", "Ecuador", "America/Guayaquil"));

        Assert.Equal(HttpStatusCode.NoContent, respuesta.StatusCode);

        await using var contexto = postgres.CrearDbContext();
        var veterinaria = await contexto.Veterinarias.SingleAsync(v => v.Id == veterinariaId);
        Assert.Equal("Clinica Renovada", veterinaria.NombreComercial);
        Assert.Equal("Quito", veterinaria.Ciudad);
    }

    // ---------- POST /personal/invitaciones ----------

    [SkippableFact]
    public async Task PostInvitaciones_ConRolInexistente_Retorna400PersonalRolInvalido()
    {
        Skip.IfNot(postgres.EstaDisponible, MotivoOmision());

        using var cliente = factory.CreateClient();
        var (_, tokens, _, _) = await CrearUsuarioConMembresiaAsync(cliente, "ADMINISTRADOR_VETERINARIA");
        ConToken(cliente, tokens.AccessToken);

        var respuesta = await cliente.PostAsJsonAsync(
            "/api/v1/personal/invitaciones",
            new InvitarPersonalRequest(CorreoUnico(), ["ROL_QUE_NO_EXISTE"]));

        Assert.Equal(HttpStatusCode.BadRequest, respuesta.StatusCode);
        var problema = await respuesta.Content.ReadFromJsonAsync<Dictionary<string, object>>();
        Assert.Equal("PERSONAL_ROL_INVALIDO", problema!["codigo"].ToString());
    }

    [SkippableFact]
    public async Task PostInvitaciones_ConDatosValidos_CreaLaInvitacion()
    {
        Skip.IfNot(postgres.EstaDisponible, MotivoOmision());

        using var cliente = factory.CreateClient();
        var (_, tokens, veterinariaId, _) = await CrearUsuarioConMembresiaAsync(cliente, "ADMINISTRADOR_VETERINARIA");
        ConToken(cliente, tokens.AccessToken);

        var respuesta = await cliente.PostAsJsonAsync(
            "/api/v1/personal/invitaciones",
            new InvitarPersonalRequest(CorreoUnico(), ["VETERINARIO"]));

        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
        var cuerpo = await respuesta.Content.ReadFromJsonAsync<InvitarPersonalResponse>();

        await using var contexto = postgres.CrearDbContext();
        var invitacion = await contexto.InvitacionesPersonal.SingleAsync(i => i.Id == cuerpo!.InvitacionId);
        Assert.Equal(veterinariaId, invitacion.VeterinariaId);
    }
}
