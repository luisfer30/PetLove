using Moq;
using SaaS.Veterinario.Application.Abstracciones;
using SaaS.Veterinario.Application.ContextoVeterinario;
using SaaS.Veterinario.Domain.Veterinarias;

namespace SaaS.Veterinario.Application.Tests.ContextoVeterinario;

public class ResolverContextoVeterinariaTests
{
    private readonly Mock<IRepositorioMembresias> _repositorioMembresias = new();
    private readonly Mock<IRepositorioVeterinarias> _repositorioVeterinarias = new();

    private ResolverContextoVeterinaria CrearCasoDeUso()
        => new(_repositorioMembresias.Object, _repositorioVeterinarias.Object);

    private static Veterinaria CrearVeterinariaActiva()
    {
        var veterinaria = Veterinaria.Crear("VET-000001", "Clinica Test", "contacto@clinica.com", "Ecuador", "America/Guayaquil");
        veterinaria.Activar(DateTimeOffset.UtcNow);
        return veterinaria;
    }

    private static Veterinaria CrearVeterinariaSuspendida()
    {
        var veterinaria = CrearVeterinariaActiva();
        veterinaria.Suspender(DateTimeOffset.UtcNow);
        return veterinaria;
    }

    [Fact]
    public async Task EjecutarAsync_SinMembresiaActiva_RetornaSinMembresia()
    {
        var usuarioId = Guid.NewGuid();
        _repositorioMembresias
            .Setup(r => r.ObtenerContextoActivoAsync(usuarioId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((InfoContextoMembresia?)null);

        var resultado = await CrearCasoDeUso().EjecutarAsync(usuarioId, CancellationToken.None);

        Assert.False(resultado.TieneMembresiaActiva);
        Assert.False(resultado.TieneContextoOperativo);
        Assert.Null(resultado.VeterinariaId);
        Assert.Empty(resultado.Roles);
        Assert.Empty(resultado.Permisos);
        _repositorioVeterinarias.Verify(r => r.ObtenerPorIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task EjecutarAsync_ConMembresiaActivaYVeterinariaActiva_RetornaContextoDisponible()
    {
        var usuarioId = Guid.NewGuid();
        var veterinaria = CrearVeterinariaActiva();
        var info = new InfoContextoMembresia(Guid.NewGuid(), veterinaria.Id, ["VETERINARIO"], ["veterinaria.ver"]);

        _repositorioMembresias.Setup(r => r.ObtenerContextoActivoAsync(usuarioId, It.IsAny<CancellationToken>())).ReturnsAsync(info);
        _repositorioVeterinarias.Setup(r => r.ObtenerPorIdAsync(veterinaria.Id, It.IsAny<CancellationToken>())).ReturnsAsync(veterinaria);

        var resultado = await CrearCasoDeUso().EjecutarAsync(usuarioId, CancellationToken.None);

        Assert.True(resultado.TieneMembresiaActiva);
        Assert.True(resultado.TieneContextoOperativo);
        Assert.Equal(veterinaria.Id, resultado.VeterinariaId);
        Assert.Equal(info.MembresiaId, resultado.MembresiaId);
        Assert.Equal(["VETERINARIO"], resultado.Roles);
        Assert.Equal(["veterinaria.ver"], resultado.Permisos);
    }

    [Fact]
    public async Task EjecutarAsync_ConVeterinariaSuspendida_RetornaVeterinariaNoDisponibleSinPermisos()
    {
        var usuarioId = Guid.NewGuid();
        var veterinaria = CrearVeterinariaSuspendida();
        var info = new InfoContextoMembresia(Guid.NewGuid(), veterinaria.Id, ["VETERINARIO"], ["veterinaria.ver"]);

        _repositorioMembresias.Setup(r => r.ObtenerContextoActivoAsync(usuarioId, It.IsAny<CancellationToken>())).ReturnsAsync(info);
        _repositorioVeterinarias.Setup(r => r.ObtenerPorIdAsync(veterinaria.Id, It.IsAny<CancellationToken>())).ReturnsAsync(veterinaria);

        var resultado = await CrearCasoDeUso().EjecutarAsync(usuarioId, CancellationToken.None);

        Assert.True(resultado.TieneMembresiaActiva);
        Assert.False(resultado.TieneContextoOperativo);
        Assert.Equal(veterinaria.Id, resultado.VeterinariaId);
        Assert.Empty(resultado.Roles);
        Assert.Empty(resultado.Permisos);
    }

    [Fact]
    public async Task EjecutarAsync_ConVeterinariaInexistente_RetornaVeterinariaNoDisponible()
    {
        var usuarioId = Guid.NewGuid();
        var veterinariaId = Guid.NewGuid();
        var info = new InfoContextoMembresia(Guid.NewGuid(), veterinariaId, ["VETERINARIO"], ["veterinaria.ver"]);

        _repositorioMembresias.Setup(r => r.ObtenerContextoActivoAsync(usuarioId, It.IsAny<CancellationToken>())).ReturnsAsync(info);
        _repositorioVeterinarias.Setup(r => r.ObtenerPorIdAsync(veterinariaId, It.IsAny<CancellationToken>())).ReturnsAsync((Veterinaria?)null);

        var resultado = await CrearCasoDeUso().EjecutarAsync(usuarioId, CancellationToken.None);

        Assert.True(resultado.TieneMembresiaActiva);
        Assert.False(resultado.TieneContextoOperativo);
    }
}
