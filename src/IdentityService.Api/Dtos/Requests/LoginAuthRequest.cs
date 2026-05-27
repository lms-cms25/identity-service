namespace IdentityService.Api.Dtos.Requests;

public sealed record LoginAuthRequest(string Email, string Password);
