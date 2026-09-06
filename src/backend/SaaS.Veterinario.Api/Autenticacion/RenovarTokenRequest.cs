namespace SaaS.Veterinario.Api.Autenticacion;

public sealed record RenovarTokenRequest(string RefreshToken, string? Dispositivo);

public sealed record CerrarSesionRequest(string RefreshToken);

public sealed record ConfirmarCorreoRequest(Guid UsuarioId, string Token);

public sealed record SolicitarRecuperacionRequest(string Correo);

public sealed record RestablecerPasswordRequest(string Correo, string Token, string NuevoPassword);

public sealed record MensajeResponse(string Mensaje);
