using Microsoft.EntityFrameworkCore;
using SaaS.Veterinario.Domain.Permisos;
using SaaS.Veterinario.Domain.Roles;
using SaaS.Veterinario.IntegrationTests.Infraestructura;

namespace SaaS.Veterinario.IntegrationTests.Persistencia;

[Collection(ColeccionPostgres.Nombre)]
public sealed class RolPermisoPersistenciaTests(PostgresContainerFixture fixture)
{
    [SkippableFact]
    public async Task AsignarPermiso_ConVariosPermisos_PersisteTodosLosPermisos()
    {
        Skip.IfNot(fixture.EstaDisponible, $"Docker no disponible en este entorno: {fixture.MotivoNoDisponible}");

        var permiso1 = Permiso.Crear($"prueba.permiso1.{Guid.NewGuid():N}", "Permiso de prueba 1");
        var permiso2 = Permiso.Crear($"prueba.permiso2.{Guid.NewGuid():N}", "Permiso de prueba 2");
        var rol = Rol.Crear($"ROL_PRUEBA_{Guid.NewGuid():N}", "Rol de prueba");
        rol.AsignarPermiso(permiso1.Id);
        rol.AsignarPermiso(permiso2.Id);

        await using (var contexto = fixture.CrearDbContext())
        {
            contexto.Permisos.AddRange(permiso1, permiso2);
            contexto.Roles.Add(rol);
            await contexto.SaveChangesAsync();
        }

        await using (var contexto = fixture.CrearDbContext())
        {
            var rolGuardado = await contexto.Roles
                .Include(r => r.Permisos)
                .SingleAsync(r => r.Id == rol.Id);

            Assert.Equal(2, rolGuardado.Permisos.Count);
        }
    }

    [SkippableFact]
    public async Task RolPermiso_ConFilaDuplicadaInsertadaDirectamente_LanzaExcepcionDePersistencia()
    {
        Skip.IfNot(fixture.EstaDisponible, $"Docker no disponible en este entorno: {fixture.MotivoNoDisponible}");

        var permiso = Permiso.Crear($"prueba.permiso.{Guid.NewGuid():N}", "Permiso de prueba");
        var rol = Rol.Crear($"ROL_PRUEBA_{Guid.NewGuid():N}", "Rol de prueba");
        rol.AsignarPermiso(permiso.Id);

        await using var contexto = fixture.CrearDbContext();
        contexto.Permisos.Add(permiso);
        contexto.Roles.Add(rol);
        await contexto.SaveChangesAsync();

        // Igual que con MembresiaRol: RolPermiso tiene constructor internal, asi que se
        // inserta la fila puente directamente por SQL para probar la PK compuesta en BD.
        await using var otroContexto = fixture.CrearDbContext();
        var conexion = otroContexto.Database.GetDbConnection();
        await conexion.OpenAsync();
        await using var comando = conexion.CreateCommand();
        comando.CommandText = "INSERT INTO roles_permisos (rol_id, permiso_id) VALUES (@rolId, @permisoId)";

        var parametroRol = comando.CreateParameter();
        parametroRol.ParameterName = "rolId";
        parametroRol.Value = rol.Id;
        comando.Parameters.Add(parametroRol);

        var parametroPermiso = comando.CreateParameter();
        parametroPermiso.ParameterName = "permisoId";
        parametroPermiso.Value = permiso.Id;
        comando.Parameters.Add(parametroPermiso);

        await Assert.ThrowsAnyAsync<Exception>(() => comando.ExecuteNonQueryAsync());
    }
}
