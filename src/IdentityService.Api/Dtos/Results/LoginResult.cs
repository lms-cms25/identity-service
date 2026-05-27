namespace IdentityService.Api.Dtos.Results;

public record LoginResult(
    bool Succeeded,
    string? AccessToken = null,
    string? TokenType = null,
    int? ExpiresIn = null,
    DateTime? ExpiresAtUtc = null,
    string? RefreshToken = null,
    LoginUserDto? User = null,
    string? Error = null,
    bool IsLockedOut = false);

public record LoginUserDto(string UserId, string Email, IList<string> Roles);