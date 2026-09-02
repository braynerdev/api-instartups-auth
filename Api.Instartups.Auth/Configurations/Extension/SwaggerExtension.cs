namespace Api.Instartups.Auth.Configurations.Extension;

public static class SwaggerExtension
{
    public static WebApplication UseSwaggerExtension(this WebApplication app, IConfiguration configuration)
    {
        string nomeApi = configuration["AppDoc:NomeApi"] 
                         ?? throw new ArgumentException("Nome da API inválido");
        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", nomeApi);
            options.RoutePrefix = "swagger-ui";
            options.DocumentTitle = nomeApi;
        });
        return app;
    }
}