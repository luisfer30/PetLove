namespace SaaS.Veterinario.Api.Autenticacion;

public sealed record RegistrarUsuarioRequest(string Nombre, string Correo, string? Telefono, string Password);

public sealed record RegistrarUsuarioResponse(Guid UsuarioId, string Correo);
