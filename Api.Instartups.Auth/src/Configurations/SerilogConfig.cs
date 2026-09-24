using Serilog;

namespace Api.Instartups.Auth.Configurations;

public static class SerilogConfig
{
    public static void AddSerilogConfig(this WebApplicationBuilder builder)
    {
        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(builder.Configuration)
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .WriteTo.Debug()
            .CreateLogger();
        builder.Host.UseSerilog();

    }
}