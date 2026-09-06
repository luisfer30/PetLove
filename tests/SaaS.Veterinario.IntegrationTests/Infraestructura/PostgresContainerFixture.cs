using Microsoft.EntityFrameworkCore;
using SaaS.Veterinario.Infrastructure.Persistencia;
using Testcontainers.PostgreSql;

namespace SaaS.Veterinario.IntegrationTests.Infraestructura;

/// <summary>
/// Arranca un contenedor real de PostgreSQL (via Testcontainers) una sola vez para toda la
/// coleccion de pruebas y aplica la migracion inicial contra el. Si Docker no esta disponible
/// en el entorno, NO se sustituye Postgres por SQLite ni ningun otro motor: en su lugar,
/// EstaDisponible queda en false y cada [SkippableFact] omite la prueba en vez de fallar.
/// </summary>
public sealed class PostgresContainerFixture : IAsyncLifetime
{
    private PostgreSqlContainer? _contenedor;

    public bool EstaDisponible { get; private set; }

    public string? MotivoNoDisponible { get; private set; }

    public async Task InitializeAsync()
    {
        try
        {
            // Build() ya valida sincronamente si el endpoint de Docker responde y lanza
            // DockerUnavailableException si no -- por eso va DENTRO del try, no solo StartAsync().
            _contenedor = new PostgreSqlBuilder("postgres:16-alpine")
                .WithDatabase("saas_veterinario_tests")
                .WithUsername("postgres")
                .WithPassword("postgres")
                .Build();

            await _contenedor.StartAsync();

            await using var dbContext = CrearDbContext();
            await dbContext.Database.MigrateAsync();

            EstaDisponible = true;
        }
        catch (Exception ex)
        {
            EstaDisponible = false;
            MotivoNoDisponible = ex.Message;
        }
    }

    public async Task DisposeAsync()
    {
        if (_contenedor is not null)
        {
            await _contenedor.DisposeAsync();
        }
    }

    public SaaSVeterinarioDbContext CrearDbContext()
    {
        if (_contenedor is null)
        {
            throw new InvalidOperationException("El contenedor de PostgreSQL no se ha inicializado.");
        }

        var opciones = new DbContextOptionsBuilder<SaaSVeterinarioDbContext>()
            .UseNpgsql(_contenedor.GetConnectionString())
            .Options;

        return new SaaSVeterinarioDbContext(opciones);
    }
}
