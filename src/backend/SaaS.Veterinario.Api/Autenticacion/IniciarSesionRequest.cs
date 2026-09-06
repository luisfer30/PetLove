namespace SaaS.Veterinario.Api.Autenticacion;

public sealed record IniciarSesionRequest(string Correo, string Password, string? Dispositivo);
