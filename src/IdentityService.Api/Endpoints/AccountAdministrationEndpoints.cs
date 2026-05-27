using IdentityService.Api.Data;
using IdentityService.Api.Dtos.Requests;
using IdentityService.Api.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IdentityService.Api.Endpoints;

public static class AccountAdministrationEndpoints
{
    public static void MapAccountAdministrationEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/admin").RequireAuthorization("AdminOnly");

        group.MapPost("/register", Register);
        group.MapGet("/all-users", GetAllUsersWithRoles);
    }

    private static async Task<IResult> Register(RegisterAuthRequest request, UserManager<AppUser> userManager, RoleManager<IdentityRole> roleManager)
    {
        var email = request.Email.Trim().ToLowerInvariant();

        if (string.IsNullOrWhiteSpace(request.RoleName))
        {
            return Results.BadRequest("Role name cannot be empty.");
        }

        var user = new AppUser
        {
            UserName = email,
            Email = email,
            EmailConfirmed = false
        };
        string normalizedRole = char.ToUpper(request.RoleName[0]) + request.RoleName.Substring(1).ToLower();

        var role = await roleManager.FindByNameAsync(normalizedRole);
        if(role is null)
        {
            IdentityRole newRole = new(normalizedRole);
            await roleManager.CreateAsync(newRole);
        }

        var result = await userManager.CreateAsync(user);

        if (!result.Succeeded)
            return Results.ValidationProblem(result.Errors.ToDictionary(x => x.Code, x => new[] { x.Description }));


        var roleResult = await userManager.AddToRoleAsync(user, normalizedRole);
        if (!roleResult.Succeeded)
            return Results.ValidationProblem(roleResult.Errors.ToDictionary(x => x.Code, x => new[] { x.Description }));

        return Results.Created();
    }

   
    private static async Task<IResult> GetAllUsersWithRoles(
        UserManager<AppUser> userManager,
        [FromServices] DataContext db) 
    {
        var usersWithRoles = await (from user in db.Users
                                    select new
                                    {
                                        user.Id,
                                        user.UserName,
                                        user.Email,
                                        Roles = (from userRole in db.UserRoles
                                                 join role in db.Roles on userRole.RoleId equals role.Id
                                                 where userRole.UserId == user.Id
                                                 select role.Name).ToList()
                                    }).ToListAsync();

        return Results.Ok(usersWithRoles);
    }
}
