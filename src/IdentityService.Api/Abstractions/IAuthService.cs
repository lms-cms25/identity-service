using IdentityService.Api.Dtos.Results;
using IdentityService.Api.Dtos.Requests;


namespace IdentityService.Api.Abstractions;

public interface IAuthService
{
    Task<AuthenticationResult> CheckUserStatusAsync(string email, CancellationToken ct = default);
    Task<AuthenticationResult> VerifyVerificationCodeAsync(VerifyEmailVerificationCodeRequest request, CancellationToken ct = default);
    Task<AuthenticationResult> CompleteProfileAsync(CompleteProfileRequest request, CancellationToken ct = default);
    Task<LoginResult> LoginAsync(LoginAuthRequest request, string? ipAddress, CancellationToken ct = default);
    Task<LoginResult> RefreshAsync(RefreshAuthRequest request, string? ipAddress, CancellationToken ct = default);
}
