using IdentityService.Api.Contracts.Authentication;

namespace IdentityService.Api.Dtos.Results;

public record VerifyEmailVerificationCodeResult(bool Succeeded, AuthenticationStage? NextStage = null, string? Email = null, string? Error = null)
{
    public static VerifyEmailVerificationCodeResult Success(string email) => new(true, AuthenticationStage.EmailVerified, email, null);
    public static VerifyEmailVerificationCodeResult Failed(string errorMessage) => new(false, null, null, errorMessage);
}
