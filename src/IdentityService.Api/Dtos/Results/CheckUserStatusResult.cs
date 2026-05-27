using IdentityService.Api.Contracts.Authentication;

namespace IdentityService.Api.Dtos.Results;

public record CheckUserStatusResult(bool Succeeded, bool IsVerified, AuthenticationStage? Stage, string? Email = null, string? Error = null)
{
    public static CheckUserStatusResult Verified(string email) => new(true, true, AuthenticationStage.ProfileIncomplete, email, null);

    public static CheckUserStatusResult NotVerified(string email, string errorMessage) => new(true, false, AuthenticationStage.EmailPending,email, errorMessage);
    public static CheckUserStatusResult NotFound(string email, string errorMessage) => new(false, false, null, email, errorMessage);

}
