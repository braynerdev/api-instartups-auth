using Api.Instartups.Auth.Common.Identity;
using Api.Instartups.Auth.Constants;
using Api.Instartups.Auth.Models;
using Microsoft.AspNetCore.Identity;

namespace Api.Instartups.Auth.Configurations.Database;

public static class IdentitySeeder
{
    public static async Task SeedIdentityDataAsync(this IServiceProvider services, IConfiguration configuration)
    {
        using var scope = services.CreateScope();
        var provider = scope.ServiceProvider;

        await SeedRolesAsync(provider);
        await SeedAdminUserAsync(provider, configuration);
    }

    private static async Task SeedRolesAsync(IServiceProvider provider)
    {
        var roleManager = provider.GetRequiredService<RoleManager<IdentityRole>>();

        foreach (var permission in PermissionConst.All)
        {
            if (!await roleManager.RoleExistsAsync(permission.Name))
            {
                await roleManager.CreateAsync(new IdentityRole(permission.Name));
            }
        }
    }

    private static async Task SeedAdminUserAsync(IServiceProvider provider, IConfiguration configuration)
    {
        var email = configuration["AdminUser:Email"];
        var userName = configuration["AdminUser:UserName"];
        var password = configuration["AdminUser:Password"];

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(userName) || string.IsNullOrWhiteSpace(password))
        {
            return;
        }

        var userManager = provider.GetRequiredService<UserManager<ApplicationUser>>();

        var user = await userManager.FindByEmailAsync(email);
        if (user is null)
        {
            user = new ApplicationUser { UserName = userName, Email = email };
            var result = await userManager.CreateAsync(user, password);
            result.EnsureSucceeded();
        }

        if (!await userManager.IsInRoleAsync(user, PermissionConst.Admin))
        {
            await userManager.AddToRoleAsync(user, PermissionConst.Admin);
        }
    }
}
