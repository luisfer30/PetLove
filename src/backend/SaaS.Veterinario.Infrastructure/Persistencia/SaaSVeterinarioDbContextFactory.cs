using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace SaaS.Veterinario.Infrastructure.Persistencia;

/// <summary>
/// Factory de disenio usada por las herramientas de "dotnet ef" (migrations add/database update/
/// migrations script) para poder crear el DbContext sin depender del arranque completo de la Api
/// ni de su contenedor de DI. La cadena de conexion nunca se hardcodea: se lee de
/// ConnectionStrings:SaaSVeterinario via appsettings/variables de entorno, con un valor de
/// desarrollo local (Postgres por defecto en localhost) solo como ultimo recurso.
/// </summary>
public sealed class SaaSVeterinarioDbContextFactory : IDesignTimeDbContextFactory<SaaSVeterinarioDbContext>
{
    private const string CadenaConexionLocalPorDefecto =
        "Host=localhost;Port=5432;Database=saas_veterinario;Username=postgres;Password=postgres";

    public SaaSVeterinarioDbContext CreateDbContext(string[] args)
    {
        var entorno = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";

        var configuracion = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile($"appsettings.{entorno}.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var cadenaConexion = configuracion.GetConnectionString("SaaSVeterinario");

        if (string.IsNullOrWhiteSpace(cadenaConexion))
        {
            cadenaConexion = CadenaConexionLocalPorDefecto;
        }

        var opciones = new DbContextOptionsBuilder<SaaSVeterinarioDbContext>()
            .UseNpgsql(cadenaConexion)
            .Options;

        return new SaaSVeterinarioDbContext(opciones);
    }
}
