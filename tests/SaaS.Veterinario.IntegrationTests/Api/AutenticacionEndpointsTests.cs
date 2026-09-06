using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SaaS.Veterinario.Api.Autenticacion;
using SaaS.Veterinario.Api.Usuarios;
using SaaS.Veterinario.Domain.Usuarios;
using SaaS.Veterinario.Infrastructure.Identidad;
using SaaS.Veterinario.IntegrationTests.Infraestructura;

namespace SaaS.Veterinario.IntegrationTests.Api;

/// <summary>
/// Pruebas de extremo a extremo contra la Api real (TestServer) y PostgreSQL real
/// (Testcontainers, compartido con PostgresContainerFixture). Cada prueba usa un correo
/// aleatorio para no interferir con las demas, ya que la base no se resetea entre pruebas.
/// </summary>
[Collection(ColeccionPostgres.Nombre)]
public sealed class AutenticacionEndpointsTests : IClassFixture<SaaSVeterinarioApiFactory>
{
    private const string Password = "password123";

    private readonly PostgresContainerFixture postgres;
    private readonly SaaSVeterinarioApiFactory factory;

    public AutenticacionEndpointsTests(PostgresContainerFixture postgres, SaaSVeterinarioApiFactory factory)
    {
        this.postgres = postgres;
        this.factory = factory;
        factory.UsarPostgres(postgres);
    }

    private string MotivoOmision() => $"Docker no disponible en este entorno: {postgres.MotivoNoDisponible}";

    private static string CorreoUnico() => $"prueba.{Guid.NewGuid():N}@correo.com";

    private async Task<(Guid UsuarioId, string Correo)> RegistrarAsync(HttpClient cliente, string? correo = null)
    {
        correo ??= CorreoUnico();
        var respuesta = await cliente.PostAsJsonAsync(
            "/api/v1/autenticacion/registrar",
            new RegistrarUsuarioRequest("Ana Perez", correo, null, Password));

        respuesta.EnsureSuccessStatusCode();
        var cuerpo = await respuesta.Content.ReadFromJsonAsync<RegistrarUsuarioResponse>();
        return (cuerpo!.UsuarioId, cuerpo.Correo);
    }

    private async Task<string> GenerarTokenConfirmacionAsync(Guid usuarioId)
    {
        using var scope = factory.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<UsuarioIdentidad>>();
        var identidad = await userManager.Users.FirstAsync(u => u.UsuarioId == usuarioId);
        return await userManager.GenerateEmailConfirmationTokenAsync(identidad);
    }

    private async Task ConfirmarCorreoAsync(HttpClient cliente, Guid usuarioId)
    {
        var token = await GenerarTokenConfirmacionAsync(usuarioId);
        var respuesta = await cliente.PostAsJsonAsync(
            "/api/v1/autenticacion/confirmar-correo",
            new ConfirmarCorreoRequest(usuarioId, token));

        respuesta.EnsureSuccessStatusCode();
    }

    private async Task<(TokenResponse Tokens, string Correo)> RegistrarConfirmarYLoginearAsync(HttpClient cliente, string? dispositivo = null)
    {
        var (usuarioId, correo) = await RegistrarAsync(cliente);
        await ConfirmarCorreoAsync(cliente, usuarioId);

        var respuestaLogin = await cliente.PostAsJsonAsync(
            "/api/v1/autenticacion/iniciar-sesion",
            new IniciarSesionRequest(correo, Password, dispositivo));

        respuestaLogin.EnsureSuccessStatusCode();
        var tokens = (await respuestaLogin.Content.ReadFromJsonAsync<TokenResponse>())!;
        return (tokens, correo);
    }

    private static string Hashear(string valor) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(valor)));

    [SkippableFact]
    public async Task Registrar_ConDatosValidos_RetornaCorreoNormalizado()
    {
        Skip.IfNot(postgres.EstaDisponible, MotivoOmision());

        using var cliente = factory.CreateClient();
        var correoOriginal = $"Prueba.{Guid.NewGuid():N}@Correo.COM";

        var respuesta = await cliente.PostAsJsonAsync(
            "/api/v1/autenticacion/registrar",
            new RegistrarUsuarioRequest("Ana Perez", correoOriginal, null, Password));

        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
        var cuerpo = await respuesta.Content.ReadFromJsonAsync<RegistrarUsuarioResponse>();
        Assert.Equal(correoOriginal.ToLowerInvariant(), cuerpo!.Correo);
    }

    [SkippableFact]
    public async Task Registrar_ConCorreoDuplicado_Retorna409()
    {
        Skip.IfNot(postgres.EstaDisponible, MotivoOmision());

        using var cliente = factory.CreateClient();
        var (_, correo) = await RegistrarAsync(cliente);

        var respuesta = await cliente.PostAsJsonAsync(
            "/api/v1/autenticacion/registrar",
            new RegistrarUsuarioRequest("Otro Nombre", correo, null, Password));

        Assert.Equal(HttpStatusCode.Conflict, respuesta.StatusCode);
    }

    [SkippableFact]
    public async Task IniciarSesion_ConPasswordIncorrecto_Retorna401()
    {
        Skip.IfNot(postgres.EstaDisponible, MotivoOmision());

        using var cliente = factory.CreateClient();
        var (usuarioId, correo) = await RegistrarAsync(cliente);
        await ConfirmarCorreoAsync(cliente, usuarioId);

        var respuesta = await cliente.PostAsJsonAsync(
            "/api/v1/autenticacion/iniciar-sesion",
            new IniciarSesionRequest(correo, "password-incorrecto", null));

        Assert.Equal(HttpStatusCode.Unauthorized, respuesta.StatusCode);
    }

    [SkippableFact]
    public async Task ConfirmarCorreo_ConTokenValido_ActivaAlUsuario()
    {
        Skip.IfNot(postgres.EstaDisponible, MotivoOmision());

        using var cliente = factory.CreateClient();
        var (usuarioId, _) = await RegistrarAsync(cliente);

        await ConfirmarCorreoAsync(cliente, usuarioId);

        await using var contexto = postgres.CrearDbContext();
        var usuario = await contexto.Usuarios.SingleAsync(u => u.Id == usuarioId);
        Assert.Equal(EstadoUsuario.Activo, usuario.Estado);
        Assert.True(usuario.CorreoVerificado);
    }

    [SkippableFact]
    public async Task IniciarSesion_ConCredencialesValidas_GeneraJwtValidoYRefreshTokenHasheado()
    {
        Skip.IfNot(postgres.EstaDisponible, MotivoOmision());

        using var cliente = factory.CreateClient();
        var (tokens, correo) = await RegistrarConfirmarYLoginearAsync(cliente);

        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(tokens.AccessToken);
        Assert.Equal("SaaS.Veterinario.Tests", jwt.Issuer);
        Assert.Contains(jwt.Claims, c => c.Type == JwtRegisteredClaimNames.Email && c.Value == correo);
        Assert.Contains(jwt.Claims, c => c.Type == JwtRegisteredClaimNames.Sub);
        Assert.Contains(jwt.Claims, c => c.Type == JwtRegisteredClaimNames.Jti);

        var hashEsperado = Hashear(tokens.RefreshToken);
        await using var contexto = postgres.CrearDbContext();
        var filaGuardada = await contexto.RefreshTokens.SingleAsync(t => t.TokenHash == hashEsperado);
        Assert.NotEqual(tokens.RefreshToken, filaGuardada.TokenHash);
    }

    [SkippableFact]
    public async Task EndpointProtegido_SinToken_Retorna401()
    {
        Skip.IfNot(postgres.EstaDisponible, MotivoOmision());

        using var cliente = factory.CreateClient();

        var respuesta = await cliente.GetAsync("/api/v1/usuarios/me");

        Assert.Equal(HttpStatusCode.Unauthorized, respuesta.StatusCode);
    }

    [SkippableFact]
    public async Task UsuariosMe_ConTokenValido_RetornaDatosDelUsuarioAutenticado()
    {
        Skip.IfNot(postgres.EstaDisponible, MotivoOmision());

        using var cliente = factory.CreateClient();
        var (tokens, correo) = await RegistrarConfirmarYLoginearAsync(cliente);
        cliente.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokens.AccessToken);

        var respuesta = await cliente.GetAsync("/api/v1/usuarios/me");

        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
        var usuario = await respuesta.Content.ReadFromJsonAsync<UsuarioActualResponse>();
        Assert.Equal(correo, usuario!.Correo);
        Assert.Equal(nameof(EstadoUsuario.Activo), usuario.Estado);
    }

    [SkippableFact]
    public async Task RenovarToken_ConTokenValido_RotaYRevocaElAnterior()
    {
        Skip.IfNot(postgres.EstaDisponible, MotivoOmision());

        using var cliente = factory.CreateClient();
        var (tokens, _) = await RegistrarConfirmarYLoginearAsync(cliente);

        var respuestaRenovar = await cliente.PostAsJsonAsync(
            "/api/v1/autenticacion/renovar",
            new RenovarTokenRequest(tokens.RefreshToken, null));

        Assert.Equal(HttpStatusCode.OK, respuestaRenovar.StatusCode);
        var nuevosTokens = await respuestaRenovar.Content.ReadFromJsonAsync<TokenResponse>();
        Assert.NotEqual(tokens.RefreshToken, nuevosTokens!.RefreshToken);

        var hashAnterior = Hashear(tokens.RefreshToken);
        await using (var contexto = postgres.CrearDbContext())
        {
            var filaAnterior = await contexto.RefreshTokens.SingleAsync(t => t.TokenHash == hashAnterior);
            Assert.NotNull(filaAnterior.FechaRevocacion);
        }

        var respuestaReutilizacion = await cliente.PostAsJsonAsync(
            "/api/v1/autenticacion/renovar",
            new RenovarTokenRequest(tokens.RefreshToken, null));

        Assert.Equal(HttpStatusCode.Unauthorized, respuestaReutilizacion.StatusCode);
    }

    [SkippableFact]
    public async Task RenovarToken_Expirado_Retorna401()
    {
        Skip.IfNot(postgres.EstaDisponible, MotivoOmision());

        using var cliente = factory.CreateClient();
        var (tokens, _) = await RegistrarConfirmarYLoginearAsync(cliente);

        var hash = Hashear(tokens.RefreshToken);
        await using (var contexto = postgres.CrearDbContext())
        {
            await contexto.Database.ExecuteSqlInterpolatedAsync(
                $"UPDATE refresh_tokens SET fecha_expiracion = now() - interval '1 day' WHERE token_hash = {hash}");
        }

        var respuesta = await cliente.PostAsJsonAsync(
            "/api/v1/autenticacion/renovar",
            new RenovarTokenRequest(tokens.RefreshToken, null));

        Assert.Equal(HttpStatusCode.Unauthorized, respuesta.StatusCode);
    }

    [SkippableFact]
    public async Task CerrarSesion_RevocaElRefreshToken()
    {
        Skip.IfNot(postgres.EstaDisponible, MotivoOmision());

        using var cliente = factory.CreateClient();
        var (tokens, _) = await RegistrarConfirmarYLoginearAsync(cliente);

        var respuestaCierre = await cliente.PostAsJsonAsync(
            "/api/v1/autenticacion/cerrar-sesion",
            new CerrarSesionRequest(tokens.RefreshToken));
        Assert.Equal(HttpStatusCode.NoContent, respuestaCierre.StatusCode);

        var respuestaRenovar = await cliente.PostAsJsonAsync(
            "/api/v1/autenticacion/renovar",
            new RenovarTokenRequest(tokens.RefreshToken, null));

        Assert.Equal(HttpStatusCode.Unauthorized, respuestaRenovar.StatusCode);
    }

    [SkippableFact]
    public async Task CerrarTodasLasSesiones_RevocaTodosLosRefreshTokensDelUsuario()
    {
        Skip.IfNot(postgres.EstaDisponible, MotivoOmision());

        using var cliente = factory.CreateClient();
        var (usuarioId, correo) = await RegistrarAsync(cliente);
        await ConfirmarCorreoAsync(cliente, usuarioId);

        var respuestaLogin1 = await cliente.PostAsJsonAsync(
            "/api/v1/autenticacion/iniciar-sesion",
            new IniciarSesionRequest(correo, Password, "dispositivo-1"));
        var tokens1 = (await respuestaLogin1.Content.ReadFromJsonAsync<TokenResponse>())!;

        var respuestaLogin2 = await cliente.PostAsJsonAsync(
            "/api/v1/autenticacion/iniciar-sesion",
            new IniciarSesionRequest(correo, Password, "dispositivo-2"));
        var tokens2 = (await respuestaLogin2.Content.ReadFromJsonAsync<TokenResponse>())!;

        cliente.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokens1.AccessToken);
        var respuestaCierre = await cliente.PostAsync("/api/v1/autenticacion/cerrar-todas-las-sesiones", null);
        Assert.Equal(HttpStatusCode.NoContent, respuestaCierre.StatusCode);
        cliente.DefaultRequestHeaders.Authorization = null;

        var respuestaRenovar1 = await cliente.PostAsJsonAsync("/api/v1/autenticacion/renovar", new RenovarTokenRequest(tokens1.RefreshToken, null));
        var respuestaRenovar2 = await cliente.PostAsJsonAsync("/api/v1/autenticacion/renovar", new RenovarTokenRequest(tokens2.RefreshToken, null));

        Assert.Equal(HttpStatusCode.Unauthorized, respuestaRenovar1.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, respuestaRenovar2.StatusCode);
    }

    [SkippableFact]
    public async Task SolicitarRecuperacion_ConCorreoInexistente_RetornaElMismoMensajeGenericoQueUnoExistente()
    {
        Skip.IfNot(postgres.EstaDisponible, MotivoOmision());

        using var cliente = factory.CreateClient();
        var (_, correoExistente) = await RegistrarAsync(cliente);

        var respuestaExistente = await cliente.PostAsJsonAsync(
            "/api/v1/autenticacion/solicitar-recuperacion",
            new SolicitarRecuperacionRequest(correoExistente));

        var respuestaInexistente = await cliente.PostAsJsonAsync(
            "/api/v1/autenticacion/solicitar-recuperacion",
            new SolicitarRecuperacionRequest(CorreoUnico()));

        Assert.Equal(HttpStatusCode.OK, respuestaExistente.StatusCode);
        Assert.Equal(HttpStatusCode.OK, respuestaInexistente.StatusCode);

        var mensajeExistente = await respuestaExistente.Content.ReadFromJsonAsync<MensajeResponse>();
        var mensajeInexistente = await respuestaInexistente.Content.ReadFromJsonAsync<MensajeResponse>();
        Assert.Equal(mensajeExistente!.Mensaje, mensajeInexistente!.Mensaje);
    }
}
