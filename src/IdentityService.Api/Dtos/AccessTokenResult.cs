namespace IdentityService.Api.Dtos;


public sealed record AccessTokenResult
(
    string AccessToken,
    string TokenType,
    int ExpiresIn,
    DateTime ExpiresAtUtc
);
