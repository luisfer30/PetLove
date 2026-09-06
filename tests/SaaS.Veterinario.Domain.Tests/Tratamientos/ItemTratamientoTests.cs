using SaaS.Veterinario.Domain.Errores;
using SaaS.Veterinario.Domain.Tratamientos;

namespace SaaS.Veterinario.Domain.Tests.Tratamientos;

public class ItemTratamientoTests
{
    private static readonly DateTimeOffset Inicio = new(2026, 1, 10, 8, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Crear_MedicamentoConIntervaloHoras_EsValido()
    {
        var item = ItemTratamiento.Crear(
            Guid.NewGuid(), TipoItemTratamiento.Medicamento, "Amoxicilina", null,
            1m, "tableta", ViaAdministracion.Oral,
            TipoFrecuenciaTratamiento.IntervaloHoras, 8, null, [],
            Inicio, Inicio.AddDays(5), null, "Con comida", 0);

        Assert.Equal(EstadoItemTratamiento.Activo, item.Estado);
        Assert.Equal(8, item.IntervaloHoras);
        Assert.Empty(item.Horarios);
    }

    [Fact]
    public void Crear_CuidadoConHoraFija_EsValido()
    {
        var item = ItemTratamiento.Crear(
            Guid.NewGuid(), TipoItemTratamiento.Cuidado, "Limpieza de herida", null,
            null, null, null,
            TipoFrecuenciaTratamiento.HoraFija, null, null, [new TimeOnly(8, 0), new TimeOnly(20, 0)],
            Inicio, Inicio.AddDays(7), null, null, 0);

        Assert.Equal(2, item.Horarios.Count);
        Assert.Null(item.ViaAdministracion);
    }

    [Fact]
    public void Crear_IntervaloHorasSinIntervalo_LanzaExcepcionDominio()
    {
        Assert.Throws<ExcepcionDominio>(() => ItemTratamiento.Crear(
            Guid.NewGuid(), TipoItemTratamiento.Medicamento, "Medicamento", null,
            null, null, null,
            TipoFrecuenciaTratamiento.IntervaloHoras, null, null, [],
            Inicio, Inicio.AddDays(1), null, null, 0));
    }

    [Fact]
    public void Crear_HoraFijaSinHorarios_LanzaExcepcionDominio()
    {
        Assert.Throws<ExcepcionDominio>(() => ItemTratamiento.Crear(
            Guid.NewGuid(), TipoItemTratamiento.Cuidado, "Cuidado", null,
            null, null, null,
            TipoFrecuenciaTratamiento.HoraFija, null, null, [],
            Inicio, Inicio.AddDays(1), null, null, 0));
    }

    [Fact]
    public void Crear_VecesPorDiaConCantidadDeHorariosDistinta_LanzaExcepcionDominio()
    {
        Assert.Throws<ExcepcionDominio>(() => ItemTratamiento.Crear(
            Guid.NewGuid(), TipoItemTratamiento.Medicamento, "Medicamento", null,
            1m, "ml", ViaAdministracion.Oral,
            TipoFrecuenciaTratamiento.VecesPorDia, null, 3, [new TimeOnly(8, 0)],
            Inicio, Inicio.AddDays(1), null, null, 0));
    }

    [Fact]
    public void Crear_SegunNecesidad_NoRequiereHorariosNiGeneraProgramaciones()
    {
        var item = ItemTratamiento.Crear(
            Guid.NewGuid(), TipoItemTratamiento.Medicamento, "Analgésico de rescate", null,
            null, null, ViaAdministracion.Oral,
            TipoFrecuenciaTratamiento.SegunNecesidad, null, null, [],
            Inicio, null, null, "Solo si hay dolor evidente", 0);

        Assert.True(item.EsSegunNecesidad);
        Assert.Empty(item.Horarios);
    }

    [Fact]
    public void Crear_UnicaConHorariosProvistos_LanzaExcepcionDominio()
    {
        Assert.Throws<ExcepcionDominio>(() => ItemTratamiento.Crear(
            Guid.NewGuid(), TipoItemTratamiento.Medicamento, "Sedante", null,
            null, null, null,
            TipoFrecuenciaTratamiento.Unica, null, null, [new TimeOnly(8, 0)],
            Inicio, null, null, null, 0));
    }

    [Fact]
    public void Suspender_DesdeActivo_CambiaEstado()
    {
        var item = ItemTratamiento.Crear(
            Guid.NewGuid(), TipoItemTratamiento.Reposo, "Reposo absoluto", null,
            null, null, null, TipoFrecuenciaTratamiento.Unica, null, null, [], Inicio, null, null, null, 0);

        item.Suspender(DateTimeOffset.UtcNow);

        Assert.Equal(EstadoItemTratamiento.Suspendido, item.Estado);
    }

    [Fact]
    public void Cancelar_DesdeCancelado_LanzaExcepcionDominio()
    {
        var item = ItemTratamiento.Crear(
            Guid.NewGuid(), TipoItemTratamiento.Reposo, "Reposo absoluto", null,
            null, null, null, TipoFrecuenciaTratamiento.Unica, null, null, [], Inicio, null, null, null, 0);
        item.Cancelar(DateTimeOffset.UtcNow);

        Assert.Throws<ExcepcionDominio>(() => item.Cancelar(DateTimeOffset.UtcNow));
    }
}
