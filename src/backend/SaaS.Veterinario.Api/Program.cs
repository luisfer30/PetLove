using Microsoft.EntityFrameworkCore;
using SaaS.Veterinario.Api.Autenticacion;
using SaaS.Veterinario.Api.ContextoVeterinario;
using SaaS.Veterinario.Api.Errores;
using SaaS.Veterinario.Api.Personal;
using SaaS.Veterinario.Api.Usuarios;
using SaaS.Veterinario.Api.Veterinarias;
using SaaS.Veterinario.Api.Extensiones;
using SaaS.Veterinario.Infrastructure.Persistencia;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

builder.Services.AddDbContext<SaaSVeterinarioDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("SaaSVeterinario")));

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<ManejadorExcepciones>();

builder.Services.AgregarIdentidad();
builder.Services.AgregarAutenticacionJwt(builder.Configuration); // registra AddAuthorization()
builder.Services.AgregarLimitadorDeVelocidad(builder.Configuration);
builder.Services.AgregarCasosDeUsoDeAutenticacion();
builder.Services.AgregarContextoVeterinario();
builder.Services.AgregarAutorizacionPorPermisos(); // debe ir despues de AgregarAutenticacionJwt

var app = builder.Build();

app.UseMiddleware<CorrelationIdMiddleware>();
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseRateLimiter();

app.UseAuthentication();
app.UseMiddleware<ContextoVeterinarioMiddleware>(); // despues de autenticar, antes de autorizar
app.UseAuthorization();

app.MapearEndpointsAutenticacion();
app.MapearEndpointsUsuarios();
app.MapearEndpointsVeterinarias();
app.MapearEndpointsPersonal();

app.Run();

// Necesario para que WebApplicationFactory<Program> (pruebas de integracion) pueda
// referenciar esta clase de entrada generada por top-level statements desde otro ensamblado.
public partial class Program;
