using Api.Instartups.Auth.Configurations.Database;
using Api.Instartups.Auth.Interfaces;
using Api.Instartups.Auth.Models;
using Api.Instartups.Auth.Options;
using Api.Instartups.Auth.Repositories;
using Api.Instartups.Auth.Services;
using Microsoft.AspNetCore.Identity;

namespace Api.Instartups.Auth.Configurations;

public static class DependencyInjection
{
    public static IServiceCollection AddDependencyInjection(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddPostgresConf(configuration);
        
        services.AddOptions<JwtOptions>()
            .Bind(configuration.GetSection(JwtOptions.SectionName))
            .ValidateOnStart();

        services.AddOptions<RefreshTokenOptions>()
            .Bind(configuration.GetSection(RefreshTokenOptions.SectionName))
            .ValidateOnStart();

        services.AddScoped<IGenerateJwtService, GenerateJwtService>();
        services.AddScoped<IGenerateRefreshTokenService, GenerateRefreshTokenService>();
        services.AddScoped<IUserSessionRepository, UserSessionRepository>();
        
        services
            .AddIdentityCore<ApplicationUser>()
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<AppDbContext>();

        return services;
    }
}