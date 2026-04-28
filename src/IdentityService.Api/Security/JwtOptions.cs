using Microsoft.IdentityModel.Tokens;

namespace IdentityService.Api.Security;

public sealed class JwtOptions
{
    public const string SectionName = "Jwt";
    public string Issuer { get; init; } = null!;
    public string Audience { get; init; } = null!;
    public string SigningKey { get; init; } = null!;
    public int AccessTokenMinutes { get; init; } = 10;
    public int RefreshTokenDays { get; init; } = 60;
    public SymmetricSecurityKey GetSigningKey() => new(Convert.FromBase64String(SigningKey));
}
