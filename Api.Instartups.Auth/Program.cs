using Api.Instartups.Auth.Configurations;
using Api.Instartups.Auth.Configurations.Database;
using Api.Instartups.Auth.Configurations.Extension;
using Api.Instartups.Auth.Constants;
using JasperFx;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDependencyInjection(builder.Configuration);

builder.AddWolverineConf();

builder.Services
    .AddAuthenticationConf(builder.Configuration)
    .AddIdentityConf()
    .AddAuthorizationConfig()
    .AddControllersConfig()
    .AddLowerCaseConfig()
    .AddMappingConfig()
    .AddCorsConfigurations(builder.Configuration)
    .AddRateLimiterConfig()
    .AddOpenApiConfig();
    
builder.AddSerilogConfig();

var app = builder.Build();

var executandoComando = args.Length > 0 && !args[0].StartsWith('-');
if (!executandoComando)
{
    await app.ApplyMigrationsAsync();
    await app.Services.SeedIdentityDataAsync(builder.Configuration);
}

app.UseCors(CorsConst.Frontend);

app.UseSerilogConfig();

app.UseExceptionsMiddleware();

if (app.Environment.IsDevelopment())
    app.UseOpenApiConfig();

else
{
    app.UseHsts();
    app.UseHttpsRedirection();
}

app.UseAuthenticationConf();
app.UseRateLimiter();
app.UseAuthorization();

app.MapControllers();

return await app.RunJasperFxCommands(args);
