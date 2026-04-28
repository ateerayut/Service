namespace Service.Api.Features.Auth;

public class JwtOptions
{
    public const string SectionName = "Jwt";

    public string Issuer { get; init; } = "service-template";
    public string Audience { get; init; } = "service-template-clients";
    public string SigningKey { get; init; } = "CHANGE_ME_TO_A_SECRET_KEY_WITH_AT_LEAST_32_CHARS";
    public int ExpirationMinutes { get; init; } = 60;
    public int RefreshTokenExpirationDays { get; init; } = 7;
}
