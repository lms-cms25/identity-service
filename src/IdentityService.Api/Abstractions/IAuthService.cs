using IdentityService.Api.Dto;

namespace IdentityService.Api.Abstractions;

public interface IAuthService
{
    Task<CheckUserStatusResult> CheckUserStatusAsync(string email, CancellationToken ct = default);
    Task<VerifyEmailVerificationCodeResult> VerifyVerificationCodeAsync(VerifyEmailVerificationCodeRequest request, CancellationToken ct = default);
}
