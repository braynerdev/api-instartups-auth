using Api.Instartups.Auth.Configurations;
using Api.Instartups.Auth.Configurations.Extension;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDependencyInjection(builder.Configuration);

builder.Services
    .AddIdentityConf()
    .AddLowerCaseConfig()
    .AddMappingConfig()
    .AddOpenApi()
    .AddSwaggerConfig(builder.Configuration);
    
builder.AddSerilogConfig();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerExtension(builder.Configuration);
}

app.UseHttpsRedirection();


app.Run();
