using IdentityService.Api.Dto;

namespace IdentityService.Api.Abstractions;

public interface IVerificationService
{
    Task PublicEmailVerificationAsync(string email, CancellationToken ct = default);
    Task SendEmailVerificationAsync(string email, string token, CancellationToken ct = default);
    Task<VerifyEmailVerificationCodeResult> VerifyEmailVerificationCodeAsync(VerifyEmailVerificationCodeRequest request, CancellationToken ct = default);
}
