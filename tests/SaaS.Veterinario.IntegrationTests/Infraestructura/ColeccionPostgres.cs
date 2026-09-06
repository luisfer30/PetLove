namespace SaaS.Veterinario.IntegrationTests.Infraestructura;

/// <summary>
/// Todas las clases de prueba de persistencia comparten UN solo contenedor de PostgreSQL
/// (via esta coleccion), en vez de levantar uno por clase. xUnit ejecuta las clases de una
/// misma coleccion de forma secuencial, evitando condiciones de carrera sobre el contenedor.
/// </summary>
[CollectionDefinition(Nombre)]
public sealed class ColeccionPostgres : ICollectionFixture<PostgresContainerFixture>
{
    public const string Nombre = "Postgres";
}
