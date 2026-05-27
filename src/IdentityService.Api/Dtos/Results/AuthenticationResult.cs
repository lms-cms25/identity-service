using IdentityService.Api.Contracts.Authentication;

namespace IdentityService.Api.Dtos.Results;

public sealed record AuthenticationResult(bool Succeeded, string Email, AuthenticationStage? Stage = null, string? Error = null)
{
    public static AuthenticationResult Success(string email, AuthenticationStage stage) => new(true, email, stage, null);
    public static AuthenticationResult Fail(string email, string errorMessage, AuthenticationStage? stage) => new(false, email, stage, errorMessage);    
}
