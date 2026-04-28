using Microsoft.Extensions.Options;
using Service.Api.Features.Auth;

namespace Service.IntegrationTests;

public class AuthTokenTests
{
    [Fact]
    public void CreateTokenPair_ReturnsAccessAndRefreshTokens()
    {
        var service = new TokenService(Options.Create(new JwtOptions
        {
            SigningKey = "test-secret-key-with-at-least-32-characters"
        }));

        var response = service.CreateTokenPair("demo");

        Assert.False(string.IsNullOrWhiteSpace(response.AccessToken));
        Assert.False(string.IsNullOrWhiteSpace(response.RefreshToken));
        Assert.Equal("Bearer", response.TokenType);
        Assert.True(response.ExpiresAt > DateTimeOffset.UtcNow);
        Assert.True(response.RefreshTokenExpiresAt > response.ExpiresAt);
    }

    [Fact]
    public void RefreshTokenStore_TryConsume_AllowsOneUseOnly()
    {
        var store = new InMemoryRefreshTokenStore();
        store.Store("refresh-token", "demo", DateTimeOffset.UtcNow.AddDays(1));

        var firstUse = store.TryConsume("refresh-token", out var username);
        var secondUse = store.TryConsume("refresh-token", out _);

        Assert.True(firstUse);
        Assert.Equal("demo", username);
        Assert.False(secondUse);
    }

    [Fact]
    public void RefreshTokenStore_Revoke_PreventsTokenUse()
    {
        var store = new InMemoryRefreshTokenStore();
        store.Store("refresh-token", "demo", DateTimeOffset.UtcNow.AddDays(1));

        var revoked = store.Revoke("refresh-token");
        var used = store.TryConsume("refresh-token", out _);

        Assert.True(revoked);
        Assert.False(used);
    }
}
