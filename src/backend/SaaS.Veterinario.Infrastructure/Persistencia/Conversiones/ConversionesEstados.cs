using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using SaaS.Veterinario.Domain.Membresias;
using SaaS.Veterinario.Domain.Personal;
using SaaS.Veterinario.Domain.Roles;
using SaaS.Veterinario.Domain.Usuarios;
using SaaS.Veterinario.Domain.Veterinarias;

namespace SaaS.Veterinario.Infrastructure.Persistencia.Conversiones;

/// <summary>
/// Convierte cada enum de estado a la cadena en mayusculas acordada (p. ej. "PENDIENTE_VERIFICACION"),
/// en vez de usar el nombre del miembro C# (PascalCase) que produciria HasConversion&lt;string&gt;() por
/// defecto. Los mapeos se resuelven con diccionarios (en vez de switch/throw) porque los lambdas de
/// ValueConverter son arboles de expresion, y un arbol de expresion no admite expresiones switch ni throw.
/// Una clave ausente en el diccionario ya lanza KeyNotFoundException por si sola.
/// </summary>
internal static class ConversionesEstados
{
    private static readonly Dictionary<EstadoUsuario, string> TextoPorEstadoUsuario = new()
    {
        [EstadoUsuario.PendienteVerificacion] = "PENDIENTE_VERIFICACION",
        [EstadoUsuario.Activo] = "ACTIVO",
        [EstadoUsuario.Bloqueado] = "BLOQUEADO",
        [EstadoUsuario.Suspendido] = "SUSPENDIDO",
    };

    private static readonly Dictionary<string, EstadoUsuario> EstadoUsuarioPorTexto =
        TextoPorEstadoUsuario.ToDictionary(par => par.Value, par => par.Key);

    private static readonly Dictionary<EstadoVeterinaria, string> TextoPorEstadoVeterinaria = new()
    {
        [EstadoVeterinaria.PendienteActivacion] = "PENDIENTE_ACTIVACION",
        [EstadoVeterinaria.Activa] = "ACTIVA",
        [EstadoVeterinaria.Suspendida] = "SUSPENDIDA",
        [EstadoVeterinaria.Inactiva] = "INACTIVA",
    };

    private static readonly Dictionary<string, EstadoVeterinaria> EstadoVeterinariaPorTexto =
        TextoPorEstadoVeterinaria.ToDictionary(par => par.Value, par => par.Key);

    private static readonly Dictionary<EstadoMembresiaVeterinaria, string> TextoPorEstadoMembresia = new()
    {
        [EstadoMembresiaVeterinaria.Invitada] = "INVITADA",
        [EstadoMembresiaVeterinaria.Activa] = "ACTIVA",
        [EstadoMembresiaVeterinaria.Suspendida] = "SUSPENDIDA",
        [EstadoMembresiaVeterinaria.Finalizada] = "FINALIZADA",
    };

    private static readonly Dictionary<string, EstadoMembresiaVeterinaria> EstadoMembresiaPorTexto =
        TextoPorEstadoMembresia.ToDictionary(par => par.Value, par => par.Key);

    private static readonly Dictionary<EstadoRol, string> TextoPorEstadoRol = new()
    {
        [EstadoRol.Activo] = "ACTIVO",
        [EstadoRol.Inactivo] = "INACTIVO",
    };

    private static readonly Dictionary<string, EstadoRol> EstadoRolPorTexto =
        TextoPorEstadoRol.ToDictionary(par => par.Value, par => par.Key);

    private static readonly Dictionary<EstadoInvitacionPersonal, string> TextoPorEstadoInvitacionPersonal = new()
    {
        [EstadoInvitacionPersonal.Pendiente] = "PENDIENTE",
        [EstadoInvitacionPersonal.Aceptada] = "ACEPTADA",
        [EstadoInvitacionPersonal.Expirada] = "EXPIRADA",
        [EstadoInvitacionPersonal.Cancelada] = "CANCELADA",
    };

    private static readonly Dictionary<string, EstadoInvitacionPersonal> EstadoInvitacionPersonalPorTexto =
        TextoPorEstadoInvitacionPersonal.ToDictionary(par => par.Value, par => par.Key);

    public static ValueConverter<EstadoUsuario, string> ConversorEstadoUsuario { get; } = new(
        estado => TextoPorEstadoUsuario[estado],
        valor => EstadoUsuarioPorTexto[valor]);

    public static ValueConverter<EstadoVeterinaria, string> ConversorEstadoVeterinaria { get; } = new(
        estado => TextoPorEstadoVeterinaria[estado],
        valor => EstadoVeterinariaPorTexto[valor]);

    public static ValueConverter<EstadoMembresiaVeterinaria, string> ConversorEstadoMembresia { get; } = new(
        estado => TextoPorEstadoMembresia[estado],
        valor => EstadoMembresiaPorTexto[valor]);

    public static ValueConverter<EstadoRol, string> ConversorEstadoRol { get; } = new(
        estado => TextoPorEstadoRol[estado],
        valor => EstadoRolPorTexto[valor]);

    public static ValueConverter<EstadoInvitacionPersonal, string> ConversorEstadoInvitacionPersonal { get; } = new(
        estado => TextoPorEstadoInvitacionPersonal[estado],
        valor => EstadoInvitacionPersonalPorTexto[valor]);
}
