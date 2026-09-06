using Microsoft.EntityFrameworkCore;
using SaaS.Veterinario.Domain.Membresias;
using SaaS.Veterinario.Domain.Usuarios;
using SaaS.Veterinario.Domain.Veterinarias;
using SaaS.Veterinario.Infrastructure.Persistencia;
using SaaS.Veterinario.IntegrationTests.Infraestructura;

namespace SaaS.Veterinario.IntegrationTests.Persistencia;

[Collection(ColeccionPostgres.Nombre)]
public sealed class MembresiaVeterinariaPersistenciaTests(PostgresContainerFixture fixture)
{
    // Deben coincidir con los Guid fijos sembrados en RolesPermisosSeed (Infrastructure);
    // se repiten aqui en vez de referenciar esa clase internal para no acoplar el proyecto
    // de pruebas a un detalle interno de Infrastructure.
    private static readonly Guid RolVeterinarioId = new("11111111-0000-0000-0000-000000000002");
    private static readonly Guid RolAsistenteId = new("11111111-0000-0000-0000-000000000003");

    private static string GenerarCodigoPublico() => $"VET-{Guid.NewGuid():N}"[..10].ToUpperInvariant();

    private static async Task<(Guid usuarioId, Guid veterinariaId)> CrearUsuarioYVeterinariaAsync(SaaSVeterinarioDbContext contexto)
    {
        var usuario = Usuario.Crear("Ana Perez", $"ana.{Guid.NewGuid():N}@correo.com");
        var veterinaria = Veterinaria.Crear(
            GenerarCodigoPublico(),
            "Clinica Test",
            $"contacto.{Guid.NewGuid():N}@clinica.com",
            "Ecuador",
            "America/Guayaquil");

        contexto.Usuarios.Add(usuario);
        contexto.Veterinarias.Add(veterinaria);
        await contexto.SaveChangesAsync();

        return (usuario.Id, veterinaria.Id);
    }

    [SkippableFact]
    public async Task GuardarMembresia_PersisteYRecupera()
    {
        Skip.IfNot(fixture.EstaDisponible, $"Docker no disponible en este entorno: {fixture.MotivoNoDisponible}");

        await using var contexto = fixture.CrearDbContext();
        var (usuarioId, veterinariaId) = await CrearUsuarioYVeterinariaAsync(contexto);

        var membresia = MembresiaVeterinaria.Crear(usuarioId, veterinariaId);
        contexto.MembresiasVeterinaria.Add(membresia);
        await contexto.SaveChangesAsync();

        await using var otroContexto = fixture.CrearDbContext();
        var guardada = await otroContexto.MembresiasVeterinaria.SingleAsync(m => m.Id == membresia.Id);

        Assert.Equal(EstadoMembresiaVeterinaria.Invitada, guardada.Estado);
    }

    [SkippableFact]
    public async Task GuardarMembresia_PermiteMultiplesMembresiasHistoricasParaElMismoUsuario()
    {
        Skip.IfNot(fixture.EstaDisponible, $"Docker no disponible en este entorno: {fixture.MotivoNoDisponible}");

        await using var contexto = fixture.CrearDbContext();
        var (usuarioId, veterinariaId1) = await CrearUsuarioYVeterinariaAsync(contexto);
        var (_, veterinariaId2) = await CrearUsuarioYVeterinariaAsync(contexto);

        var membresia1 = MembresiaVeterinaria.Crear(usuarioId, veterinariaId1);
        membresia1.Activar(DateTimeOffset.UtcNow);
        membresia1.Finalizar(DateTimeOffset.UtcNow);

        var membresia2 = MembresiaVeterinaria.Crear(usuarioId, veterinariaId2);
        membresia2.Activar(DateTimeOffset.UtcNow);
        membresia2.Finalizar(DateTimeOffset.UtcNow);

        contexto.MembresiasVeterinaria.AddRange(membresia1, membresia2);
        await contexto.SaveChangesAsync();

        var total = await contexto.MembresiasVeterinaria.CountAsync(m => m.UsuarioId == usuarioId);
        Assert.Equal(2, total);
    }

    [SkippableFact]
    public async Task GuardarMembresia_ConDosActivasParaElMismoUsuario_LanzaExcepcionDePersistencia()
    {
        Skip.IfNot(fixture.EstaDisponible, $"Docker no disponible en este entorno: {fixture.MotivoNoDisponible}");

        await using var contextoSetup = fixture.CrearDbContext();
        var (usuarioId, veterinariaId1) = await CrearUsuarioYVeterinariaAsync(contextoSetup);
        var (_, veterinariaId2) = await CrearUsuarioYVeterinariaAsync(contextoSetup);

        var primeraActiva = MembresiaVeterinaria.Crear(usuarioId, veterinariaId1);
        primeraActiva.Activar(DateTimeOffset.UtcNow);
        contextoSetup.MembresiasVeterinaria.Add(primeraActiva);
        await contextoSetup.SaveChangesAsync();

        await using var otroContexto = fixture.CrearDbContext();
        var segundaActiva = MembresiaVeterinaria.Crear(usuarioId, veterinariaId2);
        segundaActiva.Activar(DateTimeOffset.UtcNow);
        otroContexto.MembresiasVeterinaria.Add(segundaActiva);

        // Segunda barrera: el indice unico parcial en BD debe rechazar esto aunque la
        // aplicacion no haya validado antes (esa validacion de aplicacion se hace en
        // etapas posteriores, junto con los casos de uso).
        await Assert.ThrowsAsync<DbUpdateException>(() => otroContexto.SaveChangesAsync());
    }

    [SkippableFact]
    public async Task AsignarRol_ConVariosRoles_PersisteTodosLosRoles()
    {
        Skip.IfNot(fixture.EstaDisponible, $"Docker no disponible en este entorno: {fixture.MotivoNoDisponible}");

        await using var contexto = fixture.CrearDbContext();
        var (usuarioId, veterinariaId) = await CrearUsuarioYVeterinariaAsync(contexto);

        var membresia = MembresiaVeterinaria.Crear(usuarioId, veterinariaId);
        membresia.AsignarRol(RolVeterinarioId);
        membresia.AsignarRol(RolAsistenteId);

        contexto.MembresiasVeterinaria.Add(membresia);
        await contexto.SaveChangesAsync();

        await using var otroContexto = fixture.CrearDbContext();
        var guardada = await otroContexto.MembresiasVeterinaria
            .Include(m => m.Roles)
            .SingleAsync(m => m.Id == membresia.Id);

        Assert.Equal(2, guardada.Roles.Count);
        Assert.Contains(guardada.Roles, r => r.RolId == RolVeterinarioId);
        Assert.Contains(guardada.Roles, r => r.RolId == RolAsistenteId);
    }

    [SkippableFact]
    public async Task MembresiaRol_ConFilaDuplicadaInsertadaDirectamente_LanzaExcepcionDePersistencia()
    {
        Skip.IfNot(fixture.EstaDisponible, $"Docker no disponible en este entorno: {fixture.MotivoNoDisponible}");

        await using var contexto = fixture.CrearDbContext();
        var (usuarioId, veterinariaId) = await CrearUsuarioYVeterinariaAsync(contexto);

        var membresia = MembresiaVeterinaria.Crear(usuarioId, veterinariaId);
        membresia.AsignarRol(RolVeterinarioId);
        contexto.MembresiasVeterinaria.Add(membresia);
        await contexto.SaveChangesAsync();

        // El dominio ya impide duplicar un rol en memoria (MembresiaVeterinaria.AsignarRol,
        // cubierto en Domain.Tests). Como MembresiaRol tiene constructor internal, no se
        // puede construir aqui para forzar el duplicado via EF; se inserta la fila puente
        // directamente por SQL para probar la segunda barrera: la PK compuesta en BD.
        await using var otroContexto = fixture.CrearDbContext();
        var conexion = otroContexto.Database.GetDbConnection();
        await conexion.OpenAsync();
        await using var comando = conexion.CreateCommand();
        comando.CommandText = "INSERT INTO membresias_roles (membresia_id, rol_id) VALUES (@membresiaId, @rolId)";

        var parametroMembresia = comando.CreateParameter();
        parametroMembresia.ParameterName = "membresiaId";
        parametroMembresia.Value = membresia.Id;
        comando.Parameters.Add(parametroMembresia);

        var parametroRol = comando.CreateParameter();
        parametroRol.ParameterName = "rolId";
        parametroRol.Value = RolVeterinarioId;
        comando.Parameters.Add(parametroRol);

        await Assert.ThrowsAnyAsync<Exception>(() => comando.ExecuteNonQueryAsync());
    }
}
