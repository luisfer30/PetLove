using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SaaS.Veterinario.Api.Autenticacion;
using SaaS.Veterinario.Api.Citas;
using SaaS.Veterinario.Api.Consultas;
using SaaS.Veterinario.Api.Mascotas;
using SaaS.Veterinario.Application.Abstracciones;
using SaaS.Veterinario.Domain.Citas;
using SaaS.Veterinario.Domain.Consultas;
using SaaS.Veterinario.Domain.Mascotas;
using SaaS.Veterinario.Domain.Membresias;
using SaaS.Veterinario.Domain.Usuarios;
using SaaS.Veterinario.Domain.Veterinarias;
using SaaS.Veterinario.IntegrationTests.Infraestructura;

namespace SaaS.Veterinario.IntegrationTests.Api;

/// <summary>
/// Cubre la Etapa 7: Agenda + Consultas + Diagnosticos + Historial clinico basico.
///
/// Reutiliza DOS usuarios (A y B), cada uno con AMBOS roles ADMINISTRADOR_VETERINARIA +
/// VETERINARIO en su propia veterinaria (seccion 31: la combinacion de ambos roles da acceso
/// completo a agenda + clinica), memoizados en un campo STATIC -- mismo patron y misma razon
/// que en MascotasClientesEndpointsTests: xUnit crea una instancia nueva de la clase por cada
/// [SkippableFact], y registrar un usuario nuevo por test agotaria el limitador de velocidad
/// de autenticacion.
/// </summary>
[Collection(ColeccionPostgres.Nombre)]
public sealed class CitasConsultasEndpointsTests : IClassFixture<SaaSVeterinarioApiFactory>
{
    private const string Password = "password123";

    private static readonly SemaphoreSlim InicializacionLock = new(1, 1);
    private static (HttpClient ClienteA, Guid VeterinariaAId, Guid UsuarioAId, HttpClient ClienteB, Guid VeterinariaBId, Guid UsuarioBId)? _compartido;

    private readonly PostgresContainerFixture postgres;
    private readonly SaaSVeterinarioApiFactory factory;

    public CitasConsultasEndpointsTests(PostgresContainerFixture postgres, SaaSVeterinarioApiFactory factory)
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

    private static string CorreoUnico() => $"prueba.citas.{Guid.NewGuid():N}@correo.com";

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

    private async Task<(HttpClient Cliente, Guid VeterinariaId, Guid UsuarioId)> ClienteAutenticadoConVeterinariaAsync(params string[] codigosRoles)
    {
        var cliente = factory.CreateClient();
        var (usuarioId, correo) = await RegistrarYConfirmarAsync(cliente);
        var veterinariaId = await CrearVeterinariaActivaAsync();
        await CrearMembresiaActivaAsync(usuarioId, veterinariaId, codigosRoles);
        var tokens = await LoginAsync(cliente, correo);
        return (ConToken(cliente, tokens.AccessToken), veterinariaId, usuarioId);
    }

    private async Task<Guid> ObtenerPrimeraEspecieIdAsync()
    {
        await using var contexto = postgres.CrearDbContext();
        return (await contexto.Especies.FirstAsync()).Id;
    }

    private async Task<Guid> CrearMascotaVeterinariaAsync(HttpClient cliente, string nombre)
    {
        var especieId = await ObtenerPrimeraEspecieIdAsync();
        var respuesta = await cliente.PostAsJsonAsync(
            "/api/v1/mascotas",
            new RegistrarMascotaRequest(nombre, especieId, null, "Macho", null, false, null, null, null, null));
        respuesta.EnsureSuccessStatusCode();
        var creada = (await respuesta.Content.ReadFromJsonAsync<RegistrarMascotaResponse>())!;
        return creada.MascotaVeterinariaId;
    }

    // Contador estatico para que cada instancia de prueba (xUnit crea una nueva por cada
    // [SkippableFact]) reciba un dia distinto -- de lo contrario, con un Inicio/Fin fijo
    // compartido, la deteccion de solapamiento de horario (seccion 38) rechazaria la segunda
    // cita creada para el mismo usuarioAId/usuarioBId con un 409 legitimo pero indeseado aqui.
    private static int contadorHorario;

    // DateTimeOffset.UtcNow.Date pierde el offset (queda Unspecified) y el operador implicito
    // DateTime->DateTimeOffset asumiria la zona horaria LOCAL de la maquina, no UTC -- Npgsql
    // rechaza cualquier "timestamp with time zone" que no venga en offset 0.
    private readonly DateTimeOffset Inicio = new(DateTime.UtcNow.Date.AddDays(Interlocked.Increment(ref contadorHorario)).AddHours(9), TimeSpan.Zero);

    private DateTimeOffset Fin => Inicio.AddMinutes(30);

    // ---------- 1/2/3. Creacion de citas ----------

    [SkippableFact]
    public async Task CrearCita_ParaMascotaPropiaConVeterinarioAsignado_EsCreada()
    {
        Skip.IfNot(postgres.EstaDisponible, MotivoOmision());
        await AsegurarClientesCompartidosAsync();

        var mascotaVeterinariaId = await CrearMascotaVeterinariaAsync(clienteA, NombreUnico("Firulais"));

        var respuesta = await clienteA.PostAsJsonAsync(
            "/api/v1/citas",
            new CrearCitaRequest(mascotaVeterinariaId, null, usuarioAId, Inicio, Fin, "Control anual", null));

        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
        var cuerpo = await respuesta.Content.ReadFromJsonAsync<CrearCitaResponse>();
        Assert.Equal("Programada", cuerpo!.Estado);
    }

    [SkippableFact]
    public async Task CrearCita_SinClienteNiVeterinarioAsignado_EsValida()
    {
        Skip.IfNot(postgres.EstaDisponible, MotivoOmision());
        await AsegurarClientesCompartidosAsync();

        var mascotaVeterinariaId = await CrearMascotaVeterinariaAsync(clienteA, NombreUnico("SinDueno"));

        var respuesta = await clienteA.PostAsJsonAsync(
            "/api/v1/citas",
            new CrearCitaRequest(mascotaVeterinariaId, null, null, Inicio, Fin, "Emergencia", null));

        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
    }

    // ---------- 4. Aislamiento multi-tenant de citas ----------

    [SkippableFact]
    public async Task VeterinariaA_NoPuedeVerNiModificarCitaDeVeterinariaB()
    {
        Skip.IfNot(postgres.EstaDisponible, MotivoOmision());
        await AsegurarClientesCompartidosAsync();

        var mascotaDeB = await CrearMascotaVeterinariaAsync(clienteB, NombreUnico("DeB"));
        var respuestaCrear = await clienteB.PostAsJsonAsync(
            "/api/v1/citas", new CrearCitaRequest(mascotaDeB, null, usuarioBId, Inicio, Fin, "Motivo de B", null));
        var citaDeB = (await respuestaCrear.Content.ReadFromJsonAsync<CrearCitaResponse>())!;

        var respuestaGet = await clienteA.GetAsync($"/api/v1/citas/{citaDeB.CitaId}");
        Assert.Equal(HttpStatusCode.NotFound, respuestaGet.StatusCode);

        var respuestaConfirmar = await clienteA.PostAsync($"/api/v1/citas/{citaDeB.CitaId}/confirmar", null);
        Assert.Equal(HttpStatusCode.NotFound, respuestaConfirmar.StatusCode);
    }

    // ---------- 5. Actualizar cita propia ----------

    [SkippableFact]
    public async Task ActualizarCita_Propia_ModificaLosDatos()
    {
        Skip.IfNot(postgres.EstaDisponible, MotivoOmision());
        await AsegurarClientesCompartidosAsync();

        var mascotaVeterinariaId = await CrearMascotaVeterinariaAsync(clienteA, NombreUnico("Firulais"));
        var respuestaCrear = await clienteA.PostAsJsonAsync(
            "/api/v1/citas", new CrearCitaRequest(mascotaVeterinariaId, null, null, Inicio, Fin, "Motivo original", null));
        var cita = (await respuestaCrear.Content.ReadFromJsonAsync<CrearCitaResponse>())!;

        var respuestaActualizar = await clienteA.PutAsJsonAsync(
            $"/api/v1/citas/{cita.CitaId}",
            new ActualizarCitaRequest(Inicio, Fin.AddMinutes(15), "Motivo actualizado", "Trae vacunas", null));

        Assert.Equal(HttpStatusCode.NoContent, respuestaActualizar.StatusCode);

        var respuestaGet = await clienteA.GetAsync($"/api/v1/citas/{cita.CitaId}");
        var citaActualizada = await respuestaGet.Content.ReadFromJsonAsync<CitaResponse>();
        Assert.Equal("Motivo actualizado", citaActualizada!.Motivo);
    }

    // ---------- 6. Cancelar cita ----------

    [SkippableFact]
    public async Task CancelarCita_CambiaElEstadoACancelada()
    {
        Skip.IfNot(postgres.EstaDisponible, MotivoOmision());
        await AsegurarClientesCompartidosAsync();

        var mascotaVeterinariaId = await CrearMascotaVeterinariaAsync(clienteA, NombreUnico("Firulais"));
        var respuestaCrear = await clienteA.PostAsJsonAsync(
            "/api/v1/citas", new CrearCitaRequest(mascotaVeterinariaId, null, null, Inicio, Fin, "Motivo", null));
        var cita = (await respuestaCrear.Content.ReadFromJsonAsync<CrearCitaResponse>())!;

        var respuestaCancelar = await clienteA.PostAsync($"/api/v1/citas/{cita.CitaId}/cancelar", null);
        Assert.Equal(HttpStatusCode.NoContent, respuestaCancelar.StatusCode);

        var respuestaGet = await clienteA.GetAsync($"/api/v1/citas/{cita.CitaId}");
        var citaCancelada = await respuestaGet.Content.ReadFromJsonAsync<CitaResponse>();
        Assert.Equal("Cancelada", citaCancelada!.Estado);
    }

    // ---------- 7. Transicion de estado invalida rechazada ----------

    // Ajuste post-Etapa 7 (AJUSTE 1): PROGRAMADA/CONFIRMADA/EN_ESPERA ahora SI pueden llegar a
    // ATENDIDA -- la transicion realmente invalida a probar aqui es CANCELADA -> ATENDIDA
    // (item 4 de la lista de pruebas pedida), un estado terminal del que nunca se puede salir.
    [SkippableFact]
    public async Task AtenderCita_DesdeCancelada_Retorna400EstadoInvalido()
    {
        Skip.IfNot(postgres.EstaDisponible, MotivoOmision());
        await AsegurarClientesCompartidosAsync();

        var mascotaVeterinariaId = await CrearMascotaVeterinariaAsync(clienteA, NombreUnico("Firulais"));
        var respuestaCrear = await clienteA.PostAsJsonAsync(
            "/api/v1/citas", new CrearCitaRequest(mascotaVeterinariaId, null, null, Inicio, Fin, "Motivo", null));
        var cita = (await respuestaCrear.Content.ReadFromJsonAsync<CrearCitaResponse>())!;

        (await clienteA.PostAsync($"/api/v1/citas/{cita.CitaId}/cancelar", null)).EnsureSuccessStatusCode();

        var respuesta = await clienteA.PostAsync($"/api/v1/citas/{cita.CitaId}/atender", null);

        Assert.Equal(HttpStatusCode.BadRequest, respuesta.StatusCode);
        var problema = await respuesta.Content.ReadFromJsonAsync<Dictionary<string, object>>();
        Assert.Equal("CITA_ESTADO_INVALIDO", problema!["codigo"].ToString());
    }

    // Item 5 de la lista pedida: NO_ASISTIO -> ATENDIDA tambien debe seguir prohibido.
    [SkippableFact]
    public async Task AtenderCita_DesdeNoAsistio_Retorna400EstadoInvalido()
    {
        Skip.IfNot(postgres.EstaDisponible, MotivoOmision());
        await AsegurarClientesCompartidosAsync();

        var mascotaVeterinariaId = await CrearMascotaVeterinariaAsync(clienteA, NombreUnico("Firulais"));
        var respuestaCrear = await clienteA.PostAsJsonAsync(
            "/api/v1/citas", new CrearCitaRequest(mascotaVeterinariaId, null, null, Inicio, Fin, "Motivo", null));
        var cita = (await respuestaCrear.Content.ReadFromJsonAsync<CrearCitaResponse>())!;

        (await clienteA.PostAsync($"/api/v1/citas/{cita.CitaId}/no-asistio", null)).EnsureSuccessStatusCode();

        var respuesta = await clienteA.PostAsync($"/api/v1/citas/{cita.CitaId}/atender", null);

        Assert.Equal(HttpStatusCode.BadRequest, respuesta.StatusCode);
        var problema = await respuesta.Content.ReadFromJsonAsync<Dictionary<string, object>>();
        Assert.Equal("CITA_ESTADO_INVALIDO", problema!["codigo"].ToString());
    }

    // Items 1/2/3 de la lista pedida: PROGRAMADA/CONFIRMADA/EN_ESPERA -> ATENDIDA validos.
    [SkippableFact]
    public async Task AtenderCita_DesdeProgramadaConfirmadaOEnEspera_EsValido()
    {
        Skip.IfNot(postgres.EstaDisponible, MotivoOmision());
        await AsegurarClientesCompartidosAsync();

        var mascotaVeterinariaId = await CrearMascotaVeterinariaAsync(clienteA, NombreUnico("Firulais"));

        var respuestaProgramada = await clienteA.PostAsJsonAsync(
            "/api/v1/citas", new CrearCitaRequest(mascotaVeterinariaId, null, null, Inicio, Fin, "Motivo", null));
        var citaProgramada = (await respuestaProgramada.Content.ReadFromJsonAsync<CrearCitaResponse>())!;
        Assert.Equal(HttpStatusCode.NoContent, (await clienteA.PostAsync($"/api/v1/citas/{citaProgramada.CitaId}/atender", null)).StatusCode);

        var respuestaConfirmada = await clienteA.PostAsJsonAsync(
            "/api/v1/citas", new CrearCitaRequest(mascotaVeterinariaId, null, null, Inicio.AddHours(1), Fin.AddHours(1), "Motivo", null));
        var citaConfirmada = (await respuestaConfirmada.Content.ReadFromJsonAsync<CrearCitaResponse>())!;
        (await clienteA.PostAsync($"/api/v1/citas/{citaConfirmada.CitaId}/confirmar", null)).EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.NoContent, (await clienteA.PostAsync($"/api/v1/citas/{citaConfirmada.CitaId}/atender", null)).StatusCode);

        var respuestaEnEspera = await clienteA.PostAsJsonAsync(
            "/api/v1/citas", new CrearCitaRequest(mascotaVeterinariaId, null, null, Inicio.AddHours(2), Fin.AddHours(2), "Motivo", null));
        var citaEnEspera = (await respuestaEnEspera.Content.ReadFromJsonAsync<CrearCitaResponse>())!;
        (await clienteA.PostAsync($"/api/v1/citas/{citaEnEspera.CitaId}/en-espera", null)).EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.NoContent, (await clienteA.PostAsync($"/api/v1/citas/{citaEnEspera.CitaId}/atender", null)).StatusCode);
    }

    // ---------- 8/9. Crear consulta con y sin cita ----------

    [SkippableFact]
    public async Task CrearConsulta_SinCitaPrevia_EsCreadaEnBorrador()
    {
        Skip.IfNot(postgres.EstaDisponible, MotivoOmision());
        await AsegurarClientesCompartidosAsync();

        var mascotaVeterinariaId = await CrearMascotaVeterinariaAsync(clienteA, NombreUnico("Firulais"));

        var respuesta = await clienteA.PostAsJsonAsync(
            "/api/v1/consultas",
            new CrearConsultaRequest(mascotaVeterinariaId, null, usuarioAId, DateTimeOffset.UtcNow, "Emergencia sin cita", 12.5m, 38.2m, 90, "Nota interna", "Reposo", null, null));

        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
        var cuerpo = await respuesta.Content.ReadFromJsonAsync<CrearConsultaResponse>();
        Assert.Equal("Borrador", cuerpo!.Estado);
    }

    [SkippableFact]
    public async Task CrearConsulta_ConCitaValidaDeLaMismaMascota_EsCreada()
    {
        Skip.IfNot(postgres.EstaDisponible, MotivoOmision());
        await AsegurarClientesCompartidosAsync();

        var mascotaVeterinariaId = await CrearMascotaVeterinariaAsync(clienteA, NombreUnico("Firulais"));
        var respuestaCita = await clienteA.PostAsJsonAsync(
            "/api/v1/citas", new CrearCitaRequest(mascotaVeterinariaId, null, usuarioAId, Inicio, Fin, "Control", null));
        var cita = (await respuestaCita.Content.ReadFromJsonAsync<CrearCitaResponse>())!;

        var respuesta = await clienteA.PostAsJsonAsync(
            "/api/v1/consultas",
            new CrearConsultaRequest(mascotaVeterinariaId, cita.CitaId, usuarioAId, Inicio, "Control anual", null, null, null, null, null, null, null));

        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
    }

    [SkippableFact]
    public async Task CrearConsulta_ConCitaDeOtraMascota_Retorna400()
    {
        Skip.IfNot(postgres.EstaDisponible, MotivoOmision());
        await AsegurarClientesCompartidosAsync();

        var mascotaUno = await CrearMascotaVeterinariaAsync(clienteA, NombreUnico("Uno"));
        var mascotaDos = await CrearMascotaVeterinariaAsync(clienteA, NombreUnico("Dos"));
        var respuestaCita = await clienteA.PostAsJsonAsync(
            "/api/v1/citas", new CrearCitaRequest(mascotaUno, null, usuarioAId, Inicio, Fin, "Control", null));
        var cita = (await respuestaCita.Content.ReadFromJsonAsync<CrearCitaResponse>())!;

        var respuesta = await clienteA.PostAsJsonAsync(
            "/api/v1/consultas",
            new CrearConsultaRequest(mascotaDos, cita.CitaId, usuarioAId, Inicio, "Motivo", null, null, null, null, null, null, null));

        Assert.Equal(HttpStatusCode.BadRequest, respuesta.StatusCode);
        var problema = await respuesta.Content.ReadFromJsonAsync<Dictionary<string, object>>();
        Assert.Equal("CONSULTA_CITA_INVALIDA", problema!["codigo"].ToString());
    }

    // ---------- 10 / Prueba critica: aislamiento multi-tenant de consultas ----------

    [SkippableFact]
    public async Task VeterinariaA_NoPuedeConsultarConsultaDeVeterinariaB()
    {
        Skip.IfNot(postgres.EstaDisponible, MotivoOmision());
        await AsegurarClientesCompartidosAsync();

        var mascotaDeB = await CrearMascotaVeterinariaAsync(clienteB, NombreUnico("DeB"));
        var respuestaCrear = await clienteB.PostAsJsonAsync(
            "/api/v1/consultas",
            new CrearConsultaRequest(mascotaDeB, null, usuarioBId, DateTimeOffset.UtcNow, "Consulta de B", null, null, null, null, null, null, null));
        var consultaDeB = (await respuestaCrear.Content.ReadFromJsonAsync<CrearConsultaResponse>())!;

        var respuesta = await clienteA.GetAsync($"/api/v1/consultas/{consultaDeB.ConsultaId}");

        Assert.Equal(HttpStatusCode.NotFound, respuesta.StatusCode);
    }

    // ---------- 12/13. Finalizar consulta (con y sin cita vinculada) ----------

    [SkippableFact]
    public async Task FinalizarConsulta_SinCitaVinculada_CambiaEstadoAFinalizada()
    {
        Skip.IfNot(postgres.EstaDisponible, MotivoOmision());
        await AsegurarClientesCompartidosAsync();

        var mascotaVeterinariaId = await CrearMascotaVeterinariaAsync(clienteA, NombreUnico("Firulais"));
        var respuestaCrear = await clienteA.PostAsJsonAsync(
            "/api/v1/consultas",
            new CrearConsultaRequest(mascotaVeterinariaId, null, usuarioAId, DateTimeOffset.UtcNow, "Motivo", null, null, null, null, null, null, null));
        var consulta = (await respuestaCrear.Content.ReadFromJsonAsync<CrearConsultaResponse>())!;

        var respuestaFinalizar = await clienteA.PostAsync($"/api/v1/consultas/{consulta.ConsultaId}/finalizar", null);
        Assert.Equal(HttpStatusCode.NoContent, respuestaFinalizar.StatusCode);

        var respuestaGet = await clienteA.GetAsync($"/api/v1/consultas/{consulta.ConsultaId}");
        var consultaFinalizada = await respuestaGet.Content.ReadFromJsonAsync<ConsultaResponse>();
        Assert.Equal("Finalizada", consultaFinalizada!.Estado);
    }

    [SkippableFact]
    public async Task FinalizarConsulta_ConCitaEnEspera_MarcaLaCitaComoAtendida()
    {
        Skip.IfNot(postgres.EstaDisponible, MotivoOmision());
        await AsegurarClientesCompartidosAsync();

        var mascotaVeterinariaId = await CrearMascotaVeterinariaAsync(clienteA, NombreUnico("Firulais"));
        var respuestaCita = await clienteA.PostAsJsonAsync(
            "/api/v1/citas", new CrearCitaRequest(mascotaVeterinariaId, null, usuarioAId, Inicio, Fin, "Control", null));
        var cita = (await respuestaCita.Content.ReadFromJsonAsync<CrearCitaResponse>())!;

        (await clienteA.PostAsync($"/api/v1/citas/{cita.CitaId}/en-espera", null)).EnsureSuccessStatusCode();

        var respuestaConsulta = await clienteA.PostAsJsonAsync(
            "/api/v1/consultas",
            new CrearConsultaRequest(mascotaVeterinariaId, cita.CitaId, usuarioAId, Inicio, "Control anual", null, null, null, null, null, null, null));
        var consulta = (await respuestaConsulta.Content.ReadFromJsonAsync<CrearConsultaResponse>())!;

        (await clienteA.PostAsync($"/api/v1/consultas/{consulta.ConsultaId}/finalizar", null)).EnsureSuccessStatusCode();

        var respuestaGetCita = await clienteA.GetAsync($"/api/v1/citas/{cita.CitaId}");
        var citaFinal = await respuestaGetCita.Content.ReadFromJsonAsync<CitaResponse>();
        Assert.Equal("Atendida", citaFinal!.Estado);
    }

    // ---------- AJUSTE 1 (items 6/7): finalizar con cita PROGRAMADA/CONFIRMADA tambien marca ATENDIDA ----------

    [SkippableFact]
    public async Task FinalizarConsulta_ConCitaProgramada_FinalizaAmbasEntidades()
    {
        Skip.IfNot(postgres.EstaDisponible, MotivoOmision());
        await AsegurarClientesCompartidosAsync();

        var mascotaVeterinariaId = await CrearMascotaVeterinariaAsync(clienteA, NombreUnico("Firulais"));
        var respuestaCita = await clienteA.PostAsJsonAsync(
            "/api/v1/citas", new CrearCitaRequest(mascotaVeterinariaId, null, usuarioAId, Inicio, Fin, "Control", null));
        var cita = (await respuestaCita.Content.ReadFromJsonAsync<CrearCitaResponse>())!;
        Assert.Equal("Programada", cita.Estado);

        var respuestaConsulta = await clienteA.PostAsJsonAsync(
            "/api/v1/consultas",
            new CrearConsultaRequest(mascotaVeterinariaId, cita.CitaId, usuarioAId, Inicio, "Control anual", null, null, null, null, null, null, null));
        var consulta = (await respuestaConsulta.Content.ReadFromJsonAsync<CrearConsultaResponse>())!;

        (await clienteA.PostAsync($"/api/v1/consultas/{consulta.ConsultaId}/finalizar", null)).EnsureSuccessStatusCode();

        var consultaFinal = await (await clienteA.GetAsync($"/api/v1/consultas/{consulta.ConsultaId}")).Content.ReadFromJsonAsync<ConsultaResponse>();
        var citaFinal = await (await clienteA.GetAsync($"/api/v1/citas/{cita.CitaId}")).Content.ReadFromJsonAsync<CitaResponse>();
        Assert.Equal("Finalizada", consultaFinal!.Estado);
        Assert.Equal("Atendida", citaFinal!.Estado);
    }

    [SkippableFact]
    public async Task FinalizarConsulta_ConCitaConfirmada_FinalizaAmbasEntidades()
    {
        Skip.IfNot(postgres.EstaDisponible, MotivoOmision());
        await AsegurarClientesCompartidosAsync();

        var mascotaVeterinariaId = await CrearMascotaVeterinariaAsync(clienteA, NombreUnico("Firulais"));
        var respuestaCita = await clienteA.PostAsJsonAsync(
            "/api/v1/citas", new CrearCitaRequest(mascotaVeterinariaId, null, usuarioAId, Inicio, Fin, "Control", null));
        var cita = (await respuestaCita.Content.ReadFromJsonAsync<CrearCitaResponse>())!;

        (await clienteA.PostAsync($"/api/v1/citas/{cita.CitaId}/confirmar", null)).EnsureSuccessStatusCode();

        var respuestaConsulta = await clienteA.PostAsJsonAsync(
            "/api/v1/consultas",
            new CrearConsultaRequest(mascotaVeterinariaId, cita.CitaId, usuarioAId, Inicio, "Control anual", null, null, null, null, null, null, null));
        var consulta = (await respuestaConsulta.Content.ReadFromJsonAsync<CrearConsultaResponse>())!;

        (await clienteA.PostAsync($"/api/v1/consultas/{consulta.ConsultaId}/finalizar", null)).EnsureSuccessStatusCode();

        var consultaFinal = await (await clienteA.GetAsync($"/api/v1/consultas/{consulta.ConsultaId}")).Content.ReadFromJsonAsync<ConsultaResponse>();
        var citaFinal = await (await clienteA.GetAsync($"/api/v1/citas/{cita.CitaId}")).Content.ReadFromJsonAsync<CitaResponse>();
        Assert.Equal("Finalizada", consultaFinal!.Estado);
        Assert.Equal("Atendida", citaFinal!.Estado);
    }

    // ---------- 14 / Prueba critica transaccional ----------

    [SkippableFact]
    public async Task FinalizarConsulta_ConFalloAlMarcarCitaAtendida_HaceRollbackCompleto()
    {
        Skip.IfNot(postgres.EstaDisponible, MotivoOmision());

        using var scope = factory.Services.CreateScope();
        var repositorioConsultas = scope.ServiceProvider.GetRequiredService<IRepositorioConsultas>();
        var repositorioCitas = scope.ServiceProvider.GetRequiredService<IRepositorioCitas>();
        var unidadDeTrabajo = scope.ServiceProvider.GetRequiredService<IUnidadDeTrabajo>();

        var veterinariaId = await CrearVeterinariaActivaAsync();

        // creado_por_usuario_id/veterinario_usuario_id tienen FK reales hacia "usuarios", y
        // mascota_veterinaria_id hacia "mascotas_veterinaria": se necesitan filas reales
        // insertadas de antemano (no Guid.NewGuid() al aire).
        Guid usuarioId;
        Guid mascotaVeterinariaId;
        await using (var contextoSemilla = postgres.CrearDbContext())
        {
            var usuario = Usuario.Crear("Persona de Prueba", $"prueba.rollback.{Guid.NewGuid():N}@correo.com");
            contextoSemilla.Usuarios.Add(usuario);

            var especieId = (await contextoSemilla.Especies.FirstAsync()).Id;
            var mascota = Mascota.Crear($"PET-{Guid.NewGuid():N}"[..10].ToUpperInvariant(), "Firulais", especieId, null, SexoMascota.Macho, null, false, null, null, null);
            contextoSemilla.Mascotas.Add(mascota);

            var mascotaVeterinaria = MascotaVeterinaria.Crear(veterinariaId, mascota.Id);
            contextoSemilla.MascotasVeterinaria.Add(mascotaVeterinaria);

            await contextoSemilla.SaveChangesAsync();

            usuarioId = usuario.Id;
            mascotaVeterinariaId = mascotaVeterinaria.Id;
        }

        // Ajuste post-Etapa 7 (AJUSTE 1): PROGRAMADA/CONFIRMADA/EN_ESPERA ya pueden llegar a
        // ATENDIDA, asi que la cita se deja CANCELADA a proposito -- ese SI sigue siendo un
        // estado realmente terminal desde el que MarcarAtendida debe fallar.
        var cita = Cita.Crear(veterinariaId, mascotaVeterinariaId, null, null, Inicio, Fin, "Motivo", null, usuarioId);
        cita.Cancelar(usuarioId, DateTimeOffset.UtcNow);
        await repositorioCitas.AgregarAsync(cita, CancellationToken.None);

        var consulta = ConsultaVeterinaria.Crear(
            veterinariaId, mascotaVeterinariaId, cita.Id, usuarioId, Inicio, "Motivo",
            null, null, null, null, null, null, null, usuarioId);
        await repositorioConsultas.AgregarAsync(consulta, CancellationToken.None);

        await Assert.ThrowsAsync<Application.Errores.ExcepcionAplicacion>(() =>
            unidadDeTrabajo.EjecutarEnTransaccionAsync(async ct =>
            {
                consulta.Finalizar(usuarioId, DateTimeOffset.UtcNow);
                await repositorioConsultas.GuardarCambiosAsync(ct);

                try
                {
                    cita.MarcarAtendida(usuarioId, DateTimeOffset.UtcNow);
                }
                catch (Domain.Errores.ExcepcionDominio excepcion)
                {
                    throw new Application.Errores.ExcepcionAplicacion("CITA_ESTADO_INVALIDO", excepcion.Message);
                }

                await repositorioCitas.GuardarCambiosAsync(ct);
            }, CancellationToken.None));

        await using var contextoVerificacion = postgres.CrearDbContext();
        var consultaEnBd = await contextoVerificacion.Consultas.SingleAsync(c => c.Id == consulta.Id);
        var citaEnBd = await contextoVerificacion.Citas.SingleAsync(c => c.Id == cita.Id);

        Assert.Equal(EstadoConsulta.Borrador, consultaEnBd.Estado);
        Assert.Equal(EstadoCita.Cancelada, citaEnBd.Estado);
    }

    // ---------- 15/16. Consulta finalizada: no editable, si anulable ----------

    [SkippableFact]
    public async Task ActualizarConsulta_DespuesDeFinalizada_Retorna400()
    {
        Skip.IfNot(postgres.EstaDisponible, MotivoOmision());
        await AsegurarClientesCompartidosAsync();

        var mascotaVeterinariaId = await CrearMascotaVeterinariaAsync(clienteA, NombreUnico("Firulais"));
        var respuestaCrear = await clienteA.PostAsJsonAsync(
            "/api/v1/consultas",
            new CrearConsultaRequest(mascotaVeterinariaId, null, usuarioAId, DateTimeOffset.UtcNow, "Motivo", null, null, null, null, null, null, null));
        var consulta = (await respuestaCrear.Content.ReadFromJsonAsync<CrearConsultaResponse>())!;
        (await clienteA.PostAsync($"/api/v1/consultas/{consulta.ConsultaId}/finalizar", null)).EnsureSuccessStatusCode();

        var respuesta = await clienteA.PutAsJsonAsync(
            $"/api/v1/consultas/{consulta.ConsultaId}",
            new ActualizarConsultaRequest(usuarioAId, DateTimeOffset.UtcNow, "Otro motivo", null, null, null, null, null, null, null));

        Assert.Equal(HttpStatusCode.BadRequest, respuesta.StatusCode);
        var problema = await respuesta.Content.ReadFromJsonAsync<Dictionary<string, object>>();
        Assert.Equal("CONSULTA_FINALIZADA_NO_EDITABLE", problema!["codigo"].ToString());
    }

    [SkippableFact]
    public async Task AnularConsulta_DespuesDeFinalizada_EsPermitido()
    {
        Skip.IfNot(postgres.EstaDisponible, MotivoOmision());
        await AsegurarClientesCompartidosAsync();

        var mascotaVeterinariaId = await CrearMascotaVeterinariaAsync(clienteA, NombreUnico("Firulais"));
        var respuestaCrear = await clienteA.PostAsJsonAsync(
            "/api/v1/consultas",
            new CrearConsultaRequest(mascotaVeterinariaId, null, usuarioAId, DateTimeOffset.UtcNow, "Motivo", null, null, null, null, null, null, null));
        var consulta = (await respuestaCrear.Content.ReadFromJsonAsync<CrearConsultaResponse>())!;
        (await clienteA.PostAsync($"/api/v1/consultas/{consulta.ConsultaId}/finalizar", null)).EnsureSuccessStatusCode();

        var respuesta = await clienteA.PostAsJsonAsync($"/api/v1/consultas/{consulta.ConsultaId}/anular", new AnularConsultaRequest("Registrado por error"));

        Assert.Equal(HttpStatusCode.NoContent, respuesta.StatusCode);

        var respuestaGet = await clienteA.GetAsync($"/api/v1/consultas/{consulta.ConsultaId}");
        var consultaAnulada = await respuestaGet.Content.ReadFromJsonAsync<ConsultaResponse>();
        Assert.Equal("Anulada", consultaAnulada!.Estado);
    }

    // ---------- 17/18. Multiples diagnosticos, solo uno principal ----------

    [SkippableFact]
    public async Task AgregarDiagnosticos_PermiteMultiplesPeroSoloUnPrincipal()
    {
        Skip.IfNot(postgres.EstaDisponible, MotivoOmision());
        await AsegurarClientesCompartidosAsync();

        var mascotaVeterinariaId = await CrearMascotaVeterinariaAsync(clienteA, NombreUnico("Firulais"));
        var respuestaCrear = await clienteA.PostAsJsonAsync(
            "/api/v1/consultas",
            new CrearConsultaRequest(mascotaVeterinariaId, null, usuarioAId, DateTimeOffset.UtcNow, "Motivo", null, null, null, null, null, null, null));
        var consulta = (await respuestaCrear.Content.ReadFromJsonAsync<CrearConsultaResponse>())!;

        var primero = await clienteA.PostAsJsonAsync(
            $"/api/v1/consultas/{consulta.ConsultaId}/diagnosticos", new AgregarDiagnosticoRequest("Otitis externa", "Presuntivo", true));
        primero.EnsureSuccessStatusCode();

        var segundo = await clienteA.PostAsJsonAsync(
            $"/api/v1/consultas/{consulta.ConsultaId}/diagnosticos", new AgregarDiagnosticoRequest("Dermatitis leve", "Diferencial", false));
        segundo.EnsureSuccessStatusCode();

        var tercero = await clienteA.PostAsJsonAsync(
            $"/api/v1/consultas/{consulta.ConsultaId}/diagnosticos", new AgregarDiagnosticoRequest("Otro diagnóstico principal", "Confirmado", true));

        Assert.Equal(HttpStatusCode.Conflict, tercero.StatusCode);
        var problema = await tercero.Content.ReadFromJsonAsync<Dictionary<string, object>>();
        Assert.Equal("DIAGNOSTICO_PRINCIPAL_EXISTENTE", problema!["codigo"].ToString());

        var listaRespuesta = await clienteA.GetAsync($"/api/v1/consultas/{consulta.ConsultaId}/diagnosticos");
        var lista = await listaRespuesta.Content.ReadFromJsonAsync<List<DiagnosticoResponse>>();
        Assert.Equal(2, lista!.Count);
        Assert.Single(lista, d => d.EsPrincipal);
    }

    // ---------- 19. Diagnostico no editable tras finalizacion ----------

    [SkippableFact]
    public async Task ActualizarDiagnostico_DespuesDeFinalizarLaConsulta_Retorna400()
    {
        Skip.IfNot(postgres.EstaDisponible, MotivoOmision());
        await AsegurarClientesCompartidosAsync();

        var mascotaVeterinariaId = await CrearMascotaVeterinariaAsync(clienteA, NombreUnico("Firulais"));
        var respuestaCrear = await clienteA.PostAsJsonAsync(
            "/api/v1/consultas",
            new CrearConsultaRequest(mascotaVeterinariaId, null, usuarioAId, DateTimeOffset.UtcNow, "Motivo", null, null, null, null, null, null, null));
        var consulta = (await respuestaCrear.Content.ReadFromJsonAsync<CrearConsultaResponse>())!;

        var respuestaDiagnostico = await clienteA.PostAsJsonAsync(
            $"/api/v1/consultas/{consulta.ConsultaId}/diagnosticos", new AgregarDiagnosticoRequest("Otitis externa", "Presuntivo", false));
        var diagnostico = (await respuestaDiagnostico.Content.ReadFromJsonAsync<AgregarDiagnosticoResponse>())!;

        (await clienteA.PostAsync($"/api/v1/consultas/{consulta.ConsultaId}/finalizar", null)).EnsureSuccessStatusCode();

        var respuesta = await clienteA.PutAsJsonAsync(
            $"/api/v1/consultas/{consulta.ConsultaId}/diagnosticos/{diagnostico.DiagnosticoId}",
            new ActualizarDiagnosticoRequest("Otitis confirmada", "Confirmado", false));

        Assert.Equal(HttpStatusCode.BadRequest, respuesta.StatusCode);
        var problema = await respuesta.Content.ReadFromJsonAsync<Dictionary<string, object>>();
        Assert.Equal("DIAGNOSTICO_CONSULTA_NO_EDITABLE", problema!["codigo"].ToString());
    }

    // ---------- 20/21 / Prueba critica: historial independiente por veterinaria ----------

    [SkippableFact]
    public async Task HistorialMascota_EsIndependientePorVeterinaria()
    {
        Skip.IfNot(postgres.EstaDisponible, MotivoOmision());
        await AsegurarClientesCompartidosAsync();

        // Mismo nombre de mascota en ambas veterinarias simula al mismo "Toby global" atendido
        // por separado en A y B -- cada MascotaVeterinaria es una ficha totalmente distinta.
        var nombreCompartido = NombreUnico("Toby");
        var mascotaEnA = await CrearMascotaVeterinariaAsync(clienteA, nombreCompartido);
        var mascotaEnB = await CrearMascotaVeterinariaAsync(clienteB, nombreCompartido);

        var motivoA = NombreUnico("ConsultaDeA");
        var motivoB = NombreUnico("ConsultaDeB");

        await clienteA.PostAsJsonAsync(
            "/api/v1/consultas", new CrearConsultaRequest(mascotaEnA, null, usuarioAId, DateTimeOffset.UtcNow, motivoA, null, null, null, null, null, null, null));
        await clienteB.PostAsJsonAsync(
            "/api/v1/consultas", new CrearConsultaRequest(mascotaEnB, null, usuarioBId, DateTimeOffset.UtcNow, motivoB, null, null, null, null, null, null, null));

        var historialA = await (await clienteA.GetAsync($"/api/v1/mascotas/{mascotaEnA}/historial")).Content.ReadFromJsonAsync<HistorialMascotaResponse>();
        var historialB = await (await clienteB.GetAsync($"/api/v1/mascotas/{mascotaEnB}/historial")).Content.ReadFromJsonAsync<HistorialMascotaResponse>();

        Assert.Contains(historialA!.Eventos, e => e.Titulo == motivoA);
        Assert.DoesNotContain(historialA.Eventos, e => e.Titulo == motivoB);
        Assert.Contains(historialB!.Eventos, e => e.Titulo == motivoB);
        Assert.DoesNotContain(historialB.Eventos, e => e.Titulo == motivoA);

        // A no puede ni siquiera consultar el historial de la ficha de B por su UUID.
        var respuestaCruzada = await clienteA.GetAsync($"/api/v1/mascotas/{mascotaEnB}/historial");
        Assert.Equal(HttpStatusCode.NotFound, respuestaCruzada.StatusCode);
    }

    // ---------- AJUSTE 2: permiso dedicado clinica.ejercer_veterinario ----------

    private async Task<Guid> RegistrarUsuarioConRolesAsync(Guid veterinariaId, params string[] codigosRoles)
    {
        using var clienteRegistro = factory.CreateClient();
        var (usuarioId, _) = await RegistrarYConfirmarAsync(clienteRegistro);
        await CrearMembresiaActivaAsync(usuarioId, veterinariaId, codigosRoles);
        return usuarioId;
    }

    [SkippableFact]
    public async Task CrearCita_ConVeterinarioQueTieneClinicaEjercerVeterinario_LoAsigna()
    {
        Skip.IfNot(postgres.EstaDisponible, MotivoOmision());
        await AsegurarClientesCompartidosAsync();

        var usuarioVeterinario = await RegistrarUsuarioConRolesAsync(veterinariaAId, "VETERINARIO");
        var mascotaVeterinariaId = await CrearMascotaVeterinariaAsync(clienteA, NombreUnico("Firulais"));

        var respuesta = await clienteA.PostAsJsonAsync(
            "/api/v1/citas", new CrearCitaRequest(mascotaVeterinariaId, null, usuarioVeterinario, Inicio, Fin, "Control", null));

        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
    }

    [SkippableFact]
    public async Task CrearCita_ConAdministradorSinRolVeterinario_Retorna400()
    {
        Skip.IfNot(postgres.EstaDisponible, MotivoOmision());
        await AsegurarClientesCompartidosAsync();

        var usuarioAdministrador = await RegistrarUsuarioConRolesAsync(veterinariaAId, "ADMINISTRADOR_VETERINARIA");
        var mascotaVeterinariaId = await CrearMascotaVeterinariaAsync(clienteA, NombreUnico("Firulais"));

        var respuesta = await clienteA.PostAsJsonAsync(
            "/api/v1/citas", new CrearCitaRequest(mascotaVeterinariaId, null, usuarioAdministrador, Inicio, Fin, "Control", null));

        Assert.Equal(HttpStatusCode.BadRequest, respuesta.StatusCode);
        var problema = await respuesta.Content.ReadFromJsonAsync<Dictionary<string, object>>();
        Assert.Equal("CITA_VETERINARIO_INVALIDO", problema!["codigo"].ToString());
    }

    [SkippableFact]
    public async Task CrearCita_ConAsistente_Retorna400()
    {
        Skip.IfNot(postgres.EstaDisponible, MotivoOmision());
        await AsegurarClientesCompartidosAsync();

        var usuarioAsistente = await RegistrarUsuarioConRolesAsync(veterinariaAId, "ASISTENTE");
        var mascotaVeterinariaId = await CrearMascotaVeterinariaAsync(clienteA, NombreUnico("Firulais"));

        var respuesta = await clienteA.PostAsJsonAsync(
            "/api/v1/citas", new CrearCitaRequest(mascotaVeterinariaId, null, usuarioAsistente, Inicio, Fin, "Control", null));

        Assert.Equal(HttpStatusCode.BadRequest, respuesta.StatusCode);
        var problema = await respuesta.Content.ReadFromJsonAsync<Dictionary<string, object>>();
        Assert.Equal("CITA_VETERINARIO_INVALIDO", problema!["codigo"].ToString());
    }

    [SkippableFact]
    public async Task CrearCita_ConAdministradorQueTambienEsVeterinario_LoAsigna()
    {
        Skip.IfNot(postgres.EstaDisponible, MotivoOmision());
        await AsegurarClientesCompartidosAsync();

        var usuarioAdminVeterinario = await RegistrarUsuarioConRolesAsync(veterinariaAId, "ADMINISTRADOR_VETERINARIA", "VETERINARIO");
        var mascotaVeterinariaId = await CrearMascotaVeterinariaAsync(clienteA, NombreUnico("Firulais"));

        var respuesta = await clienteA.PostAsJsonAsync(
            "/api/v1/citas", new CrearCitaRequest(mascotaVeterinariaId, null, usuarioAdminVeterinario, Inicio, Fin, "Control", null));

        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
    }

    [SkippableFact]
    public async Task QuitarRolVeterinario_ImpideFuturasAsignaciones_SinRequerirJwtNuevo()
    {
        Skip.IfNot(postgres.EstaDisponible, MotivoOmision());
        await AsegurarClientesCompartidosAsync();

        var usuarioVeterinario = await RegistrarUsuarioConRolesAsync(veterinariaAId, "VETERINARIO");
        var mascotaVeterinariaId = await CrearMascotaVeterinariaAsync(clienteA, NombreUnico("Firulais"));

        // clienteA nunca vuelve a autenticarse en este test: su JWT es el mismo en las dos
        // llamadas de abajo, antes y despues de quitarle el rol VETERINARIO al usuario objetivo.
        var respuestaAntes = await clienteA.PostAsJsonAsync(
            "/api/v1/citas", new CrearCitaRequest(mascotaVeterinariaId, null, usuarioVeterinario, Inicio, Fin, "Control", null));
        Assert.Equal(HttpStatusCode.OK, respuestaAntes.StatusCode);

        await using (var contexto = postgres.CrearDbContext())
        {
            var membresia = await contexto.MembresiasVeterinaria
                .Include(m => m.Roles)
                .SingleAsync(m => m.UsuarioId == usuarioVeterinario && m.VeterinariaId == veterinariaAId);
            var rolVeterinario = await contexto.Roles.SingleAsync(r => r.Codigo == "VETERINARIO");
            membresia.QuitarRol(rolVeterinario.Id);
            await contexto.SaveChangesAsync();
        }

        var respuestaDespues = await clienteA.PostAsJsonAsync(
            "/api/v1/citas", new CrearCitaRequest(mascotaVeterinariaId, null, usuarioVeterinario, Inicio, Fin, "Otro control", null));

        Assert.Equal(HttpStatusCode.BadRequest, respuestaDespues.StatusCode);
        var problema = await respuestaDespues.Content.ReadFromJsonAsync<Dictionary<string, object>>();
        Assert.Equal("CITA_VETERINARIO_INVALIDO", problema!["codigo"].ToString());
    }
}
