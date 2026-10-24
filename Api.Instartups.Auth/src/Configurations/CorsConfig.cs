using Api.Instartups.Auth.Constants;

namespace Api.Instartups.Auth.Configurations;

public static class CorsConfig
{
    public static IServiceCollection AddCorsConfigurations(this IServiceCollection services, IConfiguration configuration)
    {
        var origensBase = configuration.GetSection("CorsPadrao").Get<string[]>()
            ?? throw new ArgumentException("Lista de origens base inválida.");

        services.AddCors(options =>
        {
            options.AddPolicy(CorsConst.Frontend, policy =>
            {
                policy.WithOrigins(origensBase)
                    .AllowAnyHeader()
                    .AllowAnyMethod();
            });
        });

        return services;
    }
}
