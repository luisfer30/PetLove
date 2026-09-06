using SaaS.Veterinario.Domain.Mascotas;

namespace SaaS.Veterinario.Infrastructure.Persistencia.Seed;

/// <summary>Catalogo global minimo. IDs fijos, mismo motivo que RolesPermisosSeed.</summary>
internal static class EspeciesSeed
{
    public static readonly Guid PerroId = new("33333333-0000-0000-0000-000000000001");
    public static readonly Guid GatoId = new("33333333-0000-0000-0000-000000000002");
    public static readonly Guid AveId = new("33333333-0000-0000-0000-000000000003");
    public static readonly Guid ConejoId = new("33333333-0000-0000-0000-000000000004");
    public static readonly Guid OtroId = new("33333333-0000-0000-0000-000000000005");

    public static object[] Especies { get; } =
    [
        new { Id = PerroId, Codigo = "PERRO", Nombre = "Perro", Estado = EstadoEspecie.Activo },
        new { Id = GatoId, Codigo = "GATO", Nombre = "Gato", Estado = EstadoEspecie.Activo },
        new { Id = AveId, Codigo = "AVE", Nombre = "Ave", Estado = EstadoEspecie.Activo },
        new { Id = ConejoId, Codigo = "CONEJO", Nombre = "Conejo", Estado = EstadoEspecie.Activo },
        new { Id = OtroId, Codigo = "OTRO", Nombre = "Otro", Estado = EstadoEspecie.Activo },
    ];
}
