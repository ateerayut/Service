namespace Service.Api.Features.Auth;

public record RevokeTokenRequest(
    string RefreshToken);
