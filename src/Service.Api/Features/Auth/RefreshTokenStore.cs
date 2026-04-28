using System.Collections.Concurrent;

namespace Service.Api.Features.Auth;

public interface IRefreshTokenStore
{
    void Store(string refreshToken, string username, DateTimeOffset expiresAt);
    bool TryConsume(string refreshToken, out string username);
    bool Revoke(string refreshToken);
}

public class InMemoryRefreshTokenStore : IRefreshTokenStore
{
    private readonly ConcurrentDictionary<string, RefreshTokenEntry> _tokens = new();

    public void Store(string refreshToken, string username, DateTimeOffset expiresAt)
    {
        _tokens[refreshToken] = new RefreshTokenEntry(
            username,
            expiresAt,
            Revoked: false);
    }

    public bool TryConsume(string refreshToken, out string username)
    {
        username = string.Empty;

        if (!_tokens.TryGetValue(refreshToken, out var entry))
            return false;

        if (entry.Revoked || entry.ExpiresAt <= DateTimeOffset.UtcNow)
            return false;

        var revokedEntry = entry with { Revoked = true };

        if (!_tokens.TryUpdate(refreshToken, revokedEntry, entry))
            return false;

        username = entry.Username;
        return true;
    }

    public bool Revoke(string refreshToken)
    {
        if (!_tokens.TryGetValue(refreshToken, out var entry))
            return false;

        var revokedEntry = entry with { Revoked = true };

        return _tokens.TryUpdate(refreshToken, revokedEntry, entry);
    }

    private record RefreshTokenEntry(
        string Username,
        DateTimeOffset ExpiresAt,
        bool Revoked);
}
