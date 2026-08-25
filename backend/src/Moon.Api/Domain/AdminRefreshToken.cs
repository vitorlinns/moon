namespace Moon.Api.Domain;

public class AdminRefreshToken
{
    public Guid Id { get; set; }

    public Guid AdminUserId { get; set; }

    public required string TokenHash { get; set; }

    public DateTimeOffset ExpiresAt { get; set; }

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset? RevokedAt { get; set; }

    /// <summary>
    /// Token que substituiu este na rotação. Se este token voltar a ser usado depois
    /// de já ter um substituto, é sinal de que foi roubado (reuse detection).
    /// </summary>
    public Guid? ReplacedByTokenId { get; set; }

    public string? CreatedByIp { get; set; }
}
