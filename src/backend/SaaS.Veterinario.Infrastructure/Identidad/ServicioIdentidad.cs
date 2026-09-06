using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SaaS.Veterinario.Application.Abstracciones;

namespace SaaS.Veterinario.Infrastructure.Identidad;

/// <summary>
/// Usa unicamente UserManager (no SignInManager): SignInManager vive en el ensamblado del
/// framework ASP.NET Core (requiere FrameworkReference a Microsoft.AspNetCore.App) y esta
/// pensado para flujos de sign-in por cookie, que esta plataforma no usa. La validacion de
/// contraseña + lockout que necesitamos la expone directamente UserManager
/// (CheckPasswordAsync / IsLockedOutAsync / AccessFailedAsync / ResetAccessFailedCountAsync),
/// que es lo mismo que SignInManager.CheckPasswordSignInAsync hace por dentro.
/// </summary>
public sealed class ServicioIdentidad(UserManager<UsuarioIdentidad> userManager) : IServicioIdentidad
{
    public async Task<bool> ExisteCorreoAsync(string correo, CancellationToken cancellationToken)
    {
        var normalizado = userManager.NormalizeEmail(correo);
        return await userManager.Users.AnyAsync(u => u.NormalizedEmail == normalizado, cancellationToken);
    }

    public async Task<ResultadoIdentidad> CrearAsync(Guid usuarioId, string correo, string password, CancellationToken cancellationToken)
    {
        var identidad = new UsuarioIdentidad
        {
            UsuarioId = usuarioId,
            UserName = correo,
            Email = correo,
        };

        var resultado = await userManager.CreateAsync(identidad, password);

        return resultado.Succeeded
            ? ResultadoIdentidad.Ok()
            : ResultadoIdentidad.Fallo(resultado.Errors.Select(e => e.Description));
    }

    public async Task<ResultadoInicioSesion> ValidarCredencialesAsync(string correo, string password, CancellationToken cancellationToken)
    {
        var identidad = await BuscarPorCorreoAsync(correo, cancellationToken);

        if (identidad is null)
        {
            return new ResultadoInicioSesion(EstadoInicioSesion.CredencialesInvalidas, null);
        }

        // Replica lo que SignInManager.CheckPasswordSignInAsync hace por dentro: revisa
        // lockout, valida password, y actualiza el contador de intentos fallidos -- todo
        // delegado en Identity, sin logica manual duplicada.
        if (userManager.SupportsUserLockout && await userManager.IsLockedOutAsync(identidad))
        {
            return new ResultadoInicioSesion(EstadoInicioSesion.Bloqueado, identidad.UsuarioId);
        }

        if (await userManager.CheckPasswordAsync(identidad, password))
        {
            if (userManager.SupportsUserLockout)
            {
                await userManager.ResetAccessFailedCountAsync(identidad);
            }

            return new ResultadoInicioSesion(EstadoInicioSesion.Exitoso, identidad.UsuarioId);
        }

        if (userManager.SupportsUserLockout)
        {
            await userManager.AccessFailedAsync(identidad);

            if (await userManager.IsLockedOutAsync(identidad))
            {
                return new ResultadoInicioSesion(EstadoInicioSesion.Bloqueado, identidad.UsuarioId);
            }
        }

        return new ResultadoInicioSesion(EstadoInicioSesion.CredencialesInvalidas, null);
    }

    public async Task<string> GenerarTokenConfirmacionCorreoAsync(Guid usuarioId, CancellationToken cancellationToken)
    {
        var identidad = await BuscarPorUsuarioIdAsync(usuarioId, cancellationToken)
            ?? throw new InvalidOperationException("No existe identidad para el usuario indicado.");

        return await userManager.GenerateEmailConfirmationTokenAsync(identidad);
    }

    public async Task<ResultadoIdentidad> ConfirmarCorreoAsync(Guid usuarioId, string token, CancellationToken cancellationToken)
    {
        var identidad = await BuscarPorUsuarioIdAsync(usuarioId, cancellationToken);

        if (identidad is null)
        {
            return ResultadoIdentidad.Fallo(["No existe identidad para el usuario indicado."]);
        }

        var resultado = await userManager.ConfirmEmailAsync(identidad, token);

        return resultado.Succeeded
            ? ResultadoIdentidad.Ok()
            : ResultadoIdentidad.Fallo(resultado.Errors.Select(e => e.Description));
    }

    public async Task<Guid?> ObtenerUsuarioIdPorCorreoAsync(string correo, CancellationToken cancellationToken)
    {
        var identidad = await BuscarPorCorreoAsync(correo, cancellationToken);
        return identidad?.UsuarioId;
    }

    public async Task<string> GenerarTokenRecuperacionPasswordAsync(Guid usuarioId, CancellationToken cancellationToken)
    {
        var identidad = await BuscarPorUsuarioIdAsync(usuarioId, cancellationToken)
            ?? throw new InvalidOperationException("No existe identidad para el usuario indicado.");

        return await userManager.GeneratePasswordResetTokenAsync(identidad);
    }

    public async Task<ResultadoIdentidad> RestablecerPasswordAsync(Guid usuarioId, string token, string nuevoPassword, CancellationToken cancellationToken)
    {
        var identidad = await BuscarPorUsuarioIdAsync(usuarioId, cancellationToken);

        if (identidad is null)
        {
            return ResultadoIdentidad.Fallo(["No existe identidad para el usuario indicado."]);
        }

        var resultado = await userManager.ResetPasswordAsync(identidad, token, nuevoPassword);

        return resultado.Succeeded
            ? ResultadoIdentidad.Ok()
            : ResultadoIdentidad.Fallo(resultado.Errors.Select(e => e.Description));
    }

    private Task<UsuarioIdentidad?> BuscarPorCorreoAsync(string correo, CancellationToken cancellationToken)
    {
        var normalizado = userManager.NormalizeEmail(correo);
        return userManager.Users.FirstOrDefaultAsync(u => u.NormalizedEmail == normalizado, cancellationToken);
    }

    private Task<UsuarioIdentidad?> BuscarPorUsuarioIdAsync(Guid usuarioId, CancellationToken cancellationToken)
        => userManager.Users.FirstOrDefaultAsync(u => u.UsuarioId == usuarioId, cancellationToken);
}
