using IdentityService.Api.Data;
using IdentityService.Api.Dto;
using IdentityService.Api.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;

namespace IdentityService.Api.Services;

public class RefreshTokenService(DataContext context, RefreshTokenHasher hasher, IOptions<JwtOptions> options)
{
    private readonly JwtOptions _options = options.Value;

    public async Task<string> CreateAsync(string userId, string? ipAddress, CancellationToken ct = default)
    {
        var plainToken = GenerateToken();
        var now = DateTime.UtcNow;

        var refreshToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            TokenHash = hasher.Hash(plainToken),
            CreatedAtUtc = now,
            ExpiresAtUtc = now.AddDays(_options.RefreshTokenDays),
            CreatedByIp = ipAddress
        };

        context.RefreshTokens.Add(refreshToken);
        await context.SaveChangesAsync(ct);
        return plainToken;
    }

    public async Task<RotateRefreshTokenResult> RotateAsync(string plainToken, string? ipAddress, CancellationToken ct = default)
    {
        var tokenHash = hasher.Hash(plainToken);

        var currentToken = await context.RefreshTokens.SingleOrDefaultAsync(x => x.TokenHash == tokenHash, ct);

        if (currentToken is null || !currentToken.IsActive)
            return RotateRefreshTokenResult.Failed();

        var newPlainToken = GenerateToken();

        var now = DateTime.UtcNow;
        var newRefreshToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = currentToken.UserId,
            TokenHash = hasher.Hash(newPlainToken),
            CreatedAtUtc = now,
            ExpiresAtUtc = now.AddDays(_options.RefreshTokenDays),
            CreatedByIp = ipAddress
        };

        currentToken.RevokedAtUtc = now;
        currentToken.RevokedByIp = ipAddress;
        currentToken.ReplacedByTokenId = newRefreshToken.Id;

        context.RefreshTokens.Add(newRefreshToken);

        await context.SaveChangesAsync(ct);

        return RotateRefreshTokenResult.Success(currentToken.UserId, newPlainToken);
    }

    public async Task RevokeAsync(string plainToken, string? ipAddress, CancellationToken ct = default)
    {
        var tokenHash = hasher.Hash(plainToken);
        var token = await context.RefreshTokens.SingleOrDefaultAsync(x => x.TokenHash == tokenHash, ct);

        if (token is null || !token.IsActive) return;

        token.RevokedAtUtc = DateTime.UtcNow;
        token.RevokedByIp = ipAddress;

        await context.SaveChangesAsync(ct);
    }

    public static string GenerateToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(64);
        return Convert.ToBase64String(bytes);
    }
}
