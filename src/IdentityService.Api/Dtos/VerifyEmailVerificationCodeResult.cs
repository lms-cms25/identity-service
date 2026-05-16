namespace IdentityService.Api.Dtos

public record VerifyEmailVerificationCodeResult(bool Succeeded, string? Email = null, string? Error = null)
{
    public static VerifyEmailVerificationCodeResult Success(string email) => new(true, email, null);
    public static VerifyEmailVerificationCodeResult Failed(string errorMessage) => new(false, errorMessage);
}
