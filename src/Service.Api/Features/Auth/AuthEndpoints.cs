using Microsoft.Extensions.Options;

namespace Service.Api.Features.Auth;

public static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapAuthEndpoints(
        this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/auth")
            .WithTags("Auth")
            .RequireRateLimiting("fixed");

        group.MapPost("/token",
            (
                TokenRequest request,
                IOptions<TokenUserOptions> tokenUserOptions,
                ITokenService tokenService,
                IRefreshTokenStore refreshTokenStore) =>
            {
                var tokenUser = tokenUserOptions.Value;

                if (!string.Equals(request.Username, tokenUser.Username, StringComparison.Ordinal) ||
                    !string.Equals(request.Password, tokenUser.Password, StringComparison.Ordinal))
                {
                    return Results.Unauthorized();
                }

                var response = tokenService.CreateTokenPair(request.Username);

                refreshTokenStore.Store(
                    response.RefreshToken,
                    request.Username,
                    response.RefreshTokenExpiresAt);

                return Results.Ok(response);
            })
        .AllowAnonymous();

        group.MapPost("/refresh",
            (
                RefreshTokenRequest request,
                ITokenService tokenService,
                IRefreshTokenStore refreshTokenStore) =>
            {
                if (!refreshTokenStore.TryConsume(request.RefreshToken, out var username))
                    return Results.Unauthorized();

                var response = tokenService.CreateTokenPair(username);

                refreshTokenStore.Store(
                    response.RefreshToken,
                    username,
                    response.RefreshTokenExpiresAt);

                return Results.Ok(response);
            })
        .AllowAnonymous();

        group.MapPost("/revoke",
            (
                RevokeTokenRequest request,
                IRefreshTokenStore refreshTokenStore) =>
            {
                refreshTokenStore.Revoke(request.RefreshToken);

                return Results.NoContent();
            })
        .RequireAuthorization();

        return app;
    }
}
