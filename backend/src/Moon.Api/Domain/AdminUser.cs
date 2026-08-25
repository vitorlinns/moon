namespace Moon.Api.Domain;

public class AdminUser
{
    public Guid Id { get; set; }

    public required string Name { get; set; }

    public required string Email { get; set; }

    public required string PasswordHash { get; set; }

    public int FailedLoginAttempts { get; set; }

    public DateTimeOffset? LockedUntil { get; set; }

    // desativar sem apagar quando alguém sai da equipe — mantém o histórico de quem fez o quê
    public bool IsActive { get; set; } = true;

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
