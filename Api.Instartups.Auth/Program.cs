using Api.Instartups.Auth.Configurations;
using Api.Instartups.Auth.Configurations.Database;
using Api.Instartups.Auth.Configurations.Extension;
using JasperFx;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

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
    .AddOpenApi()
    .AddSwaggerConfig(builder.Configuration);
    
builder.AddSerilogConfig();

var app = builder.Build();

var executandoComando = args.Length > 0 && !args[0].StartsWith('-');
if (!executandoComando)
    await app.ApplyMigrationsAsync();

await app.Services.SeedIdentityDataAsync(builder.Configuration);

app.UseSerilogConfig();

app.UseExceptionsMiddleware();

app.UseAuthenticationConf();

app.UseAuthorization();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerExtension(builder.Configuration);
}

//app.UseHttpsRedirection();

app.MapControllers();

return await app.RunJasperFxCommands(args);
