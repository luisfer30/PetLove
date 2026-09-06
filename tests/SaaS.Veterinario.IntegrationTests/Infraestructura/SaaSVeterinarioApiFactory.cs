using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace SaaS.Veterinario.IntegrationTests.Infraestructura;

/// <summary>
/// Levanta la Api completa (TestServer en memoria) apuntando al contenedor de PostgreSQL de
/// PostgresContainerFixture, con configuracion de Jwt/rate-limiting propia de pruebas.
///
/// No recibe PostgresContainerFixture por constructor: xUnit v2 no permite que un collection
/// fixture dependa de otro (a diferencia de una clase de prueba, que si puede recibir varios
/// collection fixtures). Por eso el enlace se hace via UsarPostgres(...), que cada clase de
/// prueba llama en su propio constructor -- para ese momento PostgresContainerFixture.
/// InitializeAsync() ya termino, y ConfigureWebHost (que lee la cadena de conexion) solo se
/// invoca de forma perezosa cuando un test pide CreateClient().
/// </summary>
public sealed class SaaSVeterinarioApiFactory : WebApplicationFactory<Program>
{
    private PostgresContainerFixture? _postgres;

    public void UsarPostgres(PostgresContainerFixture postgres) => _postgres = postgres;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        if (_postgres is null || !_postgres.EstaDisponible)
        {
            return;
        }

        builder.ConfigureAppConfiguration((_, configuracion) =>
        {
            configuracion.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:SaaSVeterinario"] = _postgres.ObtenerCadenaConexion(),
                ["Jwt:Issuer"] = "SaaS.Veterinario.Tests",
                ["Jwt:Audience"] = "SaaS.Veterinario.Tests.Clientes",
                ["Jwt:SigningKey"] = "clave-de-firma-unicamente-para-pruebas-de-integracion-0123456789",
                ["Jwt:AccessTokenMinutes"] = "15",
                ["Jwt:RefreshTokenDays"] = "30",
                ["RateLimiting:AutenticacionSensible:PermitLimit"] = "1000",
                ["RateLimiting:AutenticacionSensible:VentanaSegundos"] = "60",
            });
        });
    }
}
