namespace Moon.Api.Contracts.AdminAuth;

public record AdminLoginRequest(string Email, string Password);

public record AdminUserResponse(Guid Id, string Name, string Email);
