using IdentityService.Api.Dto;
using IdentityService.Api.Identity;
using Microsoft.AspNetCore.Identity;

namespace IdentityService.Api.Endpoints;

public static class AccountAdministrationEndpoints
{
    public static void MapAccountAdministrationEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/admin");

        group.MapPost("/register", Register);
    }

    private static async Task<IResult> Register(RegisterAuthRequest request, UserManager<AppUser> userManager, RoleManager<IdentityRole> roleManager)
    {
        var email = request.Email.Trim().ToLowerInvariant();

        var user = new AppUser
        {
            UserName = email,
            Email = email,
        };

        var role = await roleManager.FindByNameAsync(request.RoleName);
        if(role is null)
        {
            IdentityRole newRole = new(request.RoleName);
            await roleManager.CreateAsync(newRole);
        }

        var result = await userManager.CreateAsync(user, request.Password);

        if (!result.Succeeded)
            return Results.ValidationProblem(result.Errors.ToDictionary(x => x.Code, x => new[] { x.Description }));

        var roleResult = await userManager.AddToRoleAsync(user, request.RoleName);
        if (!roleResult.Succeeded)
            return Results.ValidationProblem(roleResult.Errors.ToDictionary(x => x.Code, x => new[] { x.Description }));

        return Results.Created();
    }
}
