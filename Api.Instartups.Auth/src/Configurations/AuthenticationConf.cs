using Api.Instartups.Auth.DTOs;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Api.Instartups.Auth.Configurations;

public static class AuthenticationConf
{
    public static IServiceCollection AddAuthenticationConf(this IServiceCollection services, IConfiguration configuration)
    {
        var pubKeyPath = configuration["JWT:PublicKeyPath"] ?? throw new ArgumentException("Caminho da chave pública inválido!");
        var pubKey = File.ReadAllText(pubKeyPath, Encoding.UTF8) ?? throw new ArgumentException("Chave pública inválida!");
        var issuer = configuration["JWT:Issuer"] ?? throw new ArgumentException("Emissor inválido!");

        var rsa = RSA.Create();
        rsa.ImportFromPem(pubKey);

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.SaveToken = false;
            options.RequireHttpsMetadata = false;

            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = issuer,
                ValidateAudience = false,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ClockSkew = TimeSpan.Zero,
                IssuerSigningKey = new RsaSecurityKey(rsa)
            };

            options.Events = new JwtBearerEvents
            {
                OnAuthenticationFailed = context =>
                {
                    return Task.CompletedTask;
                },

                OnForbidden = async context =>
                {

                    context.Response.StatusCode = StatusCodes.Status403Forbidden;
                    context.Response.ContentType = "application/json";

                    var result = JsonSerializer.Serialize(
                        BaseResponseDTO<string>.Error("Você não possui permissão para acessar este recurso."));

                    await context.Response.WriteAsync(result);
                },

                OnChallenge = async context =>
                {
                    context.HandleResponse();
                    if (context.Response.HasStarted)
                        return;


                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    context.Response.ContentType = "application/json";

                    var message = GetMessageError(context);
                    var result = JsonSerializer.Serialize(
                        BaseResponseDTO<string>.Error(message));

                    await context.Response.WriteAsync(result);
                }
            };
        });

        return services;
    }

    private static string GetMessageError(JwtBearerChallengeContext context)
    {
        return context.AuthenticateFailure switch
        {
            SecurityTokenExpiredException =>
                "O token de acesso expirou.",

            _ =>
                "É necessário estar autenticado para acessar este recurso."
        };
    }
}