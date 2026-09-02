using Mapster;

namespace Api.Instartups.Auth.Configurations;

public static class MappingConfig
{
    public static IServiceCollection AddMappingConfig(this IServiceCollection services)
    {
        services.AddMapster();

        return services;
    }
}