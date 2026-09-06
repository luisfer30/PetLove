using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SaaS.Veterinario.Api.Autenticacion;
using SaaS.Veterinario.Api.Consultas;
using SaaS.Veterinario.Api.Mascotas;
using SaaS.Veterinario.Api.Seguimientos;
using SaaS.Veterinario.Api.Tratamientos;
using SaaS.Veterinario.Application.Abstracciones;
using SaaS.Veterinario.Domain.Membresias;
using SaaS.Veterinario.Domain.Tratamientos;
using SaaS.Veterinario.Domain.Veterinarias;
using SaaS.Veterinario.IntegrationTests.Infraestructura;

namespace SaaS.Veterinario.IntegrationTests.Api;

/// <summary>
/// Cubre la Etapa 8: Tratamientos + Items + Programaciones + Seguimiento clinico.
///
/// Reutiliza DOS usuarios (A y B) con roles ADMINISTRADOR_VETERINARIA + VETERINARIO, memoizados
/// en un campo STATIC -- mismo patron y misma razon que en CitasConsultasEndpointsTests: xUnit
/// crea una instancia nueva de la clase por cada [SkippableFact], y registrar un usuario nuevo
/// por test agotaria el limitador de velocidad de autenticacion. Ambas veterinarias de prueba
/// se crean con ZonaHoraria "America/Guayaquil" (mismo helper reutilizado de Etapa 7).
/// </summary>
[Collection(ColeccionPostgres.Nombre)]
public sealed class TratamientosEndpointsTests : IClassFixture<SaaSVeterinarioApiFactory>
{
    private const string Password = "password123";

    private static readonly SemaphoreSlim InicializacionLock = new(1, 1);
    private static (HttpClient ClienteA, Guid VeterinariaAId, Guid UsuarioAId, HttpClient ClienteB, Guid VeterinariaBId, Guid UsuarioBId)? _compartido;

    private readonly PostgresContainerFixture postgres;
    private readonly SaaSVeterinarioApiFactory factory;

    public TratamientosEndpointsTests(PostgresContainerFixture postgres, SaaSVeterinarioApiFactory factory)
    {
        this.postgres = postgres;
        this.factory = factory;
        factory.UsarPostgres(postgres);
    }

    private HttpClient clienteA = null!;
    private HttpClient clienteB = null!;
    private Guid veterinariaAId;
    private Guid usuarioAId;
    private Guid veterinariaBId;
    private Guid usuarioBId;

    private async Task AsegurarClientesCompartidosAsync()
    {
        if (_compartido is null)
        {
            await InicializacionLock.WaitAsync();
            try
            {
                if (_compartido is null)
                {
                    var a = await ClienteAutenticadoConVeterinariaAsync("ADMINISTRADOR_VETERINARIA", "VETERINARIO");
                    var b = await ClienteAutenticadoConVeterinariaAsync("ADMINISTRADOR_VETERINARIA", "VETERINARIO");
                    _compartido = (a.Cliente, a.VeterinariaId, a.UsuarioId, b.Cliente, b.VeterinariaId, b.UsuarioId);
                }
            }
            finally
            {
                InicializacionLock.Release();
            }
        }

        (clienteA, veterinariaAId, usuarioAId, clienteB, veterinariaBId, usuarioBId) = _compartido!.Value;
    }

    private string MotivoOmision() => $"Docker no disponible en este entorno: {postgres.MotivoNoDisponible}";

    private static string CorreoUnico() => $"prueba.tto.{Guid.NewGuid():N}@correo.com";

    private static string NombreUnico(string prefijo) => $"{prefijo}-{Guid.NewGuid():N}"[..30];

    private static string GenerarCodigoPublico() => $"VET-{Guid.NewGuid():N}"[..10].ToUpperInvariant();

    private static int contadorHorario;

    private readonly DateTimeOffset Inicio = new(DateTime.UtcNow.Date.AddDays(Interlocked.Increment(ref contadorHorario)).AddHours(8), TimeSpan.Zero);

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

    private async Task<(HttpClient Cliente, Guid VeterinariaId, Guid UsuarioId)> ClienteAutenticadoConVeterinariaAsync(params string[] codigosRoles)
    {
        var cliente = factory.CreateClient();
        var (usuarioId, correo) = await RegistrarYConfirmarAsync(cliente);
        var veterinariaId = await CrearVeterinariaActivaAsync();
        await CrearMembresiaActivaAsync(usuarioId, veterinariaId, codigosRoles);
        var tokens = await LoginAsync(cliente, correo);
        return (ConToken(cliente, tokens.AccessToken), veterinariaId, usuarioId);
    }

    private async Task<Guid> CrearMascotaVeterinariaAsync(HttpClient cliente, string nombre)
    {
        await using var contexto = postgres.CrearDbContext();
        var especieId = (await contexto.Especies.FirstAsync()).Id;
        var respuesta = await cliente.PostAsJsonAsync(
            "/api/v1/mascotas",
            new RegistrarMascotaRequest(nombre, especieId, null, "Macho", null, false, null, null, null, null));
        respuesta.EnsureSuccessStatusCode();
        var creada = (await respuesta.Content.ReadFromJsonAsync<RegistrarMascotaResponse>())!;
        return creada.MascotaVeterinariaId;
    }

    private async Task<Guid> CrearPlanAsync(HttpClient cliente, Guid mascotaVeterinariaId, Guid? consultaId = null)
    {
        var respuesta = await cliente.PostAsJsonAsync(
            "/api/v1/tratamientos",
            new CrearPlanTratamientoRequest(mascotaVeterinariaId, consultaId, NombreUnico("Tratamiento"), null, Inicio, null));
        respuesta.EnsureSuccessStatusCode();
        var creado = (await respuesta.Content.ReadFromJsonAsync<CrearPlanTratamientoResponse>())!;
        return creado.PlanTratamientoId;
    }

    // ---------- 1. Crear plan para mascota propia ----------

    [SkippableFact]
    public async Task CrearPlan_ParaMascotaPropia_EsCreadoPendiente()
    {
        Skip.IfNot(postgres.EstaDisponible, MotivoOmision());
        await AsegurarClientesCompartidosAsync();

        var mascotaVeterinariaId = await CrearMascotaVeterinariaAsync(clienteA, NombreUnico("Firulais"));

        var respuesta = await clienteA.PostAsJsonAsync(
            "/api/v1/tratamientos", new CrearPlanTratamientoRequest(mascotaVeterinariaId, null, NombreUnico("Tratamiento"), "Analgésicos post-cirugía", Inicio, null));

        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
        var cuerpo = await respuesta.Content.ReadFromJsonAsync<CrearPlanTratamientoResponse>();
        Assert.Equal("Pendiente", cuerpo!.Estado);
    }

    // ---------- 2 / Prueba critica multi-tenant ----------

    [SkippableFact]
    public async Task VeterinariaA_NoPuedeConsultarTratamientoDeVeterinariaB()
    {
        Skip.IfNot(postgres.EstaDisponible, MotivoOmision());
        await AsegurarClientesCompartidosAsync();

        var mascotaDeB = await CrearMascotaVeterinariaAsync(clienteB, NombreUnico("DeB"));
        var planDeB = await CrearPlanAsync(clienteB, mascotaDeB);

        var respuestaGet = await clienteA.GetAsync($"/api/v1/tratamientos/{planDeB}");
        Assert.Equal(HttpStatusCode.NotFound, respuestaGet.StatusCode);

        var respuestaActivar = await clienteA.PostAsync($"/api/v1/tratamientos/{planDeB}/activar", null);
        Assert.Equal(HttpStatusCode.NotFound, respuestaActivar.StatusCode);
    }

    // ---------- 3/4. Crear plan desde consulta propia / rechazar consulta ajena ----------

    [SkippableFact]
    public async Task CrearPlan_DesdeConsultaPropia_EsCreado()
    {
        Skip.IfNot(postgres.EstaDisponible, MotivoOmision());
        await AsegurarClientesCompartidosAsync();

        var mascotaVeterinariaId = await CrearMascotaVeterinariaAsync(clienteA, NombreUnico("Firulais"));
        var respuestaConsulta = await clienteA.PostAsJsonAsync(
            "/api/v1/consultas",
            new CrearConsultaRequest(mascotaVeterinariaId, null, usuarioAId, DateTimeOffset.UtcNow, "Motivo", null, null, null, null, null, null, null));
        var consulta = (await respuestaConsulta.Content.ReadFromJsonAsync<CrearConsultaResponse>())!;

        var respuesta = await clienteA.PostAsJsonAsync(
            "/api/v1/tratamientos", new CrearPlanTratamientoRequest(mascotaVeterinariaId, consulta.ConsultaId, NombreUnico("Tratamiento"), null, Inicio, null));

        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
    }

    [SkippableFact]
    public async Task CrearPlan_ConConsultaDeOtroTenant_Retorna400()
    {
        Skip.IfNot(postgres.EstaDisponible, MotivoOmision());
        await AsegurarClientesCompartidosAsync();

        var mascotaDeB = await CrearMascotaVeterinariaAsync(clienteB, NombreUnico("DeB"));
        var respuestaConsultaB = await clienteB.PostAsJsonAsync(
            "/api/v1/consultas",
            new CrearConsultaRequest(mascotaDeB, null, usuarioBId, DateTimeOffset.UtcNow, "Motivo", null, null, null, null, null, null, null));
        var consultaB = (await respuestaConsultaB.Content.ReadFromJsonAsync<CrearConsultaResponse>())!;

        var mascotaDeA = await CrearMascotaVeterinariaAsync(clienteA, NombreUnico("DeA"));
        var respuesta = await clienteA.PostAsJsonAsync(
            "/api/v1/tratamientos", new CrearPlanTratamientoRequest(mascotaDeA, consultaB.ConsultaId, NombreUnico("Tratamiento"), null, Inicio, null));

        Assert.Equal(HttpStatusCode.BadRequest, respuesta.StatusCode);
        var problema = await respuesta.Content.ReadFromJsonAsync<Dictionary<string, object>>();
        Assert.Equal("TRATAMIENTO_CONSULTA_INVALIDA", problema!["codigo"].ToString());
    }

    // ---------- 5/6/7. Plan desde consulta BORRADOR: PENDIENTE, no activable, activable al finalizar ----------

    [SkippableFact]
    public async Task CrearPlanDesdeConsultaBorrador_QuedaPendienteYNoPuedeActivarseHastaFinalizar()
    {
        Skip.IfNot(postgres.EstaDisponible, MotivoOmision());
        await AsegurarClientesCompartidosAsync();

        var mascotaVeterinariaId = await CrearMascotaVeterinariaAsync(clienteA, NombreUnico("Firulais"));
        var respuestaConsulta = await clienteA.PostAsJsonAsync(
            "/api/v1/consultas",
            new CrearConsultaRequest(mascotaVeterinariaId, null, usuarioAId, DateTimeOffset.UtcNow, "Motivo", null, null, null, null, null, null, null));
        var consulta = (await respuestaConsulta.Content.ReadFromJsonAsync<CrearConsultaResponse>())!;
        Assert.Equal("Borrador", consulta.Estado);

        var respuestaPlan = await clienteA.PostAsJsonAsync(
            "/api/v1/tratamientos", new CrearPlanTratamientoRequest(mascotaVeterinariaId, consulta.ConsultaId, NombreUnico("Tratamiento"), null, Inicio, null));
        var plan = (await respuestaPlan.Content.ReadFromJsonAsync<CrearPlanTratamientoResponse>())!;
        Assert.Equal("Pendiente", plan.Estado);

        // Item 6: mientras la consulta sigue BORRADOR, activar el plan debe fallar.
        var respuestaActivarAntes = await clienteA.PostAsync($"/api/v1/tratamientos/{plan.PlanTratamientoId}/activar", null);
        Assert.Equal(HttpStatusCode.BadRequest, respuestaActivarAntes.StatusCode);
        var problema = await respuestaActivarAntes.Content.ReadFromJsonAsync<Dictionary<string, object>>();
        Assert.Equal("TRATAMIENTO_REQUIERE_CONSULTA_FINALIZADA", problema!["codigo"].ToString());

        // Item 7: al finalizar la consulta, activar el plan debe tener exito.
        (await clienteA.PostAsync($"/api/v1/consultas/{consulta.ConsultaId}/finalizar", null)).EnsureSuccessStatusCode();

        var respuestaActivarDespues = await clienteA.PostAsync($"/api/v1/tratamientos/{plan.PlanTratamientoId}/activar", null);
        Assert.Equal(HttpStatusCode.NoContent, respuestaActivarDespues.StatusCode);

        var planFinal = await (await clienteA.GetAsync($"/api/v1/tratamientos/{plan.PlanTratamientoId}")).Content.ReadFromJsonAsync<PlanTratamientoResponse>();
        Assert.Equal("Activo", planFinal!.Estado);
    }

    // ---------- 8/9. Activar plan genera programaciones con timestamps correctos ----------

    [SkippableFact]
    public async Task ActivarPlan_ConItemIntervaloHoras_GeneraProgramacionesConTimestampsCorrectos()
    {
        Skip.IfNot(postgres.EstaDisponible, MotivoOmision());
        await AsegurarClientesCompartidosAsync();

        var mascotaVeterinariaId = await CrearMascotaVeterinariaAsync(clienteA, NombreUnico("Firulais"));
        var planId = await CrearPlanAsync(clienteA, mascotaVeterinariaId);

        var respuestaItem = await clienteA.PostAsJsonAsync(
            $"/api/v1/tratamientos/{planId}/items",
            new AgregarItemTratamientoRequest(
                "Medicamento", "Amoxicilina", null, 1m, "tableta", "Oral", "IntervaloHoras", 8, null, [], Inicio, Inicio.AddDays(1), null, null, 0));
        respuestaItem.EnsureSuccessStatusCode();

        (await clienteA.PostAsync($"/api/v1/tratamientos/{planId}/activar", null)).EnsureSuccessStatusCode();

        var respuestaProgramaciones = await clienteA.GetAsync($"/api/v1/tratamientos/{planId}/programaciones");
        var programaciones = await respuestaProgramaciones.Content.ReadFromJsonAsync<List<ProgramacionTratamientoResponse>>();

        Assert.Equal(3, programaciones!.Count); // Inicio, +8h, +16h (fin exclusivo a las 24h)
        Assert.All(programaciones, p => Assert.Equal("Pendiente", p.Estado));
        Assert.Contains(programaciones, p => p.FechaHoraProgramada == Inicio);
        Assert.Contains(programaciones, p => p.FechaHoraProgramada == Inicio.AddHours(8));
    }

    // ---------- 10. Segun necesidad no genera programaciones ----------

    [SkippableFact]
    public async Task AgregarItemSegunNecesidad_NoGeneraProgramaciones()
    {
        Skip.IfNot(postgres.EstaDisponible, MotivoOmision());
        await AsegurarClientesCompartidosAsync();

        var mascotaVeterinariaId = await CrearMascotaVeterinariaAsync(clienteA, NombreUnico("Firulais"));
        var planId = await CrearPlanAsync(clienteA, mascotaVeterinariaId);

        var respuestaItem = await clienteA.PostAsJsonAsync(
            $"/api/v1/tratamientos/{planId}/items",
            new AgregarItemTratamientoRequest(
                "Medicamento", "Analgésico de rescate", null, null, null, "Oral", "SegunNecesidad", null, null, [], Inicio, null, null, "Solo si hay dolor", 0));
        respuestaItem.EnsureSuccessStatusCode();

        (await clienteA.PostAsync($"/api/v1/tratamientos/{planId}/activar", null)).EnsureSuccessStatusCode();

        var programaciones = await (await clienteA.GetAsync($"/api/v1/tratamientos/{planId}/programaciones")).Content.ReadFromJsonAsync<List<ProgramacionTratamientoResponse>>();
        Assert.Empty(programaciones!);
    }

    // ---------- 11. Horarios fijos multiples generan filas correctas ----------

    [SkippableFact]
    public async Task ActivarPlan_ConItemHoraFijaMultiplesHorarios_GeneraFilasCorrectas()
    {
        Skip.IfNot(postgres.EstaDisponible, MotivoOmision());
        await AsegurarClientesCompartidosAsync();

        var mascotaVeterinariaId = await CrearMascotaVeterinariaAsync(clienteA, NombreUnico("Firulais"));
        var planId = await CrearPlanAsync(clienteA, mascotaVeterinariaId);

        // Inicio esta anclado en offset UTC (+00:00), no en hora local de Guayaquil: para que
        // el rango cubra generosamente 2 dias completos de ocurrencias LOCALES (08:00/20:00
        // Guayaquil = 13:00/01:00 UTC) sin que el corte de fechaFin excluya alguna por el
        // desfase de -5h, se usa un rango de 2 dias completos.
        var horarios = new List<TimeOnly> { new(8, 0), new(20, 0) };
        var respuestaItem = await clienteA.PostAsJsonAsync(
            $"/api/v1/tratamientos/{planId}/items",
            new AgregarItemTratamientoRequest(
                "Cuidado", "Limpieza de herida", null, null, null, null, "HoraFija", null, null, horarios, Inicio, Inicio.AddDays(2), null, null, 0));
        var item = (await respuestaItem.Content.ReadFromJsonAsync<AgregarItemTratamientoResponse>())!;

        var itemsRespuesta = await (await clienteA.GetAsync($"/api/v1/tratamientos/{planId}/items")).Content.ReadFromJsonAsync<List<ItemTratamientoResponse>>();
        var itemCreado = itemsRespuesta!.Single(i => i.Id == item.ItemTratamientoId);
        Assert.Equal(2, itemCreado.Horarios.Count);

        (await clienteA.PostAsync($"/api/v1/tratamientos/{planId}/activar", null)).EnsureSuccessStatusCode();

        var programaciones = await (await clienteA.GetAsync($"/api/v1/tratamientos/{planId}/programaciones")).Content.ReadFromJsonAsync<List<ProgramacionTratamientoResponse>>();
        Assert.Equal(4, programaciones!.Count); // dia 1 y dia 2 completos (08:00 y 20:00 cada uno); el dia 3 queda fuera del corte.
    }

    // ---------- 12. Sin duplicados (proteccion de indice unico) ----------

    [SkippableFact]
    public async Task ProgramacionesDuplicadas_MismoItemYFecha_SonRechazadasPorElIndiceUnico()
    {
        Skip.IfNot(postgres.EstaDisponible, MotivoOmision());
        await AsegurarClientesCompartidosAsync();

        var mascotaVeterinariaId = await CrearMascotaVeterinariaAsync(clienteA, NombreUnico("Firulais"));
        var planId = await CrearPlanAsync(clienteA, mascotaVeterinariaId);
        var respuestaItem = await clienteA.PostAsJsonAsync(
            $"/api/v1/tratamientos/{planId}/items",
            new AgregarItemTratamientoRequest("Medicamento", "Amoxicilina", null, 1m, "tableta", "Oral", "Unica", null, null, [], Inicio, null, null, null, 0));
        var item = (await respuestaItem.Content.ReadFromJsonAsync<AgregarItemTratamientoResponse>())!;

        using var scope = factory.Services.CreateScope();
        var repositorioProgramaciones = scope.ServiceProvider.GetRequiredService<IRepositorioProgramacionesTratamiento>();

        var primera = Domain.Tratamientos.ProgramacionTratamiento.Crear(veterinariaAId, mascotaVeterinariaId, planId, item.ItemTratamientoId, Inicio);
        await repositorioProgramaciones.AgregarRangoAsync([primera], CancellationToken.None);

        var duplicada = Domain.Tratamientos.ProgramacionTratamiento.Crear(veterinariaAId, mascotaVeterinariaId, planId, item.ItemTratamientoId, Inicio);

        await Assert.ThrowsAnyAsync<DbUpdateException>(() => repositorioProgramaciones.AgregarRangoAsync([duplicada], CancellationToken.None));
    }

    // ---------- 13. Suspender plan cancela futuras pendientes ----------

    [SkippableFact]
    public async Task SuspenderPlan_CancelaProgramacionesFuturasPendientes()
    {
        Skip.IfNot(postgres.EstaDisponible, MotivoOmision());
        await AsegurarClientesCompartidosAsync();

        var mascotaVeterinariaId = await CrearMascotaVeterinariaAsync(clienteA, NombreUnico("Firulais"));
        var planId = await CrearPlanAsync(clienteA, mascotaVeterinariaId);
        await clienteA.PostAsJsonAsync(
            $"/api/v1/tratamientos/{planId}/items",
            new AgregarItemTratamientoRequest("Medicamento", "Amoxicilina", null, 1m, "tableta", "Oral", "IntervaloHoras", 8, null, [], Inicio, Inicio.AddDays(1), null, null, 0));
        (await clienteA.PostAsync($"/api/v1/tratamientos/{planId}/activar", null)).EnsureSuccessStatusCode();

        var respuesta = await clienteA.PostAsJsonAsync($"/api/v1/tratamientos/{planId}/suspender", new SuspenderPlanTratamientoRequest("Efectos adversos"));
        Assert.Equal(HttpStatusCode.NoContent, respuesta.StatusCode);

        var programaciones = await (await clienteA.GetAsync($"/api/v1/tratamientos/{planId}/programaciones")).Content.ReadFromJsonAsync<List<ProgramacionTratamientoResponse>>();
        Assert.All(programaciones!, p => Assert.Equal("Cancelada", p.Estado));

        var plan = await (await clienteA.GetAsync($"/api/v1/tratamientos/{planId}")).Content.ReadFromJsonAsync<PlanTratamientoResponse>();
        Assert.Equal("Suspendido", plan!.Estado);
    }

    // ---------- 14. Reanudar no regenera las canceladas ----------

    [SkippableFact]
    public async Task ReanudarPlan_NoRegeneraLasProgramacionesCanceladas()
    {
        Skip.IfNot(postgres.EstaDisponible, MotivoOmision());
        await AsegurarClientesCompartidosAsync();

        var mascotaVeterinariaId = await CrearMascotaVeterinariaAsync(clienteA, NombreUnico("Firulais"));
        var planId = await CrearPlanAsync(clienteA, mascotaVeterinariaId);
        await clienteA.PostAsJsonAsync(
            $"/api/v1/tratamientos/{planId}/items",
            new AgregarItemTratamientoRequest("Medicamento", "Amoxicilina", null, 1m, "tableta", "Oral", "IntervaloHoras", 8, null, [], Inicio, Inicio.AddDays(1), null, null, 0));
        (await clienteA.PostAsync($"/api/v1/tratamientos/{planId}/activar", null)).EnsureSuccessStatusCode();

        var cantidadAntes = (await (await clienteA.GetAsync($"/api/v1/tratamientos/{planId}/programaciones")).Content.ReadFromJsonAsync<List<ProgramacionTratamientoResponse>>())!.Count;

        await clienteA.PostAsJsonAsync($"/api/v1/tratamientos/{planId}/suspender", new SuspenderPlanTratamientoRequest("Motivo"));
        (await clienteA.PostAsync($"/api/v1/tratamientos/{planId}/reanudar", null)).EnsureSuccessStatusCode();

        var cantidadDespues = (await (await clienteA.GetAsync($"/api/v1/tratamientos/{planId}/programaciones")).Content.ReadFromJsonAsync<List<ProgramacionTratamientoResponse>>())!.Count;

        Assert.Equal(cantidadAntes, cantidadDespues);
        var plan = await (await clienteA.GetAsync($"/api/v1/tratamientos/{planId}")).Content.ReadFromJsonAsync<PlanTratamientoResponse>();
        Assert.Equal("Activo", plan!.Estado);
    }

    // ---------- 15/16. Completar/Cancelar plan cancelan futuras pendientes ----------

    [SkippableFact]
    public async Task CompletarPlan_CancelaProgramacionesFuturasPendientes()
    {
        Skip.IfNot(postgres.EstaDisponible, MotivoOmision());
        await AsegurarClientesCompartidosAsync();

        var mascotaVeterinariaId = await CrearMascotaVeterinariaAsync(clienteA, NombreUnico("Firulais"));
        var planId = await CrearPlanAsync(clienteA, mascotaVeterinariaId);
        await clienteA.PostAsJsonAsync(
            $"/api/v1/tratamientos/{planId}/items",
            new AgregarItemTratamientoRequest("Medicamento", "Amoxicilina", null, 1m, "tableta", "Oral", "IntervaloHoras", 8, null, [], Inicio, Inicio.AddDays(1), null, null, 0));
        (await clienteA.PostAsync($"/api/v1/tratamientos/{planId}/activar", null)).EnsureSuccessStatusCode();

        (await clienteA.PostAsync($"/api/v1/tratamientos/{planId}/completar", null)).EnsureSuccessStatusCode();

        var programaciones = await (await clienteA.GetAsync($"/api/v1/tratamientos/{planId}/programaciones")).Content.ReadFromJsonAsync<List<ProgramacionTratamientoResponse>>();
        Assert.All(programaciones!, p => Assert.Equal("Cancelada", p.Estado));
    }

    [SkippableFact]
    public async Task CancelarPlan_CancelaProgramacionesFuturasPendientes()
    {
        Skip.IfNot(postgres.EstaDisponible, MotivoOmision());
        await AsegurarClientesCompartidosAsync();

        var mascotaVeterinariaId = await CrearMascotaVeterinariaAsync(clienteA, NombreUnico("Firulais"));
        var planId = await CrearPlanAsync(clienteA, mascotaVeterinariaId);
        await clienteA.PostAsJsonAsync(
            $"/api/v1/tratamientos/{planId}/items",
            new AgregarItemTratamientoRequest("Medicamento", "Amoxicilina", null, 1m, "tableta", "Oral", "IntervaloHoras", 8, null, [], Inicio, Inicio.AddDays(1), null, null, 0));
        (await clienteA.PostAsync($"/api/v1/tratamientos/{planId}/activar", null)).EnsureSuccessStatusCode();

        var respuesta = await clienteA.PostAsJsonAsync($"/api/v1/tratamientos/{planId}/cancelar", new CancelarPlanTratamientoRequest("Mascota falleció"));
        Assert.Equal(HttpStatusCode.NoContent, respuesta.StatusCode);

        var programaciones = await (await clienteA.GetAsync($"/api/v1/tratamientos/{planId}/programaciones")).Content.ReadFromJsonAsync<List<ProgramacionTratamientoResponse>>();
        Assert.All(programaciones!, p => Assert.Equal("Cancelada", p.Estado));
    }

    // ---------- 17/18. Registrar realizada / omitida ----------

    [SkippableFact]
    public async Task RegistrarProgramacion_RealizadaYOmitida()
    {
        Skip.IfNot(postgres.EstaDisponible, MotivoOmision());
        await AsegurarClientesCompartidosAsync();

        var mascotaVeterinariaId = await CrearMascotaVeterinariaAsync(clienteA, NombreUnico("Firulais"));
        var planId = await CrearPlanAsync(clienteA, mascotaVeterinariaId);
        await clienteA.PostAsJsonAsync(
            $"/api/v1/tratamientos/{planId}/items",
            new AgregarItemTratamientoRequest("Medicamento", "Amoxicilina", null, 1m, "tableta", "Oral", "IntervaloHoras", 8, null, [], Inicio, Inicio.AddDays(1), null, null, 0));
        (await clienteA.PostAsync($"/api/v1/tratamientos/{planId}/activar", null)).EnsureSuccessStatusCode();

        var programaciones = (await (await clienteA.GetAsync($"/api/v1/tratamientos/{planId}/programaciones")).Content.ReadFromJsonAsync<List<ProgramacionTratamientoResponse>>())!;
        var primera = programaciones[0];
        var segunda = programaciones[1];

        var respuestaRealizar = await clienteA.PostAsJsonAsync($"/api/v1/programaciones-tratamiento/{primera.Id}/realizar", new RegistrarProgramacionRealizadaRequest("Toleró bien"));
        Assert.Equal(HttpStatusCode.NoContent, respuestaRealizar.StatusCode);

        var respuestaOmitir = await clienteA.PostAsJsonAsync($"/api/v1/programaciones-tratamiento/{segunda.Id}/omitir", new MarcarProgramacionOmitidaRequest("Mascota vomitó"));
        Assert.Equal(HttpStatusCode.NoContent, respuestaOmitir.StatusCode);

        var actualizadas = (await (await clienteA.GetAsync($"/api/v1/tratamientos/{planId}/programaciones")).Content.ReadFromJsonAsync<List<ProgramacionTratamientoResponse>>())!;
        Assert.Equal("Realizada", actualizadas.Single(p => p.Id == primera.Id).Estado);
        Assert.Equal("Omitida", actualizadas.Single(p => p.Id == segunda.Id).Estado);
    }

    // ---------- 19. Adherencia correcta ----------

    [SkippableFact]
    public async Task ObtenerAdherencia_CalculaElPorcentajeCorrectamente()
    {
        Skip.IfNot(postgres.EstaDisponible, MotivoOmision());
        await AsegurarClientesCompartidosAsync();

        var mascotaVeterinariaId = await CrearMascotaVeterinariaAsync(clienteA, NombreUnico("Firulais"));
        var planId = await CrearPlanAsync(clienteA, mascotaVeterinariaId);
        await clienteA.PostAsJsonAsync(
            $"/api/v1/tratamientos/{planId}/items",
            new AgregarItemTratamientoRequest("Medicamento", "Amoxicilina", null, 1m, "tableta", "Oral", "IntervaloHoras", 8, null, [], Inicio, Inicio.AddDays(1), null, null, 0));
        (await clienteA.PostAsync($"/api/v1/tratamientos/{planId}/activar", null)).EnsureSuccessStatusCode();

        var programaciones = (await (await clienteA.GetAsync($"/api/v1/tratamientos/{planId}/programaciones")).Content.ReadFromJsonAsync<List<ProgramacionTratamientoResponse>>())!;
        Assert.Equal(3, programaciones.Count);

        await clienteA.PostAsJsonAsync($"/api/v1/programaciones-tratamiento/{programaciones[0].Id}/realizar", new RegistrarProgramacionRealizadaRequest(null));
        await clienteA.PostAsJsonAsync($"/api/v1/programaciones-tratamiento/{programaciones[1].Id}/omitir", new MarcarProgramacionOmitidaRequest(null));
        // programaciones[2] permanece PENDIENTE.

        var adherencia = await (await clienteA.GetAsync($"/api/v1/tratamientos/{planId}/adherencia")).Content.ReadFromJsonAsync<AdherenciaResponse>();

        Assert.Equal(1, adherencia!.Realizadas);
        Assert.Equal(1, adherencia.Omitidas);
        Assert.Equal(1, adherencia.Pendientes);
        Assert.Equal(50m, adherencia.PorcentajeAdherencia); // 1 / (1+1+0) * 100
    }

    // ---------- 21/22/23. Seguimiento manual y automatico ----------

    [SkippableFact]
    public async Task CrearSeguimiento_Manual_EsCreadoPendiente()
    {
        Skip.IfNot(postgres.EstaDisponible, MotivoOmision());
        await AsegurarClientesCompartidosAsync();

        var mascotaVeterinariaId = await CrearMascotaVeterinariaAsync(clienteA, NombreUnico("Firulais"));

        var respuesta = await clienteA.PostAsJsonAsync(
            $"/api/v1/mascotas/{mascotaVeterinariaId}/seguimientos",
            new CrearSeguimientoRequest(null, null, "Control", DateTimeOffset.UtcNow.AddDays(30), "Revisión de rutina", null));

        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);

        var seguimientos = await (await clienteA.GetAsync($"/api/v1/mascotas/{mascotaVeterinariaId}/seguimientos")).Content.ReadFromJsonAsync<List<SeguimientoClinicoResponse>>();
        Assert.Single(seguimientos!);
        Assert.False(seguimientos![0].EsGeneradoDesdeProximoControl);
    }

    [SkippableFact]
    public async Task FinalizarConsulta_ConProximaFechaControl_GeneraSeguimientoAutomaticoSinDuplicar()
    {
        Skip.IfNot(postgres.EstaDisponible, MotivoOmision());
        await AsegurarClientesCompartidosAsync();

        var mascotaVeterinariaId = await CrearMascotaVeterinariaAsync(clienteA, NombreUnico("Firulais"));
        var proximaFecha = DateTimeOffset.UtcNow.AddDays(20);
        var respuestaConsulta = await clienteA.PostAsJsonAsync(
            "/api/v1/consultas",
            new CrearConsultaRequest(mascotaVeterinariaId, null, usuarioAId, DateTimeOffset.UtcNow, "Motivo", null, null, null, null, null, proximaFecha, "Revisar evolución"));
        var consulta = (await respuestaConsulta.Content.ReadFromJsonAsync<CrearConsultaResponse>())!;

        (await clienteA.PostAsync($"/api/v1/consultas/{consulta.ConsultaId}/finalizar", null)).EnsureSuccessStatusCode();

        var seguimientos = await (await clienteA.GetAsync($"/api/v1/mascotas/{mascotaVeterinariaId}/seguimientos")).Content.ReadFromJsonAsync<List<SeguimientoClinicoResponse>>();
        Assert.Single(seguimientos!, s => s.EsGeneradoDesdeProximoControl && s.ConsultaOrigenId == consulta.ConsultaId);

        // Item 23: reintentar finalizar (ya finalizada) no debe duplicar el seguimiento automatico.
        var respuestaReintento = await clienteA.PostAsync($"/api/v1/consultas/{consulta.ConsultaId}/finalizar", null);
        Assert.Equal(HttpStatusCode.BadRequest, respuestaReintento.StatusCode);

        var seguimientosDespues = await (await clienteA.GetAsync($"/api/v1/mascotas/{mascotaVeterinariaId}/seguimientos")).Content.ReadFromJsonAsync<List<SeguimientoClinicoResponse>>();
        Assert.Single(seguimientosDespues!, s => s.EsGeneradoDesdeProximoControl && s.ConsultaOrigenId == consulta.ConsultaId);
    }

    // ---------- 24. Seguimiento A no visible B ----------

    [SkippableFact]
    public async Task Seguimiento_DeVeterinariaA_NoVisibleParaB()
    {
        Skip.IfNot(postgres.EstaDisponible, MotivoOmision());
        await AsegurarClientesCompartidosAsync();

        var mascotaDeA = await CrearMascotaVeterinariaAsync(clienteA, NombreUnico("DeA"));
        var respuestaSeguimiento = await clienteA.PostAsJsonAsync(
            $"/api/v1/mascotas/{mascotaDeA}/seguimientos",
            new CrearSeguimientoRequest(null, null, "Control", DateTimeOffset.UtcNow.AddDays(15), "Control de A", null));
        var seguimientoDeA = (await respuestaSeguimiento.Content.ReadFromJsonAsync<CrearSeguimientoResponse>())!;

        var respuesta = await clienteB.PostAsync($"/api/v1/seguimientos/{seguimientoDeA.SeguimientoId}/realizar", null);
        Assert.Equal(HttpStatusCode.NotFound, respuesta.StatusCode);
    }

    // ---------- 25. Historial muestra tratamiento de A solamente ----------

    [SkippableFact]
    public async Task HistorialClinico_MuestraPlanTratamientoDeLaPropiaVeterinaria()
    {
        Skip.IfNot(postgres.EstaDisponible, MotivoOmision());
        await AsegurarClientesCompartidosAsync();

        var mascotaVeterinariaId = await CrearMascotaVeterinariaAsync(clienteA, NombreUnico("Firulais"));
        var nombrePlan = NombreUnico("TratamientoHistorial");
        await clienteA.PostAsJsonAsync("/api/v1/tratamientos", new CrearPlanTratamientoRequest(mascotaVeterinariaId, null, nombrePlan, null, Inicio, null));

        var historial = await (await clienteA.GetAsync($"/api/v1/mascotas/{mascotaVeterinariaId}/historial")).Content.ReadFromJsonAsync<HistorialMascotaResponse>();

        Assert.Contains(historial!.Eventos, e => e.Tipo == "PlanTratamiento" && e.Titulo == nombrePlan);
    }

    // ---------- Prueba critica: misma mascota global, tratamientos independientes por veterinaria ----------

    [SkippableFact]
    public async Task Tratamientos_DeMismaMascotaGlobal_SonIndependientesPorVeterinaria()
    {
        Skip.IfNot(postgres.EstaDisponible, MotivoOmision());
        await AsegurarClientesCompartidosAsync();

        var nombreCompartido = NombreUnico("Toby");
        var mascotaEnA = await CrearMascotaVeterinariaAsync(clienteA, nombreCompartido);
        var mascotaEnB = await CrearMascotaVeterinariaAsync(clienteB, nombreCompartido);

        var nombrePlanA = NombreUnico("PlanDeA");
        var nombrePlanB = NombreUnico("PlanDeB");
        await clienteA.PostAsJsonAsync("/api/v1/tratamientos", new CrearPlanTratamientoRequest(mascotaEnA, null, nombrePlanA, null, Inicio, null));
        await clienteB.PostAsJsonAsync("/api/v1/tratamientos", new CrearPlanTratamientoRequest(mascotaEnB, null, nombrePlanB, null, Inicio, null));

        var planesA = await (await clienteA.GetAsync("/api/v1/tratamientos")).Content.ReadFromJsonAsync<List<PlanTratamientoResponse>>();
        var planesB = await (await clienteB.GetAsync("/api/v1/tratamientos")).Content.ReadFromJsonAsync<List<PlanTratamientoResponse>>();

        Assert.Contains(planesA!, p => p.Nombre == nombrePlanA);
        Assert.DoesNotContain(planesA!, p => p.Nombre == nombrePlanB);
        Assert.Contains(planesB!, p => p.Nombre == nombrePlanB);
        Assert.DoesNotContain(planesB!, p => p.Nombre == nombrePlanA);
    }

    // ---------- Prueba critica transaccional: activacion ----------

    [SkippableFact]
    public async Task ActivarTratamiento_SiFallaGeneracionProgramaciones_HaceRollbackCompleto()
    {
        Skip.IfNot(postgres.EstaDisponible, MotivoOmision());

        using var scope = factory.Services.CreateScope();
        var repositorioPlanes = scope.ServiceProvider.GetRequiredService<IRepositorioPlanesTratamiento>();
        var repositorioProgramaciones = scope.ServiceProvider.GetRequiredService<IRepositorioProgramacionesTratamiento>();
        var unidadDeTrabajo = scope.ServiceProvider.GetRequiredService<IUnidadDeTrabajo>();

        var veterinariaId = await CrearVeterinariaActivaAsync();
        Guid mascotaVeterinariaId;
        Guid itemId;
        await using (var contextoSemilla = postgres.CrearDbContext())
        {
            var usuario = SaaS.Veterinario.Domain.Usuarios.Usuario.Crear("Persona de Prueba", $"prueba.tto.{Guid.NewGuid():N}@correo.com");
            contextoSemilla.Usuarios.Add(usuario);
            var especieId = (await contextoSemilla.Especies.FirstAsync()).Id;
            var mascota = SaaS.Veterinario.Domain.Mascotas.Mascota.Crear(
                $"PET-{Guid.NewGuid():N}"[..10].ToUpperInvariant(), "Firulais", especieId, null, SaaS.Veterinario.Domain.Mascotas.SexoMascota.Macho, null, false, null, null, null);
            contextoSemilla.Mascotas.Add(mascota);
            var mascotaVeterinaria = SaaS.Veterinario.Domain.Mascotas.MascotaVeterinaria.Crear(veterinariaId, mascota.Id);
            contextoSemilla.MascotasVeterinaria.Add(mascotaVeterinaria);

            var plan = PlanTratamiento.Crear(veterinariaId, mascotaVeterinaria.Id, null, "Tratamiento", null, Inicio, null, usuario.Id);
            contextoSemilla.PlanesTratamiento.Add(plan);

            var item = ItemTratamiento.Crear(
                plan.Id, TipoItemTratamiento.Medicamento, "Amoxicilina", null, 1m, "tableta", null,
                TipoFrecuenciaTratamiento.Unica, null, null, [], Inicio, null, null, null, 0);
            contextoSemilla.ItemsTratamiento.Add(item);

            // Programacion PRE-EXISTENTE que colisiona exactamente con la que ActivarPlan
            // generaria (mismo item, misma fecha) -- provoca una violacion real del indice
            // unico al intentar persistir el lote generado.
            var colision = Domain.Tratamientos.ProgramacionTratamiento.Crear(veterinariaId, mascotaVeterinaria.Id, plan.Id, item.Id, Inicio);
            contextoSemilla.ProgramacionesTratamiento.Add(colision);

            await contextoSemilla.SaveChangesAsync();

            mascotaVeterinariaId = mascotaVeterinaria.Id;
            itemId = item.Id;
        }

        var planEnRepo = (await repositorioPlanes.ListarPorMascotaVeterinariaAsync(mascotaVeterinariaId, veterinariaId, CancellationToken.None)).Single();

        await Assert.ThrowsAnyAsync<DbUpdateException>(() =>
            unidadDeTrabajo.EjecutarEnTransaccionAsync(async ct =>
            {
                planEnRepo.Activar(planEnRepo.CreadoPorUsuarioId, DateTimeOffset.UtcNow);
                await repositorioPlanes.GuardarCambiosAsync(ct);

                // Se intenta persistir OTRA programacion identica a la ya sembrada -- viola el
                // indice unico (item_tratamiento_id, fecha_hora_programada) dentro de la MISMA
                // transaccion que ya activo el plan.
                var duplicada = Domain.Tratamientos.ProgramacionTratamiento.Crear(veterinariaId, mascotaVeterinariaId, planEnRepo.Id, itemId, Inicio);
                await repositorioProgramaciones.AgregarRangoAsync([duplicada], ct);
            }, CancellationToken.None));

        await using var contextoVerificacion = postgres.CrearDbContext();
        var planEnBd = await contextoVerificacion.PlanesTratamiento.SingleAsync(p => p.Id == planEnRepo.Id);
        var programacionesEnBd = await contextoVerificacion.ProgramacionesTratamiento.Where(p => p.PlanTratamientoId == planEnRepo.Id).ToListAsync();

        Assert.Equal(EstadoPlanTratamiento.Pendiente, planEnBd.Estado);
        Assert.Single(programacionesEnBd); // solo la sembrada originalmente, ninguna adicional.
    }

    // ---------- Prueba critica transaccional: suspension ----------

    [SkippableFact]
    public async Task SuspenderTratamiento_SiFallaCancelacionProgramaciones_HaceRollbackCompleto()
    {
        Skip.IfNot(postgres.EstaDisponible, MotivoOmision());
        await AsegurarClientesCompartidosAsync();

        var mascotaVeterinariaId = await CrearMascotaVeterinariaAsync(clienteA, NombreUnico("Firulais"));
        var planId = await CrearPlanAsync(clienteA, mascotaVeterinariaId);
        await clienteA.PostAsJsonAsync(
            $"/api/v1/tratamientos/{planId}/items",
            new AgregarItemTratamientoRequest("Medicamento", "Amoxicilina", null, 1m, "tableta", "Oral", "Unica", null, null, [], Inicio, null, null, null, 0));
        (await clienteA.PostAsync($"/api/v1/tratamientos/{planId}/activar", null)).EnsureSuccessStatusCode();

        using var scope = factory.Services.CreateScope();
        var repositorioPlanes = scope.ServiceProvider.GetRequiredService<IRepositorioPlanesTratamiento>();
        var repositorioProgramaciones = scope.ServiceProvider.GetRequiredService<IRepositorioProgramacionesTratamiento>();
        var unidadDeTrabajo = scope.ServiceProvider.GetRequiredService<IUnidadDeTrabajo>();

        var plan = (await repositorioPlanes.ObtenerPorIdAsync(planId, veterinariaAId, CancellationToken.None))!;
        var pendiente = (await repositorioProgramaciones.ListarPendientesPorPlanAsync(planId, CancellationToken.None)).Single();

        await Assert.ThrowsAnyAsync<Domain.Errores.ExcepcionDominio>(() =>
            unidadDeTrabajo.EjecutarEnTransaccionAsync(async ct =>
            {
                plan.Suspender(plan.CreadoPorUsuarioId, "Motivo", DateTimeOffset.UtcNow);
                await repositorioPlanes.GuardarCambiosAsync(ct);

                pendiente.Cancelar(DateTimeOffset.UtcNow);
                await repositorioProgramaciones.GuardarCambiosAsync(ct);

                // Segunda cancelacion sobre la MISMA fila ya cancelada: CANCELADA -> CANCELADA
                // no es una transicion valida (seccion 17) -- fuerza un fallo real DESPUES de
                // que la suspension del plan y la primera cancelacion ya se ejecutaron.
                pendiente.Cancelar(DateTimeOffset.UtcNow);
            }, CancellationToken.None));

        await using var contextoVerificacion = postgres.CrearDbContext();
        var planEnBd = await contextoVerificacion.PlanesTratamiento.SingleAsync(p => p.Id == planId);
        var programacionEnBd = await contextoVerificacion.ProgramacionesTratamiento.SingleAsync(p => p.Id == pendiente.Id);

        Assert.Equal(EstadoPlanTratamiento.Activo, planEnBd.Estado);
        Assert.Equal(EstadoProgramacionTratamiento.Pendiente, programacionEnBd.Estado);
    }

    // ---------- Prueba de zona horaria (extremo a extremo) ----------

    [SkippableFact]
    public async Task ActivarPlan_ConHoraFijaParaVeterinariaGuayaquil_PersisteElOffsetCorrecto()
    {
        Skip.IfNot(postgres.EstaDisponible, MotivoOmision());
        await AsegurarClientesCompartidosAsync();

        var mascotaVeterinariaId = await CrearMascotaVeterinariaAsync(clienteA, NombreUnico("Firulais"));
        var planId = await CrearPlanAsync(clienteA, mascotaVeterinariaId);

        // Inicio esta anclado en offset UTC (+00:00); la ocurrencia real de "08:00 hora local de
        // Guayaquil" cae varias horas DESPUES de ese instante en UTC (08:00-05:00 = 13:00Z). El
        // rango [FechaInicio, FechaFin] debe cubrir generosamente ese instante para no filtrarlo.
        var horarios = new List<TimeOnly> { new(8, 0) };
        await clienteA.PostAsJsonAsync(
            $"/api/v1/tratamientos/{planId}/items",
            new AgregarItemTratamientoRequest(
                "Cuidado", "Control matutino", null, null, null, null, "HoraFija", null, null, horarios,
                Inicio.AddHours(-2), Inicio.AddHours(10), null, null, 0));

        (await clienteA.PostAsync($"/api/v1/tratamientos/{planId}/activar", null)).EnsureSuccessStatusCode();

        var programaciones = await (await clienteA.GetAsync($"/api/v1/tratamientos/{planId}/programaciones")).Content.ReadFromJsonAsync<List<ProgramacionTratamientoResponse>>();
        var unica = Assert.Single(programaciones!);

        // La veterinaria de prueba usa ZonaHoraria "America/Guayaquil" (UTC-5, sin horario de
        // verano): 08:00 hora clinica local debe persistirse como el INSTANTE correcto (13:00Z),
        // nunca como si 08:00 fuera directamente UTC. Postgres/Npgsql normaliza a Offset=0 al
        // escribir, por eso se verifica convirtiendo de vuelta a la zona de la veterinaria.
        Assert.Equal(TimeSpan.Zero, unica.FechaHoraProgramada.Offset);
        Assert.Equal(13, unica.FechaHoraProgramada.Hour);
        var horaLocal = TimeZoneInfo.ConvertTime(unica.FechaHoraProgramada, TimeZoneInfo.FindSystemTimeZoneById("America/Guayaquil"));
        Assert.Equal(8, horaLocal.Hour);
    }
}
