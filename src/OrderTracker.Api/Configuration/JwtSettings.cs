namespace OrderTracker.Api.Configuration;

public sealed class JwtSettings
{
    public const string SectionName = "JwtSettings";

    public required string Issuer { get; init; }
    public required string Audience { get; init; }
    public required string SecretKey { get; init; }
    public int AccessTokenExpiryMinutes { get; init; } = 15;
    public int RefreshTokenExpiryDays { get; init; } = 7;
}
