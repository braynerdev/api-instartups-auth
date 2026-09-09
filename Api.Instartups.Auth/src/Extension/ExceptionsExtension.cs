using Api.Instartups.Auth.Middleware;

namespace Api.Instartups.Auth.Configurations.Extension;

public static class ExceptionsExtension
{
    public static void UseExceptionsMiddleware(this IApplicationBuilder app)
    {
        app.UseMiddleware<ExceptionsMiddleware>();
    }
}