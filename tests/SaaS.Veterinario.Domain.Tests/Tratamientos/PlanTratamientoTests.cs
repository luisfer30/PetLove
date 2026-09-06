using SaaS.Veterinario.Domain.Errores;
using SaaS.Veterinario.Domain.Tratamientos;

namespace SaaS.Veterinario.Domain.Tests.Tratamientos;

public class PlanTratamientoTests
{
    private static readonly DateTimeOffset Inicio = new(2026, 1, 10, 9, 0, 0, TimeSpan.Zero);

    private static PlanTratamiento CrearPlanValido()
        => PlanTratamiento.Crear(Guid.NewGuid(), Guid.NewGuid(), null, "Tratamiento post-cirugía", null, Inicio, null, Guid.NewGuid());

    [Fact]
    public void Crear_ConDatosValidos_InicializaEnPendiente()
    {
        var plan = CrearPlanValido();

        Assert.Equal(EstadoPlanTratamiento.Pendiente, plan.Estado);
    }

    [Fact]
    public void Crear_SinNombre_LanzaExcepcionDominio()
    {
        Assert.Throws<ExcepcionDominio>(() => PlanTratamiento.Crear(Guid.NewGuid(), Guid.NewGuid(), null, "   ", null, Inicio, null, Guid.NewGuid()));
    }

    [Fact]
    public void Crear_ConFinEstimadaAnteriorAInicio_LanzaExcepcionDominio()
    {
        Assert.Throws<ExcepcionDominio>(() => PlanTratamiento.Crear(
            Guid.NewGuid(), Guid.NewGuid(), null, "Motivo", null, Inicio, Inicio.AddDays(-1), Guid.NewGuid()));
    }

    [Fact]
    public void Activar_DesdePendiente_CambiaAActivo()
    {
        var plan = CrearPlanValido();

        plan.Activar(Guid.NewGuid(), DateTimeOffset.UtcNow);

        Assert.Equal(EstadoPlanTratamiento.Activo, plan.Estado);
    }

    [Fact]
    public void Suspender_DesdeActivo_RegistraMotivoYUsuario()
    {
        var plan = CrearPlanValido();
        plan.Activar(Guid.NewGuid(), DateTimeOffset.UtcNow);
        var usuarioId = Guid.NewGuid();

        plan.Suspender(usuarioId, "Efectos adversos", DateTimeOffset.UtcNow);

        Assert.Equal(EstadoPlanTratamiento.Suspendido, plan.Estado);
        Assert.Equal("Efectos adversos", plan.MotivoSuspension);
        Assert.Equal(usuarioId, plan.SuspendidoPorUsuarioId);
    }

    [Fact]
    public void Suspender_SinMotivo_LanzaExcepcionDominio()
    {
        var plan = CrearPlanValido();
        plan.Activar(Guid.NewGuid(), DateTimeOffset.UtcNow);

        Assert.Throws<ExcepcionDominio>(() => plan.Suspender(Guid.NewGuid(), "   ", DateTimeOffset.UtcNow));
    }

    [Fact]
    public void Reanudar_DesdeSuspendido_CambiaAActivo()
    {
        var plan = CrearPlanValido();
        plan.Activar(Guid.NewGuid(), DateTimeOffset.UtcNow);
        plan.Suspender(Guid.NewGuid(), "Motivo", DateTimeOffset.UtcNow);

        plan.Reanudar(Guid.NewGuid(), DateTimeOffset.UtcNow);

        Assert.Equal(EstadoPlanTratamiento.Activo, plan.Estado);
    }

    [Fact]
    public void Completar_DesdeActivo_RegistraFechaYUsuario()
    {
        var plan = CrearPlanValido();
        plan.Activar(Guid.NewGuid(), DateTimeOffset.UtcNow);
        var usuarioId = Guid.NewGuid();
        var momento = DateTimeOffset.UtcNow;

        plan.Completar(usuarioId, momento);

        Assert.Equal(EstadoPlanTratamiento.Completado, plan.Estado);
        Assert.Equal(momento, plan.FechaCompletado);
        Assert.Equal(usuarioId, plan.CompletadoPorUsuarioId);
    }

    [Fact]
    public void Completar_DesdeSuspendido_EsPermitido()
    {
        var plan = CrearPlanValido();
        plan.Activar(Guid.NewGuid(), DateTimeOffset.UtcNow);
        plan.Suspender(Guid.NewGuid(), "Motivo", DateTimeOffset.UtcNow);

        plan.Completar(Guid.NewGuid(), DateTimeOffset.UtcNow);

        Assert.Equal(EstadoPlanTratamiento.Completado, plan.Estado);
    }

    [Fact]
    public void Cancelar_DesdeActivo_RegistraMotivo()
    {
        var plan = CrearPlanValido();
        plan.Activar(Guid.NewGuid(), DateTimeOffset.UtcNow);

        plan.Cancelar(Guid.NewGuid(), "Mascota falleció", DateTimeOffset.UtcNow);

        Assert.Equal(EstadoPlanTratamiento.Cancelado, plan.Estado);
        Assert.Equal("Mascota falleció", plan.MotivoCancelacion);
    }

    [Fact]
    public void Activar_DesdeCompletado_LanzaExcepcionDominio()
    {
        var plan = CrearPlanValido();
        plan.Activar(Guid.NewGuid(), DateTimeOffset.UtcNow);
        plan.Completar(Guid.NewGuid(), DateTimeOffset.UtcNow);

        Assert.Throws<ExcepcionDominio>(() => plan.Activar(Guid.NewGuid(), DateTimeOffset.UtcNow));
    }

    [Fact]
    public void Activar_DesdeCancelado_LanzaExcepcionDominio()
    {
        var plan = CrearPlanValido();
        plan.Cancelar(Guid.NewGuid(), "Motivo", DateTimeOffset.UtcNow);

        Assert.Throws<ExcepcionDominio>(() => plan.Activar(Guid.NewGuid(), DateTimeOffset.UtcNow));
    }
}
