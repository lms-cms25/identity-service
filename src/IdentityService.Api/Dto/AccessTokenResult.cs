namespace IdentityService.Api.Dto;


public sealed record AccessTokenResult
(
    string AccessToken,
    string TokenType,
    int ExpiresIn,
    DateTime ExpiresAtUtc
);
