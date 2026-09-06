using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SaaS.Veterinario.Api.Autenticacion;
using SaaS.Veterinario.Api.Clientes;
using SaaS.Veterinario.Api.Mascotas;
using SaaS.Veterinario.Application.Abstracciones;
using SaaS.Veterinario.Domain.Mascotas;
using SaaS.Veterinario.Domain.Membresias;
using SaaS.Veterinario.Domain.Veterinarias;
using SaaS.Veterinario.IntegrationTests.Infraestructura;

namespace SaaS.Veterinario.IntegrationTests.Api;

/// <summary>
/// Cubre la Etapa 6: Clientes + Mascotas + relaciones por veterinaria + aislamiento
/// multi-tenant. Invariantes clave bajo prueba: Mascota es identidad GLOBAL (R01/R02) y su
/// unica relacion con una veterinaria vive en MascotaVeterinaria (R04); encontrar una Mascota
/// global nunca debe filtrar veterinarias, fichas, clientes ni historial ajenos (R06).
///
/// Reutiliza DOS veterinarias (A y B) ya autenticadas en todos los tests, en vez de registrar
/// un usuario nuevo por test: cada registro/login pasa por el limitador de velocidad
/// "autenticacion-sensible" (Etapa 4), y con ~17 tests haciendo 2 registros cada uno se agota
/// el limite configurado para pruebas. xUnit crea una instancia NUEVA de esta clase por cada
/// [SkippableFact] (IClassFixture solo comparte SaaSVeterinarioApiFactory/PostgresContainerFixture,
/// no la clase de prueba en si), asi que la memoizacion de los dos clientes se hace en un campo
/// STATIC protegido por SemaphoreSlim -- se registran una sola vez para toda la clase, sin
/// importar cuantos metodos de prueba se ejecuten. Las aserciones de aislamiento por lo tanto
/// verifican presencia/ausencia de elementos identificados por nombre unico, nunca conteos
/// exactos, ya que las veterinarias compartidas acumulan datos de todos los tests.
/// </summary>
[Collection(ColeccionPostgres.Nombre)]
public sealed class MascotasClientesEndpointsTests : IClassFixture<SaaSVeterinarioApiFactory>
{
    private const string Password = "password123";

    private static readonly SemaphoreSlim InicializacionLock = new(1, 1);
    private static (HttpClient ClienteA, Guid VeterinariaAId, HttpClient ClienteB, Guid VeterinariaBId)? _compartido;

    private readonly PostgresContainerFixture postgres;
    private readonly SaaSVeterinarioApiFactory factory;

    public MascotasClientesEndpointsTests(PostgresContainerFixture postgres, SaaSVeterinarioApiFactory factory)
    {
        this.postgres = postgres;
        this.factory = factory;
        factory.UsarPostgres(postgres);
    }

    private HttpClient clienteA = null!;
    private HttpClient clienteB = null!;
    private Guid veterinariaAId;
    private Guid veterinariaBId;

    private async Task AsegurarClientesCompartidosAsync()
    {
        if (_compartido is null)
        {
            await InicializacionLock.WaitAsync();
            try
            {
                if (_compartido is null)
                {
                    var a = await ClienteAutenticadoConVeterinariaAsync("ADMINISTRADOR_VETERINARIA");
                    var b = await ClienteAutenticadoConVeterinariaAsync("ADMINISTRADOR_VETERINARIA");
                    _compartido = (a.Cliente, a.VeterinariaId, b.Cliente, b.VeterinariaId);
                }
            }
            finally
            {
                InicializacionLock.Release();
            }
        }

        (clienteA, veterinariaAId, clienteB, veterinariaBId) = _compartido!.Value;
    }

    private string MotivoOmision() => $"Docker no disponible en este entorno: {postgres.MotivoNoDisponible}";

    private static string CorreoUnico() => $"prueba.mc.{Guid.NewGuid():N}@correo.com";

    private static string NombreUnico(string prefijo) => $"{prefijo}-{Guid.NewGuid():N}"[..30];

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

    private async Task<TokenResponse> LoginAsync(HttpClient cliente, string correo)
    {
        var respuesta = await cliente.PostAsJsonAsync(
            "/api/v1/autenticacion/iniciar-sesion",
            new IniciarSesionRequest(correo, Password, null));
        respuesta.EnsureSuccessStatusCode();
        return (await respuesta.Content.ReadFromJsonAsync<TokenResponse>())!;
    }

    private static HttpClient ConToken(HttpClient cliente, string accessToken)
    {
        cliente.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        return cliente;
    }

    /// <summary>Registra, confirma, crea Veterinaria+MembresiaVeterinaria ACTIVA con los roles indicados, e inicia sesión.</summary>
    private async Task<(HttpClient Cliente, Guid VeterinariaId)> ClienteAutenticadoConVeterinariaAsync(params string[] codigosRoles)
    {
        var cliente = factory.CreateClient();
        var (usuarioId, correo) = await RegistrarYConfirmarAsync(cliente);
        var veterinariaId = await CrearVeterinariaActivaAsync();
        await CrearMembresiaActivaAsync(usuarioId, veterinariaId, codigosRoles);
        var tokens = await LoginAsync(cliente, correo);
        return (ConToken(cliente, tokens.AccessToken), veterinariaId);
    }

    private async Task<Guid> ObtenerPrimeraEspecieIdAsync()
    {
        await using var contexto = postgres.CrearDbContext();
        return (await contexto.Especies.FirstAsync()).Id;
    }

    // ---------- 1. Creacion atomica de Mascota + MascotaVeterinaria ----------

    [SkippableFact]
    public async Task PostMascotas_ConDatosValidos_CreaMascotaYFichaAtomicamenteSinClienteNiResponsable()
    {
        Skip.IfNot(postgres.EstaDisponible, MotivoOmision());
        await AsegurarClientesCompartidosAsync();

        var especieId = await ObtenerPrimeraEspecieIdAsync();

        var respuesta = await clienteA!.PostAsJsonAsync(
            "/api/v1/mascotas",
            new RegistrarMascotaRequest(NombreUnico("Firulais"), especieId, null, "Macho", null, false, null, null, null, "HC-001"));

        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
        var cuerpo = await respuesta.Content.ReadFromJsonAsync<RegistrarMascotaResponse>();
        Assert.StartsWith("PET-", cuerpo!.CodigoPublico);

        await using var contexto = postgres.CrearDbContext();
        Assert.True(await contexto.Mascotas.AnyAsync(m => m.Id == cuerpo.MascotaId));
        Assert.True(await contexto.MascotasVeterinaria.AnyAsync(mv => mv.Id == cuerpo.MascotaVeterinariaId && mv.MascotaId == cuerpo.MascotaId));
        Assert.False(await contexto.ResponsablesMascota.AnyAsync(r => r.MascotaVeterinariaId == cuerpo.MascotaVeterinariaId));
    }

    // ---------- 2. Rollback real si la segunda escritura de la transaccion falla ----------

    [SkippableFact]
    public async Task RegistrarMascota_ConFalloPosteriorAAmbasEscrituras_HaceRollbackYNoDejaMascotaHuerfana()
    {
        Skip.IfNot(postgres.EstaDisponible, MotivoOmision());
        await AsegurarClientesCompartidosAsync();

        using var scope = factory.Services.CreateScope();
        var repositorioMascotas = scope.ServiceProvider.GetRequiredService<IRepositorioMascotas>();
        var repositorioMascotasVeterinarias = scope.ServiceProvider.GetRequiredService<IRepositorioMascotasVeterinarias>();
        var unidadDeTrabajo = scope.ServiceProvider.GetRequiredService<IUnidadDeTrabajo>();

        var especieId = await ObtenerPrimeraEspecieIdAsync();

        var mascota = Mascota.Crear($"PET-{Guid.NewGuid():N}"[..10].ToUpperInvariant(), "Firulais", especieId, null, SexoMascota.Macho, null, false, null, null, null);
        var segundaEscrituraFueExitosa = false;

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            unidadDeTrabajo.EjecutarEnTransaccionAsync(
                async ct =>
                {
                    await repositorioMascotas.AgregarAsync(mascota, ct);

                    var mascotaVeterinaria = MascotaVeterinaria.Crear(veterinariaAId, mascota.Id);
                    await repositorioMascotasVeterinarias.AgregarAsync(mascotaVeterinaria, ct);
                    segundaEscrituraFueExitosa = true;

                    throw new InvalidOperationException("Fallo simulado posterior a ambas escrituras.");
                },
                CancellationToken.None));

        Assert.True(segundaEscrituraFueExitosa);

        await using var contexto = postgres.CrearDbContext();
        Assert.False(await contexto.Mascotas.AnyAsync(m => m.Id == mascota.Id));
        Assert.False(await contexto.MascotasVeterinaria.AnyAsync(mv => mv.MascotaId == mascota.Id));
    }

    // ---------- 3. Misma Mascota global vinculable a dos veterinarias distintas ----------

    [SkippableFact]
    public async Task VincularMascota_MismaMascotaGlobal_PuedeVincularseADosVeterinariasDistintasSinMezclarDatos()
    {
        Skip.IfNot(postgres.EstaDisponible, MotivoOmision());
        await AsegurarClientesCompartidosAsync();

        var especieId = await ObtenerPrimeraEspecieIdAsync();

        var respuestaCrear = await clienteA!.PostAsJsonAsync(
            "/api/v1/mascotas",
            new RegistrarMascotaRequest(NombreUnico("Firulais"), especieId, null, "Macho", null, false, null, null, null, "HC-A-001"));
        respuestaCrear.EnsureSuccessStatusCode();
        var creada = (await respuestaCrear.Content.ReadFromJsonAsync<RegistrarMascotaResponse>())!;

        var respuestaVincular = await clienteB!.PostAsJsonAsync(
            "/api/v1/mascotas/vincular",
            new VincularMascotaRequest(creada.CodigoPublico, "HC-B-777"));

        Assert.Equal(HttpStatusCode.OK, respuestaVincular.StatusCode);
        var vinculada = (await respuestaVincular.Content.ReadFromJsonAsync<VincularMascotaResponse>())!;
        Assert.NotEqual(creada.MascotaVeterinariaId, vinculada.MascotaVeterinariaId);

        await using var contexto = postgres.CrearDbContext();
        Assert.Equal(2, await contexto.MascotasVeterinaria.CountAsync(mv => mv.MascotaId == creada.MascotaId));
        var fichaA = await contexto.MascotasVeterinaria.SingleAsync(mv => mv.Id == creada.MascotaVeterinariaId);
        var fichaB = await contexto.MascotasVeterinaria.SingleAsync(mv => mv.Id == vinculada.MascotaVeterinariaId);
        Assert.Equal("HC-A-001", fichaA.NumeroHistoria);
        Assert.Equal("HC-B-777", fichaB.NumeroHistoria);
    }

    // ---------- 4. Vinculo duplicado en la misma clinica rechazado ----------

    [SkippableFact]
    public async Task VincularMascota_YaVinculadaEnLaMismaVeterinaria_Retorna409()
    {
        Skip.IfNot(postgres.EstaDisponible, MotivoOmision());
        await AsegurarClientesCompartidosAsync();

        var especieId = await ObtenerPrimeraEspecieIdAsync();

        var respuestaCrear = await clienteA!.PostAsJsonAsync(
            "/api/v1/mascotas",
            new RegistrarMascotaRequest(NombreUnico("Firulais"), especieId, null, "Macho", null, false, null, null, null, null));
        var creada = (await respuestaCrear.Content.ReadFromJsonAsync<RegistrarMascotaResponse>())!;

        var respuestaDuplicada = await clienteA.PostAsJsonAsync(
            "/api/v1/mascotas/vincular",
            new VincularMascotaRequest(creada.CodigoPublico, null));

        Assert.Equal(HttpStatusCode.Conflict, respuestaDuplicada.StatusCode);
        var problema = await respuestaDuplicada.Content.ReadFromJsonAsync<Dictionary<string, object>>();
        Assert.Equal("MASCOTA_YA_REGISTRADA_EN_VETERINARIA", problema!["codigo"].ToString());
    }

    // ---------- 5/6. Cada veterinaria lista solo sus propias fichas ----------

    [SkippableFact]
    public async Task GetMascotas_ListaSoloFichasDeLaPropiaVeterinaria()
    {
        Skip.IfNot(postgres.EstaDisponible, MotivoOmision());
        await AsegurarClientesCompartidosAsync();

        var especieId = await ObtenerPrimeraEspecieIdAsync();
        var nombreDeA = NombreUnico("DeA");
        var nombreDeB = NombreUnico("DeB");

        await clienteA!.PostAsJsonAsync("/api/v1/mascotas", new RegistrarMascotaRequest(nombreDeA, especieId, null, "Macho", null, false, null, null, null, null));
        await clienteB!.PostAsJsonAsync("/api/v1/mascotas", new RegistrarMascotaRequest(nombreDeB, especieId, null, "Hembra", null, false, null, null, null, null));

        var listaA = await (await clienteA.GetAsync("/api/v1/mascotas")).Content.ReadFromJsonAsync<List<MascotaFichaResponse>>();
        var listaB = await (await clienteB.GetAsync("/api/v1/mascotas")).Content.ReadFromJsonAsync<List<MascotaFichaResponse>>();

        Assert.Contains(listaA!, f => f.Nombre == nombreDeA);
        Assert.DoesNotContain(listaA!, f => f.Nombre == nombreDeB);
        Assert.Contains(listaB!, f => f.Nombre == nombreDeB);
        Assert.DoesNotContain(listaB!, f => f.Nombre == nombreDeA);
    }

    // ---------- 7. Ficha de otra veterinaria por UUID nunca es visible (404) ----------
    // Esta es la prueba critica nombrada "VeterinariaA_NoPuedeConsultarFichaMascotaDeVeterinariaB".

    [SkippableFact]
    public async Task VeterinariaA_NoPuedeConsultarFichaMascotaDeVeterinariaB()
    {
        Skip.IfNot(postgres.EstaDisponible, MotivoOmision());
        await AsegurarClientesCompartidosAsync();

        var especieId = await ObtenerPrimeraEspecieIdAsync();

        var respuestaB = await clienteB!.PostAsJsonAsync(
            "/api/v1/mascotas",
            new RegistrarMascotaRequest(NombreUnico("DeB"), especieId, null, "Hembra", null, false, null, null, null, "HISTORIA-SECRETA-B"));
        var creadaB = (await respuestaB.Content.ReadFromJsonAsync<RegistrarMascotaResponse>())!;

        var respuestaDesdeA = await clienteA!.GetAsync($"/api/v1/mascotas/{creadaB.MascotaVeterinariaId}");

        Assert.Equal(HttpStatusCode.NotFound, respuestaDesdeA.StatusCode);
    }

    // ---------- 8. Busqueda global por CodigoPublico ----------

    [SkippableFact]
    public async Task BuscarMascota_PorCodigoPublico_LaEncuentra()
    {
        Skip.IfNot(postgres.EstaDisponible, MotivoOmision());
        await AsegurarClientesCompartidosAsync();

        var especieId = await ObtenerPrimeraEspecieIdAsync();
        var nombre = NombreUnico("Firulais");

        var respuestaCrear = await clienteA!.PostAsJsonAsync(
            "/api/v1/mascotas",
            new RegistrarMascotaRequest(nombre, especieId, null, "Macho", null, false, null, null, null, null));
        var creada = (await respuestaCrear.Content.ReadFromJsonAsync<RegistrarMascotaResponse>())!;

        var respuestaBuscar = await clienteA.GetAsync($"/api/v1/mascotas/buscar?codigoPublico={creada.CodigoPublico}");

        Assert.Equal(HttpStatusCode.OK, respuestaBuscar.StatusCode);
        var encontrada = await respuestaBuscar.Content.ReadFromJsonAsync<MascotaEncontradaResponse>();
        Assert.Equal(nombre, encontrada!.Nombre);
    }

    // ---------- 9. Prueba critica: buscar no revela veterinarias ni historial previo ----------

    [SkippableFact]
    public async Task BuscarMascotaExistente_NoRevelaVeterinariasNiHistorialPrevio()
    {
        Skip.IfNot(postgres.EstaDisponible, MotivoOmision());
        await AsegurarClientesCompartidosAsync();

        var especieId = await ObtenerPrimeraEspecieIdAsync();
        var microchipSecreto = $"CHIP-SECRETO-{Guid.NewGuid():N}"[..20];
        var historiaSecreta = $"HIST-A-{Guid.NewGuid():N}"[..20]; // numero_historia tiene HasMaxLength(50)

        var respuestaCrear = await clienteA!.PostAsJsonAsync(
            "/api/v1/mascotas",
            new RegistrarMascotaRequest(NombreUnico("Firulais"), especieId, null, "Macho", null, false, null, microchipSecreto, null, historiaSecreta));
        respuestaCrear.EnsureSuccessStatusCode();
        var creada = (await respuestaCrear.Content.ReadFromJsonAsync<RegistrarMascotaResponse>())!;

        // B busca por CodigoPublico (el unico dato que puede conocer sin haber tratado antes a la mascota).
        var respuestaBuscar = await clienteB!.GetAsync($"/api/v1/mascotas/buscar?codigoPublico={creada.CodigoPublico}");
        Assert.Equal(HttpStatusCode.OK, respuestaBuscar.StatusCode);

        var cuerpoCrudo = await respuestaBuscar.Content.ReadAsStringAsync();
        Assert.DoesNotContain(historiaSecreta, cuerpoCrudo);
        Assert.DoesNotContain(microchipSecreto, cuerpoCrudo);
        Assert.DoesNotContain(veterinariaAId.ToString(), cuerpoCrudo);
        Assert.DoesNotContain(creada.MascotaVeterinariaId.ToString(), cuerpoCrudo);

        var encontrada = await respuestaBuscar.Content.ReadFromJsonAsync<MascotaEncontradaResponse>();
        Assert.Null(encontrada!.NumeroMicrochip);

        // B todavia no tiene vinculo -- la busqueda en si no crea ni revela ninguna ficha para esa mascota.
        var respuestaListaB = await clienteB.GetAsync("/api/v1/mascotas");
        var listaB = await respuestaListaB.Content.ReadFromJsonAsync<List<MascotaFichaResponse>>();
        Assert.DoesNotContain(listaB!, f => f.MascotaId == creada.MascotaId);
    }

    // ---------- 10. Microchip global duplicado rechazado ----------

    [SkippableFact]
    public async Task PostMascotas_ConMicrochipDuplicado_Retorna409()
    {
        Skip.IfNot(postgres.EstaDisponible, MotivoOmision());
        await AsegurarClientesCompartidosAsync();

        var especieId = await ObtenerPrimeraEspecieIdAsync();
        var microchip = $"CHIP-{Guid.NewGuid():N}"[..15];

        var primeraRespuesta = await clienteA!.PostAsJsonAsync(
            "/api/v1/mascotas",
            new RegistrarMascotaRequest(NombreUnico("Firulais"), especieId, null, "Macho", null, false, null, microchip, null, null));
        primeraRespuesta.EnsureSuccessStatusCode();

        var segundaRespuesta = await clienteB!.PostAsJsonAsync(
            "/api/v1/mascotas",
            new RegistrarMascotaRequest(NombreUnico("OtroNombre"), especieId, null, "Hembra", null, false, null, microchip, null, null));

        Assert.Equal(HttpStatusCode.Conflict, segundaRespuesta.StatusCode);
        var problema = await segundaRespuesta.Content.ReadFromJsonAsync<Dictionary<string, object>>();
        Assert.Equal("MASCOTA_MICROCHIP_DUPLICADO", problema!["codigo"].ToString());
    }

    // ---------- 11/12. Clientes: aislamiento por veterinaria ----------

    [SkippableFact]
    public async Task GetClientes_ListaSoloClientesDeLaPropiaVeterinariaYNoExigeCorreoUnico()
    {
        Skip.IfNot(postgres.EstaDisponible, MotivoOmision());
        await AsegurarClientesCompartidosAsync();

        var correoCompartido = CorreoUnico();
        var nombreDeA = NombreUnico("ClienteDeA");
        var nombreDeB = NombreUnico("ClienteDeB");

        var respuestaA = await clienteA!.PostAsJsonAsync("/api/v1/clientes", new RegistrarClienteRequest(nombreDeA, correoCompartido, null, null));
        var respuestaB = await clienteB!.PostAsJsonAsync("/api/v1/clientes", new RegistrarClienteRequest(nombreDeB, correoCompartido, null, null));
        respuestaA.EnsureSuccessStatusCode();
        respuestaB.EnsureSuccessStatusCode();

        var listaA = await (await clienteA.GetAsync("/api/v1/clientes")).Content.ReadFromJsonAsync<List<ClienteVeterinariaResponse>>();
        var listaB = await (await clienteB.GetAsync("/api/v1/clientes")).Content.ReadFromJsonAsync<List<ClienteVeterinariaResponse>>();

        Assert.Contains(listaA!, c => c.Nombre == nombreDeA);
        Assert.DoesNotContain(listaA!, c => c.Nombre == nombreDeB);
        Assert.Contains(listaB!, c => c.Nombre == nombreDeB);
        Assert.DoesNotContain(listaB!, c => c.Nombre == nombreDeA);
    }

    [SkippableFact]
    public async Task GetClientePorId_ConIdDeOtraVeterinaria_Retorna404()
    {
        Skip.IfNot(postgres.EstaDisponible, MotivoOmision());
        await AsegurarClientesCompartidosAsync();

        var respuestaCrear = await clienteB!.PostAsJsonAsync("/api/v1/clientes", new RegistrarClienteRequest(NombreUnico("ClienteDeB"), null, null, null));
        var creado = (await respuestaCrear.Content.ReadFromJsonAsync<RegistrarClienteResponse>())!;

        var respuestaDesdeA = await clienteA!.GetAsync($"/api/v1/clientes/{creado.ClienteId}");

        Assert.Equal(HttpStatusCode.NotFound, respuestaDesdeA.StatusCode);
    }

    // ---------- 13. Un solo responsable principal activo por MascotaVeterinaria ----------

    [SkippableFact]
    public async Task PostResponsables_SegundoPrincipalParaLaMismaFicha_Retorna409()
    {
        Skip.IfNot(postgres.EstaDisponible, MotivoOmision());
        await AsegurarClientesCompartidosAsync();

        var especieId = await ObtenerPrimeraEspecieIdAsync();

        var respuestaMascota = await clienteA!.PostAsJsonAsync(
            "/api/v1/mascotas",
            new RegistrarMascotaRequest(NombreUnico("Firulais"), especieId, null, "Macho", null, false, null, null, null, null));
        var mascota = (await respuestaMascota.Content.ReadFromJsonAsync<RegistrarMascotaResponse>())!;

        var primerResponsable = await clienteA.PostAsJsonAsync(
            $"/api/v1/mascotas/{mascota.MascotaVeterinariaId}/responsables",
            new RegistrarResponsableRequest(null, "Responsable 1", null, null, "Propietario", true, true, true));
        primerResponsable.EnsureSuccessStatusCode();

        var segundoResponsable = await clienteA.PostAsJsonAsync(
            $"/api/v1/mascotas/{mascota.MascotaVeterinariaId}/responsables",
            new RegistrarResponsableRequest(null, "Responsable 2", null, null, "Familiar", true, true, true));

        Assert.Equal(HttpStatusCode.Conflict, segundoResponsable.StatusCode);
        var problema = await segundoResponsable.Content.ReadFromJsonAsync<Dictionary<string, object>>();
        Assert.Equal("RESPONSABLE_PRINCIPAL_YA_EXISTE", problema!["codigo"].ToString());
    }

    // ---------- 14. Responsables de una ficha ajena nunca son visibles ----------

    [SkippableFact]
    public async Task GetResponsables_DeFichaDeOtraVeterinaria_NuncaSonVisibles()
    {
        Skip.IfNot(postgres.EstaDisponible, MotivoOmision());
        await AsegurarClientesCompartidosAsync();

        var especieId = await ObtenerPrimeraEspecieIdAsync();

        var respuestaMascotaB = await clienteB!.PostAsJsonAsync(
            "/api/v1/mascotas",
            new RegistrarMascotaRequest(NombreUnico("DeB"), especieId, null, "Hembra", null, false, null, null, null, null));
        var mascotaB = (await respuestaMascotaB.Content.ReadFromJsonAsync<RegistrarMascotaResponse>())!;

        await clienteB.PostAsJsonAsync(
            $"/api/v1/mascotas/{mascotaB.MascotaVeterinariaId}/responsables",
            new RegistrarResponsableRequest(null, "Responsable de B", null, null, "Propietario", true, true, true));

        // A intenta leer los responsables de la ficha de B usando el mismo UUID: nunca debe ver nada.
        var respuestaDesdeA = await clienteA!.GetAsync($"/api/v1/mascotas/{mascotaB.MascotaVeterinariaId}/responsables");

        Assert.Equal(HttpStatusCode.OK, respuestaDesdeA.StatusCode);
        var responsablesVistosPorA = await respuestaDesdeA.Content.ReadFromJsonAsync<List<ResponsableMascotaResponse>>();
        Assert.Empty(responsablesVistosPorA!);
    }

    // ---------- 15. Responsable con Cliente de otra veterinaria rechazado ----------

    [SkippableFact]
    public async Task PostResponsables_ConClienteDeOtraVeterinaria_Retorna400()
    {
        Skip.IfNot(postgres.EstaDisponible, MotivoOmision());
        await AsegurarClientesCompartidosAsync();

        var especieId = await ObtenerPrimeraEspecieIdAsync();

        var respuestaMascotaA = await clienteA!.PostAsJsonAsync(
            "/api/v1/mascotas",
            new RegistrarMascotaRequest(NombreUnico("DeA"), especieId, null, "Macho", null, false, null, null, null, null));
        var mascotaA = (await respuestaMascotaA.Content.ReadFromJsonAsync<RegistrarMascotaResponse>())!;

        var respuestaClienteB = await clienteB!.PostAsJsonAsync("/api/v1/clientes", new RegistrarClienteRequest(NombreUnico("ClienteDeB"), null, null, null));
        var clienteDeB = (await respuestaClienteB.Content.ReadFromJsonAsync<RegistrarClienteResponse>())!;

        var respuestaResponsable = await clienteA.PostAsJsonAsync(
            $"/api/v1/mascotas/{mascotaA.MascotaVeterinariaId}/responsables",
            new RegistrarResponsableRequest(clienteDeB.ClienteId, "Responsable", null, null, "Propietario", false, true, true));

        Assert.Equal(HttpStatusCode.BadRequest, respuestaResponsable.StatusCode);
        var problema = await respuestaResponsable.Content.ReadFromJsonAsync<Dictionary<string, object>>();
        Assert.Equal("CLIENTE_INVALIDO", problema!["codigo"].ToString());
    }

    // ---------- 16. Responsable nace NO_VINCULADO y sin UsuarioId (seccion 25) ----------

    [SkippableFact]
    public async Task PostResponsables_ConDatosValidos_NaceSinUsuarioVinculado()
    {
        Skip.IfNot(postgres.EstaDisponible, MotivoOmision());
        await AsegurarClientesCompartidosAsync();

        var especieId = await ObtenerPrimeraEspecieIdAsync();

        var respuestaMascota = await clienteA!.PostAsJsonAsync(
            "/api/v1/mascotas",
            new RegistrarMascotaRequest(NombreUnico("Firulais"), especieId, null, "Macho", null, false, null, null, null, null));
        var mascota = (await respuestaMascota.Content.ReadFromJsonAsync<RegistrarMascotaResponse>())!;

        var respuestaResponsable = await clienteA.PostAsJsonAsync(
            $"/api/v1/mascotas/{mascota.MascotaVeterinariaId}/responsables",
            new RegistrarResponsableRequest(null, "Ana Perez", null, null, "Propietario", false, true, false));
        respuestaResponsable.EnsureSuccessStatusCode();
        var creado = (await respuestaResponsable.Content.ReadFromJsonAsync<RegistrarResponsableResponse>())!;

        await using var contexto = postgres.CrearDbContext();
        var responsable = await contexto.ResponsablesMascota.SingleAsync(r => r.Id == creado.ResponsableId);
        Assert.Null(responsable.UsuarioId);
        Assert.Equal(EstadoVinculacionResponsable.NoVinculado, responsable.EstadoVinculacion);
    }

    // ---------- 17. Falta de permiso: mascotas.crear requerido para registrar ----------

    [SkippableFact]
    public async Task PostMascotas_SinPermisoMascotasCrear_Retorna403()
    {
        Skip.IfNot(postgres.EstaDisponible, MotivoOmision());
        await AsegurarClientesCompartidosAsync();

        using var clienteSinRoles = factory.CreateClient();
        var (usuarioId, correo) = await RegistrarYConfirmarAsync(clienteSinRoles);
        var veterinariaId = await CrearVeterinariaActivaAsync();
        await CrearMembresiaActivaAsync(usuarioId, veterinariaId); // sin roles, por lo tanto sin permisos
        var tokens = await LoginAsync(clienteSinRoles, correo);
        ConToken(clienteSinRoles, tokens.AccessToken);

        var especieId = await ObtenerPrimeraEspecieIdAsync();

        var respuesta = await clienteSinRoles.PostAsJsonAsync(
            "/api/v1/mascotas",
            new RegistrarMascotaRequest(NombreUnico("Firulais"), especieId, null, "Macho", null, false, null, null, null, null));

        Assert.Equal(HttpStatusCode.Forbidden, respuesta.StatusCode);
        var problema = await respuesta.Content.ReadFromJsonAsync<Dictionary<string, object>>();
        Assert.Equal("AUTORIZACION_PERMISO_REQUERIDO", problema!["codigo"].ToString());
    }
}
