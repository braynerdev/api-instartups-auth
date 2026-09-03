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
            
            o.Password.RequireDigit = true;
            o.Password.RequireLowercase = true;
            o.Password.RequireNonAlphanumeric = true;
            o.Password.RequireUppercase = true;
            o.Password.RequiredLength = 1;
            o.Password.RequiredUniqueChars = 1;
            
            o.User.RequireUniqueEmail = true;
        });
        return services;
    }
}