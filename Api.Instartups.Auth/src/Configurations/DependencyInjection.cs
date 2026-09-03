using Api.Instartups.Auth.Configurations.Database;

namespace Api.Instartups.Auth.Configurations;

public static class DependencyInjection
{
    public static IServiceCollection AddDependencyInjection(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddPostgresConf(configuration);

        return services;
    }
}