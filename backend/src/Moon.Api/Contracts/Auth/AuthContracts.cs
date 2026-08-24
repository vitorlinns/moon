namespace Moon.Api.Contracts.Auth;

public record RegisterRequest(string Cpf, string Name, string Email, string Password);

public record LoginRequest(string Email, string Password);

public record UpdateProfileRequest(string Name, string Email);

public record UserResponse(Guid Id, string Name, string Email, string Cpf);

public record ErrorResponse(string Message);
