using Api.Instartups.Auth.Configurations.Database;
using Microsoft.AspNetCore.Identity;

namespace Api.Instartups.Auth.Configurations;

public static class DependencyInjection
{
    public static IServiceCollection AddDependencyInjection(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddPostgresConf(configuration);
        
        services
            .AddIdentityCore<IdentityUser>()
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<AppDbContext>();

        return services;
    }
}