using Microsoft.OpenApi;
using Scalar.AspNetCore;

namespace Api.Instartups.Auth.Configurations;

public static class OpenApiConfig
{
    private const string _apiName = "Instartups Auth";
    private const string _apiDescription = "API responsável pelo gerenciamento de autenticação e acesso aos serviços da Instartups, plataforma que conecta startups do Porto Digital a mentores e investidores anjo do Nordeste.";

    public static IServiceCollection AddOpenApiConfig(this IServiceCollection services)
    {
        services.AddOpenApi("v1", options =>
        {
            options.AddDocumentTransformer((document, context, cancellationToken) =>
            {
                document.Info = new OpenApiInfo
                {
                    Title = _apiName,
                    Version = "v1",
                    Description = _apiDescription
                };

                document.Components ??= new OpenApiComponents();
                document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();
                document.Components.SecuritySchemes["Bearer"] = new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "Informe apenas o token JWT, sem o prefixo Bearer."
                };

                document.Security ??= [];
                document.Security.Add(new OpenApiSecurityRequirement
                {
                    [new OpenApiSecuritySchemeReference("Bearer", document)] = []
                });

                return Task.CompletedTask;
            });
        });

        return services;
    }

    public static WebApplication UseOpenApiConfig(this WebApplication app)
    {
        app.UseStaticFiles();
        app.MapOpenApi();
        app.MapScalarApiReference("/docs", options =>
        {
            options
                .WithTitle(_apiName)
                .WithFavicon("/img/favicon-logo.svg")
                .WithJavaScriptConfiguration("/js/scalar.js");
        });

        return app;
    }
}
