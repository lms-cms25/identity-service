namespace IdentityService.Api.Dtos.Results;

public sealed record CompleteProfileResult(bool Succeeded, string Email, string? Error)
{
    public static CompleteProfileResult NotFound(string email, string errorMessage) => new(false, email, errorMessage);
    public static CompleteProfileResult NotVerified(string email, string errorMessage) => new(false, email, errorMessage);
}
