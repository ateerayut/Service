namespace Service.Api.Features.Auth;

public record TokenResponse(
    string AccessToken,
    string RefreshToken,
    string TokenType,
    DateTimeOffset ExpiresAt,
    DateTimeOffset RefreshTokenExpiresAt);
