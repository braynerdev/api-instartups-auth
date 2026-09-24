using Microsoft.OpenApi;

namespace Api.Instartups.Auth.Configurations;

public static class SwaggerConfig
{
    public static IServiceCollection AddSwaggerConfig(this IServiceCollection services, IConfiguration configuration)
    {
        string nomeApi = configuration["AppDoc:NomeApi"] 
            ?? throw new ArgumentException("Nome da API inválido");
        
        string descricaoApi = configuration["AppDoc:DescricaoApi"]
            ?? throw new ArgumentException("Descrição da API inválido");
        
        
    
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = nomeApi,
                Version = "v1",
                Description = descricaoApi
            });

            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.ApiKey,
                Scheme = "Bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "Informe o token JWT no formato: Bearer {seu token}",
            });

            c.AddSecurityRequirement(document => new OpenApiSecurityRequirement
            {
                [new OpenApiSecuritySchemeReference("Bearer", document)] = []
            });
        });

        return services;
    }
}