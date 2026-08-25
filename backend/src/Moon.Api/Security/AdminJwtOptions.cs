namespace Moon.Api.Security;

public class AdminJwtOptions
{
    public const string SectionName = "AdminJwt";

    public required string Key { get; set; }

    public required string Issuer { get; set; }

    public required string Audience { get; set; }

    public int AccessTokenExpiresMinutes { get; set; } = 15;

    public int RefreshTokenExpiresDays { get; set; } = 30;
}
