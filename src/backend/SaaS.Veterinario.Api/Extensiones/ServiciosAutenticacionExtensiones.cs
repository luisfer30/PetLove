using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SaaS.Veterinario.Api.Autenticacion;
using SaaS.Veterinario.Application.Abstracciones;
using SaaS.Veterinario.Application.Autenticacion;
using SaaS.Veterinario.Infrastructure.Identidad;
using SaaS.Veterinario.Infrastructure.Persistencia;

namespace SaaS.Veterinario.Api.Extensiones;

public static class ServiciosAutenticacionExtensiones
{
    /// <summary>
    /// AddIdentityCore (no AddIdentity): no necesitamos cookies de sign-in ni el sistema de
    /// Roles de Identity (la autorizacion de esta plataforma usa Rol/Permiso de dominio).
    /// La politica de password es solo longitud minima (sin exigir mayusculas/digitos/
    /// simbolos) y el lockout es el propuesto: 5 intentos, 15 minutos.
    /// </summary>
    public static IServiceCollection AgregarIdentidad(this IServiceCollection servicios)
    {
        servicios.AddIdentityCore<UsuarioIdentidad>(opciones =>
            {
                opciones.User.RequireUniqueEmail = true;

                opciones.Password.RequiredLength = 8;
                opciones.Password.RequireDigit = false;
                opciones.Password.RequireLowercase = false;
                opciones.Password.RequireUppercase = false;
                opciones.Password.RequireNonAlphanumeric = false;

                opciones.Lockout.MaxFailedAccessAttempts = 5;
                opciones.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
                opciones.Lockout.AllowedForNewUsers = true;
            })
            .AddEntityFrameworkStores<SaaSVeterinarioDbContext>()
            .AddDefaultTokenProviders();

        return servicios;
    }

    /// <summary>
    /// JWT Bearer como (unico) esquema de autenticacion. El secreto de firma sale de
    /// configuracion (User Secrets en Development, variable de entorno en produccion);
    /// nunca esta escrito en appsettings.json.
    ///
    /// TokenValidationParameters se arma dentro de un IConfigureOptions&lt;JwtBearerOptions&gt;
    /// que depende de IOptions&lt;JwtOptions&gt; -- es decir, se resuelve perezosamente, la
    /// primera vez que el middleware de autenticacion necesita JwtBearerOptions -- en vez de
    /// leer IConfiguration de forma sincrona y anticipada durante el registro de servicios.
    /// Esto importa porque ServicioTokens (quien FIRMA los tokens) tambien lee JwtOptions via
    /// IOptions perezoso: si la validacion leyera la configuracion en un momento distinto
    /// (mas temprano) que la firma, un origen de configuracion agregado despues (como el que
    /// usan las pruebas de integracion para inyectar una clave de firma de prueba) podria
    /// quedar visible para uno y no para el otro, y todo token valido se rechazaria.
    /// </summary>
    public static IServiceCollection AgregarAutenticacionJwt(this IServiceCollection servicios, IConfiguration configuracion)
    {
        servicios
            .AddOptions<JwtOptions>()
            .Bind(configuracion.GetSection(JwtOptions.Seccion))
            .ValidateOnStart();

        servicios.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer();

        servicios.AddSingleton<IConfigureOptions<JwtBearerOptions>>(sp =>
        {
            var jwtOptions = sp.GetRequiredService<IOptions<JwtOptions>>().Value;

            return new ConfigureNamedOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme, opciones =>
            {
                // JwtSecurityTokenHandler remapea por defecto claims cortos ("sub", "email")
                // a los tipos largos de ClaimTypes (NameIdentifier, Email). Se desactiva para
                // que HttpContext.User conserve exactamente los nombres de claim del JWT.
                opciones.MapInboundClaims = false;

                opciones.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = jwtOptions.Issuer,
                    ValidateAudience = true,
                    ValidAudience = jwtOptions.Audience,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SigningKey)),
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromSeconds(30),
                };
            });
        });

        servicios.AddAuthorization();

        return servicios;
    }

    /// <summary>
    /// Rate limiting basico para los endpoints sensibles a fuerza bruta (login, registro,
    /// solicitud de recuperacion). Limite configurable por ambiente vía appsettings; el
    /// valor por defecto es generoso para no volver impredecibles los tests de integracion.
    /// </summary>
    public static IServiceCollection AgregarLimitadorDeVelocidad(this IServiceCollection servicios, IConfiguration configuracion)
    {
        var permitLimit = configuracion.GetValue("RateLimiting:AutenticacionSensible:PermitLimit", 30);
        var ventanaSegundos = configuracion.GetValue("RateLimiting:AutenticacionSensible:VentanaSegundos", 60);

        servicios.AddRateLimiter(opciones =>
        {
            opciones.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            opciones.AddFixedWindowLimiter(EndpointsAutenticacion.PoliticaSensible, limiteOpciones =>
            {
                limiteOpciones.PermitLimit = permitLimit;
                limiteOpciones.Window = TimeSpan.FromSeconds(ventanaSegundos);
                limiteOpciones.QueueLimit = 0;
            });
        });

        return servicios;
    }

    /// <summary>
    /// Registra las abstracciones de Application con sus implementaciones de Infrastructure,
    /// y los casos de uso de autenticacion (clases simples, sin MediatR).
    /// </summary>
    public static IServiceCollection AgregarCasosDeUsoDeAutenticacion(this IServiceCollection servicios)
    {
        servicios.AddScoped<IRepositorioUsuarios, RepositorioUsuarios>();
        servicios.AddScoped<IUnidadDeTrabajo, UnidadDeTrabajoEfCore>();
        servicios.AddScoped<IServicioIdentidad, ServicioIdentidad>();
        servicios.AddScoped<IServicioTokens, ServicioTokens>();
        servicios.AddScoped<IServicioCorreo, ServicioCorreoDesarrollo>();

        servicios.AddScoped<RegistrarUsuario>();
        servicios.AddScoped<IniciarSesion>();
        servicios.AddScoped<RenovarToken>();
        servicios.AddScoped<CerrarSesion>();
        servicios.AddScoped<CerrarTodasLasSesiones>();
        servicios.AddScoped<ConfirmarCorreo>();
        servicios.AddScoped<SolicitarRecuperacionPassword>();
        servicios.AddScoped<RestablecerPassword>();

        return servicios;
    }
}
