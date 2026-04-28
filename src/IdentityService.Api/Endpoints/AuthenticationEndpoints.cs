using IdentityService.Api.Dto;
using IdentityService.Api.Identity;
using IdentityService.Api.Security;
using IdentityService.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

namespace IdentityService.Api.Endpoints;

public static class AuthenticationEndpoints
{
    public static void MaoAuthenticationEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/auth");

        group.MapPost("/register", Register);
        //.RequiredAuthorization(policy => policy.RequireRole("Admin"));
        group.MapPost("/login", Login);
        group.MapPost("/refresh", Refresh);
        group.MapPost("/logout", Logout);
        group.MapGet("/me", Me).RequireAuthorization();

    }





    private static async Task<IResult> Register(RegisterAuthRequest request, UserManager<AppUser> userManager)
    {
        var email = request.Email.Trim().ToLowerInvariant();

        var user = new AppUser
        {
            UserName = email,
            Email = email,
        };

        var result = await userManager.CreateAsync(user, request.Password);

        if (!result.Succeeded)
            return Results.ValidationProblem(result.Errors.ToDictionary(x => x.Code, x => new[] { x.Description }));

        return Results.Created($"/api/users/{user.Id}", new { user.Id, user.Email });
    }




    //kan ha sin egna authorization service: UserManager<AppUser> userManager, JwtTokenService jwtTokenService, RefreshTokenService refreshTokenService, HttpContext httpContext, CancellationToken ct = default




    private static async Task<IResult> Login(LoginAuthRequest request, UserManager<AppUser> userManager, JwtTokenService jwtTokenService, RefreshTokenService refreshTokenService, HttpContext httpContext, CancellationToken ct = default)
    {
        var email = request.Email.Trim().ToLowerInvariant();

        var user = await userManager.FindByEmailAsync(email);
        if (user is null)
            return Results.Unauthorized();

        if (await userManager.IsLockedOutAsync(user))
            return Results.Problem(title: "Locked out", detail: "User is remporary locked out", statusCode: StatusCodes.Status423Locked);

        var passwordValid = await userManager.CheckPasswordAsync(user, request.Password);
        if (!passwordValid)
        {
            await userManager.AccessFailedAsync(user);
            return Results.Unauthorized();
        }

        await userManager.ResetAccessFailedCountAsync(user);

        var roles = await userManager.GetRolesAsync(user);
        var accessToken = jwtTokenService.CreateAccessToken(user, roles);
        var refreshToken = await refreshTokenService.CreateAsync(user.Id, httpContext.Connection.RemoteIpAddress?.ToString(), ct);

        //här kan det vara cleanare att ha en ResultDto
        return Results.Ok(new
        {
            accessToken = accessToken.AccessToken,
            accessToken.TokenType,
            accessToken.ExpiresIn,
            accessToken.ExpiresAtUtc,
            refreshToken,
            user = new
            {
                userId = user.Id,
                email = user.Email,
                roles
            }
        });


    }






    private static async Task<IResult> Refresh(RefreshAuthRequest request, UserManager<AppUser> userManager, JwtTokenService jwtTokenService, RefreshTokenService refreshTokenService, HttpContext httpContext, CancellationToken ct = default)
    {
        var result = await refreshTokenService.RotateAsync(request.RefreshToken, httpContext.Connection.RemoteIpAddress?.ToString(), ct);
        if (!result.Succeeded || string.IsNullOrWhiteSpace(result.UserId))
            return Results.Unauthorized();

        var user = await userManager.FindByIdAsync(result.UserId);
        if (user is null) return Results.Unauthorized();

        var roles = await userManager.GetRolesAsync(user);
        var accessToken = jwtTokenService.CreateAccessToken(user, roles);

        return Results.Ok(new
        {
            accessToken = accessToken.AccessToken,
            accessToken.TokenType,
            accessToken.ExpiresIn,
            accessToken.ExpiresAtUtc,
            refreshToken = result.NewRefreshToken,
            user = new
            {
                userId = user.Id,
                email = user.Email,
                roles
            }
        });
    }




    private static async Task<IResult> Logout(LogoutAuthRequest request, RefreshTokenService refreshTokenService, HttpContext httpContext, CancellationToken ct = default)
    {
        await refreshTokenService.RevokeAsync(request.RefreshToken, httpContext.Connection.RemoteIpAddress?.ToString(), ct);
        return Results.NoContent();
    }






    [Authorize]
    private static async Task<IResult> Me(HttpContext httpContext, UserManager<AppUser> userManager, CancellationToken ct = default)
    {

        var userId = httpContext.User.FindFirst(JwtClaimTypes.UserId)?.Value;
        var email = httpContext.User.FindFirst(JwtClaimTypes.Email)?.Value;
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
