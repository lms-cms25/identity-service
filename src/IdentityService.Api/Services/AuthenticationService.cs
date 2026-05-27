using IdentityService.Api.Abstractions;
using IdentityService.Api.Contracts.Authentication;
using IdentityService.Api.Dtos.Requests;
using IdentityService.Api.Dtos.Results;
using IdentityService.Api.Identity;
using Microsoft.AspNetCore.Identity;

namespace IdentityService.Api.Services;

public class AuthenticationService(
    UserManager<AppUser> userManager,
    IVerificationService verificationService,
    JwtTokenService jwtTokenService,
    RefreshTokenService refreshTokenService) : IAuthService
{
    public async Task<AuthenticationResult> CheckUserStatusAsync(string email, CancellationToken ct = default)
    {
        var user = await userManager.FindByEmailAsync(email);

        // Om användaren inte hittas är det ett rent misslyckande (Succeeded = false)
        if (user is null)
            return AuthenticationResult.Fail(email, $"User with email '{email}' not found.", null);

        // Om mailen inte är bekräftad (Succeeded = true, men vi är kvar på EmailPending)
        if (!user.EmailConfirmed)
        {
            var token = await userManager.GenerateEmailConfirmationTokenAsync(user);
            await verificationService.SendEmailVerificationAsync(email, token, ct);

            return AuthenticationResult.Success(email, AuthenticationStage.EmailPending);
        }

        var hasPassword = await userManager.HasPasswordAsync(user);
        if (hasPassword)
        {
            // Användaren har både verifierat mailen OCH satt ett lösenord -> Skicka direkt till Authenticated!
            return AuthenticationResult.Success(email, AuthenticationStage.Authenticated);
        }

        // Kontot finns och mailen är godkänd -> Dags att slutföra profilen
        return AuthenticationResult.Success(email, AuthenticationStage.ProfileIncomplete);
    }

    public async Task<AuthenticationResult> VerifyVerificationCodeAsync(VerifyEmailVerificationCodeRequest request, CancellationToken ct = default)
    {
        var user = await userManager.FindByEmailAsync(request.Email);

        if (user is null)
            return AuthenticationResult.Fail(request.Email, $"User with email '{request.Email}' not found.", null);

        var result = await verificationService.VerifyEmailVerificationCodeAsync(request, ct);

        if (result.Succeeded)
        {
            user.EmailConfirmed = true;
            await userManager.UpdateAsync(user);

            return AuthenticationResult.Success(request.Email, AuthenticationStage.EmailVerified);
        }

        // Kodverifieringen felade (t.ex. fel kod) -> Stanna kvar i EmailPending
        return AuthenticationResult.Fail(
            request.Email,
            result.Error ?? "Invalid verification code.",
            AuthenticationStage.EmailPending);
    }

    public async Task<AuthenticationResult> CompleteProfileAsync(CompleteProfileRequest request, CancellationToken ct = default)
    {
        if (!request.AcceptTerms)
            return AuthenticationResult.Fail(request.Email, "You must accept the terms and conditions to complete your profile.", AuthenticationStage.ProfileIncomplete);

        var user = await userManager.FindByEmailAsync(request.Email);

        if (user is null)
            return AuthenticationResult.Fail(request.Email, $"User with email '{request.Email}' not found.", null);

        if (!user.EmailConfirmed)
            return AuthenticationResult.Fail(request.Email, "Email must be verified before completing profile.", AuthenticationStage.EmailPending);

        // Sätt lösenordet i Identity
        var passwordResult = await userManager.AddPasswordAsync(user, request.Password);

        if (!passwordResult.Succeeded)
        {
            var firstError = passwordResult.Errors.FirstOrDefault()?.Description ?? "Failed to set password.";
            return AuthenticationResult.Fail(request.Email, firstError, AuthenticationStage.ProfileIncomplete);
        }

        // Allt klart! Kontot är aktiverat och redo.
        return AuthenticationResult.Success(request.Email, AuthenticationStage.Authenticated);
    }

    public async Task<LoginResult> LoginAsync(LoginAuthRequest request, string? ipAddress, CancellationToken ct = default)
    {
        var email = request.Email.Trim().ToLowerInvariant();

        var user = await userManager.FindByEmailAsync(email);
        if (user is null)
            return new LoginResult(false, Error: "Invalid email or password.");

        if (await userManager.IsLockedOutAsync(user))
            return new LoginResult(false, Error: "User is temporarily locked out.", IsLockedOut: true);

        var passwordValid = await userManager.CheckPasswordAsync(user, request.Password);
        if (!passwordValid)
        {
            await userManager.AccessFailedAsync(user);
            return new LoginResult(false, Error: "Invalid email or password.");
        }

        // Återställ felaktiga försök vid lyckad inloggning
        await userManager.ResetAccessFailedCountAsync(user);

        var roles = await userManager.GetRolesAsync(user);
        var accessToken = jwtTokenService.CreateAccessToken(user, roles);
        var refreshToken = await refreshTokenService.CreateAsync(user.Id, ipAddress, ct);

        return new LoginResult(
            Succeeded: true,
            AccessToken: accessToken.AccessToken,
            TokenType: accessToken.TokenType,
            ExpiresIn: accessToken.ExpiresIn,
            ExpiresAtUtc: accessToken.ExpiresAtUtc,
            RefreshToken: refreshToken,
            User: new LoginUserDto(user.Id, user.Email!, roles)
        );
    }

    public async Task<LoginResult> RefreshAsync(RefreshAuthRequest request, string? ipAddress, CancellationToken ct = default)
    {
        // 1. Rotera refresh-token via din existerande tjänst
        var rotateResult = await refreshTokenService.RotateAsync(request.RefreshToken, ipAddress, ct);

        if (!rotateResult.Succeeded || string.IsNullOrWhiteSpace(rotateResult.UserId))
            return new LoginResult(false, Error: "Invalid or expired refresh token.");

        // 2. Hämta användaren
        var user = await userManager.FindByIdAsync(rotateResult.UserId);
        if (user is null)
            return new LoginResult(false, Error: "User not found.");

        // 3. Generera nya access-tokens
        var roles = await userManager.GetRolesAsync(user);
        var accessToken = jwtTokenService.CreateAccessToken(user, roles);

        // 4. Returnera det uppdaterade resultatet
        return new LoginResult(
            Succeeded: true,
            AccessToken: accessToken.AccessToken,
            TokenType: accessToken.TokenType,
            ExpiresIn: accessToken.ExpiresIn,
            ExpiresAtUtc: accessToken.ExpiresAtUtc,
            RefreshToken: rotateResult.NewRefreshToken, // Den nya roterade token
            User: new LoginUserDto(user.Id, user.Email!, roles)
        );
    }
}