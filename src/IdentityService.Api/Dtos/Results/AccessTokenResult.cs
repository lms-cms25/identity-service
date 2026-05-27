namespace IdentityService.Api.Dtos.Results;


public sealed record AccessTokenResult
(
    string AccessToken,
    string TokenType,
    int ExpiresIn,
    DateTime ExpiresAtUtc
);
