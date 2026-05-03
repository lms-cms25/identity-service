using IdentityService.Api.Abstractions;
using IdentityService.Api.Dto;
using IdentityService.Api.Identity;
using Microsoft.AspNetCore.Identity;

namespace IdentityService.Api.Services;

public class AuthService(UserManager<AppUser> userManager, IVerificationService verificationService) : IAuthService
{
    public async Task<CheckUserStatusResult> CheckUserStatusAsync(string email, CancellationToken ct = default)
    {
        var user = await userManager.FindByEmailAsync(email);

        if (user is null)
            return CheckUserStatusResult.NotFound(email, $"User with email '{email}' not found");

        if (!user.EmailConfirmed)
        {
            var token = await userManager.GenerateEmailConfirmationTokenAsync(user);

            await verificationService.SendEmailVerificationAsync(email, token, ct);
            return CheckUserStatusResult.NotVerified(email, $"User with email '{email}' not verified");
        }

        return CheckUserStatusResult.Verified(email);
    }

    public async Task<VerifyEmailVerificationCodeResult> VerifyVerificationCodeAsync(VerifyEmailVerificationCodeRequest request, CancellationToken ct = default)
    {
        var user = await userManager.FindByEmailAsync(request.Email);

        if (user is null)
            return VerifyEmailVerificationCodeResult.Failed($"User with email '{request.Email}' not found");

        var result = await verificationService.VerifyEmailVerificationCodeAsync(request, ct);

        if (result.Succeeded)
        {
            user.EmailConfirmed = true;
            await userManager.UpdateAsync(user);
        }

        return result;
    }
}
