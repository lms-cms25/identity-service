using IdentityService.Api.Identity;
using Microsoft.AspNetCore.Identity;

namespace IdentityService.Api.Data;

public static class IdentitySeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();

        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();

        var retry = 10;

        while (retry-- > 0)
        {
            try
            {
                if (!await roleManager.RoleExistsAsync("Admin"))
                    await roleManager.CreateAsync(new IdentityRole("Admin"));

                if (userManager.Users.Any())
                    return;

                var email = config["Admin:Email"];
                var password = config["Admin:Password"];

                if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
                    throw new Exception("Admin credentials are missing in configuration.");

                var adminUser = new AppUser
                {
                    UserName = email,
                    Email = email,
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(adminUser, password);

                if (result.Succeeded)
                    await userManager.AddToRoleAsync(adminUser, "Admin");

                return;
            }
            catch
            {
                await Task.Delay(3000);
            }
        }

        throw new Exception("Seeder failed after retries (DB not ready)");
    }
}
