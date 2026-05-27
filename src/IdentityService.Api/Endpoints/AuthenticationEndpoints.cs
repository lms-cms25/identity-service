using IdentityService.Api.Abstractions;
using IdentityService.Api.Dtos.Requests;
using IdentityService.Api.Identity;
using IdentityService.Api.Security;
using IdentityService.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

namespace IdentityService.Api.Endpoints;

public static class AuthenticationEndpoints
{
    public static void MapAuthenticationEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/auth");

        group.MapPost("/check", Check);
        group.MapPost("/verify", Verify);
        group.MapPost("/complete-profile", CompleteProfile);
        group.MapPost("/login", Login);
        group.MapPost("/refresh", Refresh);
        group.MapPost("/logout", Logout);
        group.MapGet("/me", Me).RequireAuthorization();
    }

    private static async Task<IResult> Check(CheckUserStatusRequest request, IAuthService authService, CancellationToken ct = default)
    {
        var result = await authService.CheckUserStatusAsync(request.Email, ct);

        if (!result.Succeeded)
            return Results.BadRequest(result);

        return Results.Ok(result);
    }

    private static async Task<IResult> Verify(VerifyEmailVerificationCodeRequest request, IAuthService authService, CancellationToken ct = default)
    {
        var result = await authService.VerifyVerificationCodeAsync(request, ct);

        if (!result.Succeeded)
            return Results.BadRequest(result);

        return Results.Ok(result);
    }

    private static async Task<IResult> CompleteProfile(CompleteProfileRequest request, IAuthService authService, CancellationToken ct = default)
    {
        var result = await authService.CompleteProfileAsync(request, ct);

        if (!result.Succeeded)
            return Results.BadRequest(result);

        return Results.Ok(result);
    }

    private static async Task<IResult> Login(LoginAuthRequest request, IAuthService authService, HttpContext httpContext, CancellationToken ct = default)
    {
        var ipAddress = httpContext.Connection.RemoteIpAddress?.ToString();
        var result = await authService.LoginAsync(request, ipAddress, ct);

        if (!result.Succeeded)
        {
            if (result.IsLockedOut)
                return Results.Problem(title: "Locked out", detail: result.Error, statusCode: StatusCodes.Status423Locked);

            return Results.Unauthorized();
        }

        return Results.Ok(result);
    }

    private static async Task<IResult> Refresh(RefreshAuthRequest request, IAuthService authService, HttpContext httpContext, CancellationToken ct = default)
    {
        var ipAddress = httpContext.Connection.RemoteIpAddress?.ToString();
        var result = await authService.RefreshAsync(request, ipAddress, ct);

        if (!result.Succeeded)
            return Results.Unauthorized();

        return Results.Ok(result);
    }

    private static async Task<IResult> Logout(LogoutAuthRequest request, RefreshTokenService refreshTokenService, HttpContext httpContext, CancellationToken ct = default)
    {
        // Tips för framtiden: Även denna Revoke kan flyttas till din AuthService om du vill ta arkitekturen hela vägen ut, men den fungerar fint här så länge.
        await refreshTokenService.RevokeAsync(request.RefreshToken, httpContext.Connection.RemoteIpAddress?.ToString(), ct);
        return Results.NoContent();
    }

    private static async Task<IResult> Me(HttpContext httpContext, UserManager<AppUser> userManager, CancellationToken ct = default)
    {
        // Tips för framtiden: Du kan skapa en metod 'authService.GetMeAsync(userId)' för att städa denna sista lilla rest också!
        var userId = httpContext.User.FindFirst(JwtClaimTypes.UserId)?.Value;
        var roles = httpContext.User.FindAll(JwtClaimTypes.Role).Select(x => x.Value).ToArray();

        if (string.IsNullOrWhiteSpace(userId))
            return Results.Unauthorized();

        var user = await userManager.FindByIdAsync(userId);
        if (user is null)
            return Results.Unauthorized();

        return Results.Ok(new
        {
            user.Id,
            user.Email,
            user.PhoneNumber,
            roles
        });
    }
}