using Api.Instartups.Auth.Configurations;
using Api.Instartups.Auth.Configurations.Database;
using Api.Instartups.Auth.Configurations.Extension;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDependencyInjection(builder.Configuration);

builder.AddWolverineConf();

builder.Services
    .AddIdentityConf()
    .AddControllersConfig()
    .AddLowerCaseConfig()
    .AddMappingConfig()
    .AddOpenApi()
    .AddSwaggerConfig(builder.Configuration);
    
builder.AddSerilogConfig();

var app = builder.Build();

app.UseExceptionsMiddleware();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerExtension(builder.Configuration);
}

//app.UseHttpsRedirection();


app.MapControllers();

app.Run();
