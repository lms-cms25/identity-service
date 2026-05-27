namespace IdentityService.Api.Dtos;

public sealed record RegisterAuthRequest(string Email, string Password, string RoleName);
