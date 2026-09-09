using Microsoft.AspNetCore.Identity;

namespace Api.Instartups.Auth.Configurations;

public static class IdentityConf
{
    public static IServiceCollection AddIdentityConf(this IServiceCollection services)
    {
        services.Configure<IdentityOptions>(o =>
        {
            
            o.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
            o.Lockout.MaxFailedAccessAttempts = 5;
            o.Lockout.AllowedForNewUsers = true;
            
            o.Password.RequireDigit = false;
            o.Password.RequireLowercase = false;
            o.Password.RequireNonAlphanumeric = false;
            o.Password.RequireUppercase = false;
            o.Password.RequiredLength = 1;
            o.Password.RequiredUniqueChars = 0;
            
            o.User.RequireUniqueEmail = true;
        });
        
        services.Configure<PasswordHasherOptions>(options =>
        {
            options.IterationCount = 1000;
        });
        return services;
    }
}