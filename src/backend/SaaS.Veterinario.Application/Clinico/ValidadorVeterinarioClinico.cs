using SaaS.Veterinario.Application.Abstracciones;
using SaaS.Veterinario.Application.Errores;

namespace SaaS.Veterinario.Application.Clinico;

/// <summary>
/// Valida que un UsuarioId propuesto como veterinario (de una Cita o una Consulta) tenga
/// MembresiaVeterinaria ACTIVA en la MISMA veterinaria del contexto y el permiso dedicado
/// "clinica.ejercer_veterinario" -- no basta con que el Usuario exista globalmente (seccion
/// 5/15), ni con que tenga cualquier otro permiso clinico: este permiso significa
/// exclusivamente "puede ser seleccionado como profesional veterinario responsable de una
/// cita/consulta" (ajuste posterior a la Etapa 7), asignado solo al rol VETERINARIO por
/// defecto. Compartido entre Citas y Consultas porque la regla es identica en ambos casos.
/// </summary>
internal static class ValidadorVeterinarioClinico
{
    private const string PermisoEjercerVeterinario = "clinica.ejercer_veterinario";

    public static async Task ValidarAsync(
        IRepositorioMembresias repositorioMembresias,
        Guid veterinarioUsuarioId,
        Guid veterinariaId,
        string codigoError,
        CancellationToken cancellationToken)
    {
        var contextoVeterinario = await repositorioMembresias.ObtenerContextoActivoAsync(veterinarioUsuarioId, cancellationToken);

        if (contextoVeterinario is null
            || contextoVeterinario.VeterinariaId != veterinariaId
            || !contextoVeterinario.Permisos.Contains(PermisoEjercerVeterinario))
        {
            throw new ExcepcionAplicacion(codigoError, "El veterinario indicado no es válido para esta veterinaria.");
        }
    }
}
