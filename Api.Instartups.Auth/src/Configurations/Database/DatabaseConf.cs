using Microsoft.EntityFrameworkCore;

namespace Api.Instartups.Auth.Configurations.Database;


public static class DatabaseConf
{
    public static IServiceCollection AddPostgresConf(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var ConnectionString = configuration.GetConnectionString("PostgresConnection")
                               ?? throw new InvalidOperationException("Connection string 'PostgresConnection' não configurada.");

        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(
                ConnectionString
            )
        );
        

        return services;
    }
}