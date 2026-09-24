namespace Api.Instartups.Auth.Configurations.Extension;

public static class AuthenticationMiddlewareExtensions
{
    public static IApplicationBuilder UseAuthenticationConf(this IApplicationBuilder app)
    {
        app.UseAuthentication();
        return app;
    }
}