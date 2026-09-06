using SaaS.Veterinario.Domain.Consultas;
using SaaS.Veterinario.Domain.Errores;

namespace SaaS.Veterinario.Domain.Tests.Consultas;

public class DiagnosticoConsultaTests
{
    [Fact]
    public void Crear_ConDatosValidos_AsignaDescripcionYTipo()
    {
        var diagnostico = DiagnosticoConsulta.Crear(Guid.NewGuid(), "Otitis externa", TipoDiagnostico.Presuntivo, esPrincipal: true);

        Assert.Equal("Otitis externa", diagnostico.Descripcion);
        Assert.Equal(TipoDiagnostico.Presuntivo, diagnostico.Tipo);
        Assert.True(diagnostico.EsPrincipal);
    }

    [Fact]
    public void Crear_SinDescripcion_LanzaExcepcionDominio()
    {
        Assert.Throws<ExcepcionDominio>(() => DiagnosticoConsulta.Crear(Guid.NewGuid(), "   ", TipoDiagnostico.Otro, false));
    }

    [Fact]
    public void Crear_SinConsultaId_LanzaExcepcionDominio()
    {
        Assert.Throws<ExcepcionDominio>(() => DiagnosticoConsulta.Crear(Guid.Empty, "Descripción", TipoDiagnostico.Otro, false));
    }

    [Fact]
    public void Actualizar_ConDatosValidos_ModificaCampos()
    {
        var diagnostico = DiagnosticoConsulta.Crear(Guid.NewGuid(), "Otitis externa", TipoDiagnostico.Presuntivo, false);

        diagnostico.Actualizar("Otitis confirmada por cultivo", TipoDiagnostico.Confirmado, true);

        Assert.Equal("Otitis confirmada por cultivo", diagnostico.Descripcion);
        Assert.Equal(TipoDiagnostico.Confirmado, diagnostico.Tipo);
        Assert.True(diagnostico.EsPrincipal);
    }

    [Fact]
    public void Actualizar_SinDescripcion_LanzaExcepcionDominio()
    {
        var diagnostico = DiagnosticoConsulta.Crear(Guid.NewGuid(), "Otitis externa", TipoDiagnostico.Presuntivo, false);

        Assert.Throws<ExcepcionDominio>(() => diagnostico.Actualizar("   ", TipoDiagnostico.Otro, false));
    }
}
