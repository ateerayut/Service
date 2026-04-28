using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Service.Api.Features.Auth;

public interface ITokenService
{
    TokenResponse CreateTokenPair(string username);
}

public class TokenService : ITokenService
{
    private readonly JwtOptions _jwt;

    public TokenService(IOptions<JwtOptions> jwtOptions)
    {
        _jwt = jwtOptions.Value;
    }

    public TokenResponse CreateTokenPair(string username)
    {
        var accessTokenExpiresAt = DateTimeOffset.UtcNow.AddMinutes(_jwt.ExpirationMinutes);
        var refreshTokenExpiresAt = DateTimeOffset.UtcNow.AddDays(_jwt.RefreshTokenExpirationDays);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, username),
            new Claim(JwtRegisteredClaimNames.UniqueName, username),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.CreateVersion7().ToString())
        };

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_jwt.SigningKey));

        var credentials = new SigningCredentials(
            key,
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _jwt.Issuer,
            audience: _jwt.Audience,
            claims: claims,
            expires: accessTokenExpiresAt.UtcDateTime,
            signingCredentials: credentials);

        var accessToken = new JwtSecurityTokenHandler()
            .WriteToken(token);

        var refreshToken = WebEncoders.Base64UrlEncode(
            RandomNumberGenerator.GetBytes(64));

        return new TokenResponse(
            accessToken,
            refreshToken,
            "Bearer",
            accessTokenExpiresAt,
            refreshTokenExpiresAt);
    }
}
