using Microsoft.EntityFrameworkCore;
using SaaS.Veterinario.Domain.Veterinarias;
using SaaS.Veterinario.IntegrationTests.Infraestructura;

namespace SaaS.Veterinario.IntegrationTests.Persistencia;

[Collection(ColeccionPostgres.Nombre)]
public sealed class VeterinariaPersistenciaTests(PostgresContainerFixture fixture)
{
    private static string GenerarCodigoPublico() => $"VET-{Guid.NewGuid():N}"[..10].ToUpperInvariant();

    [SkippableFact]
    public async Task GuardarVeterinaria_PersisteYRecupera()
    {
        Skip.IfNot(fixture.EstaDisponible, $"Docker no disponible en este entorno: {fixture.MotivoNoDisponible}");

        var codigo = GenerarCodigoPublico();
        var veterinaria = Veterinaria.Crear(codigo, "Clinica Test", "contacto@clinicatest.com", "Ecuador", "America/Guayaquil");

        await using (var contexto = fixture.CrearDbContext())
        {
            contexto.Veterinarias.Add(veterinaria);
            await contexto.SaveChangesAsync();
        }

        await using (var contexto = fixture.CrearDbContext())
        {
            var guardada = await contexto.Veterinarias.SingleAsync(v => v.Id == veterinaria.Id);

            Assert.Equal(codigo, guardada.CodigoPublico.Valor);
            Assert.Equal(EstadoVeterinaria.PendienteActivacion, guardada.Estado);
        }
    }

    [SkippableFact]
    public async Task GuardarVeterinaria_ConCodigoPublicoDuplicado_LanzaExcepcionDePersistencia()
    {
        Skip.IfNot(fixture.EstaDisponible, $"Docker no disponible en este entorno: {fixture.MotivoNoDisponible}");

        var codigo = GenerarCodigoPublico();

        await using (var contexto = fixture.CrearDbContext())
        {
            contexto.Veterinarias.Add(Veterinaria.Crear(codigo, "Clinica Uno", "uno@clinica.com", "Ecuador", "America/Guayaquil"));
            await contexto.SaveChangesAsync();
        }

        await using var otroContexto = fixture.CrearDbContext();
        otroContexto.Veterinarias.Add(Veterinaria.Crear(codigo, "Clinica Dos", "dos@clinica.com", "Ecuador", "America/Guayaquil"));

        await Assert.ThrowsAsync<DbUpdateException>(() => otroContexto.SaveChangesAsync());
    }
}
