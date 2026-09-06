using SaaS.Veterinario.Application.Abstracciones;
using SaaS.Veterinario.Application.Errores;

namespace SaaS.Veterinario.Application.Clinico;

/// <summary>
/// Crear/activar/modificar un PlanTratamiento clinico debe hacerlo un usuario con el permiso
/// dedicado "clinica.ejercer_veterinario" (seccion 46), ademas del permiso de accion propio del
/// endpoint (tratamientos.crear/activar/etc) que ya exige RequierePermiso. A diferencia de
/// ValidadorVeterinarioClinico (Etapa 7), aqui se valida al USUARIO AUTENTICADO actual via
/// IContextoVeterinaria -- no a un tercero asignado por Id -- porque PlanTratamiento no tiene
/// un campo VeterinarioUsuarioId propio.
/// </summary>
internal static class ValidadorPermisoClinico
{
    private const string PermisoEjercerVeterinario = "clinica.ejercer_veterinario";

    public static void ValidarUsuarioActual(IContextoVeterinaria contexto)
    {
        if (!contexto.TienePermiso(PermisoEjercerVeterinario))
        {
            throw new ExcepcionAplicacion(
                CodigosError.AutorizacionPermisoRequerido, "Se requiere el permiso de ejercicio clínico para esta acción.");
        }
    }
}
