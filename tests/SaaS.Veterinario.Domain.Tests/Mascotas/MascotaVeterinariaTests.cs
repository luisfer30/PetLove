using SaaS.Veterinario.Domain.Errores;
using SaaS.Veterinario.Domain.Mascotas;

namespace SaaS.Veterinario.Domain.Tests.Mascotas;

public class MascotaVeterinariaTests
{
    [Fact]
    public void Crear_ConDatosValidos_InicializaEnActiva()
    {
        var mascotaVeterinaria = MascotaVeterinaria.Crear(Guid.NewGuid(), Guid.NewGuid(), "HC-001");

        Assert.Equal(EstadoMascotaVeterinaria.Activa, mascotaVeterinaria.Estado);
        Assert.Equal("HC-001", mascotaVeterinaria.NumeroHistoria);
        Assert.Null(mascotaVeterinaria.FechaEliminacion);
    }

    [Fact]
    public void Crear_ConVeterinariaIdVacio_LanzaExcepcionDominio()
    {
        Assert.Throws<ExcepcionDominio>(() => MascotaVeterinaria.Crear(Guid.Empty, Guid.NewGuid()));
    }

    [Fact]
    public void Desactivar_DesdeActiva_CambiaEstadoAInactiva()
    {
        var mascotaVeterinaria = MascotaVeterinaria.Crear(Guid.NewGuid(), Guid.NewGuid());

        mascotaVeterinaria.Desactivar(DateTimeOffset.UtcNow);

        Assert.Equal(EstadoMascotaVeterinaria.Inactiva, mascotaVeterinaria.Estado);
    }
}
