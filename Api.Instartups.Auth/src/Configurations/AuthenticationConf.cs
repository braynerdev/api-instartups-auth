using Api.Instartups.Auth.DTOs;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Api.Instartups.Auth.Configurations;

public static class AuthenticationConf
{
    public static IServiceCollection AddAuthenticationConf(this IServiceCollection services, IConfiguration configuration)
    {
        var pubKeyPath = configuration["Jwt:PublicKeyPath"] ?? throw new ArgumentException("Caminho da chave pública inválido!");
        var pubKey = File.ReadAllText(pubKeyPath, Encoding.UTF8) ?? throw new ArgumentException("Chave pública inválida!");
        var issuer = configuration["Jwt:Issuer"] ?? throw new ArgumentException("Emissor inválido!");
        var audience = configuration["Jwt:Audience"] ?? throw new ArgumentException("Público-alvo inválido!");

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
            options.MapInboundClaims = false;

            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = issuer,
                ValidateAudience = true,
                ValidAudience = audience,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ClockSkew = TimeSpan.Zero,
                IssuerSigningKey = new RsaSecurityKey(rsa),
                NameClaimType = JwtRegisteredClaimNames.Sub,
                RoleClaimType = "roles",
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
                        BaseResponseDTO<string>.Error(null, "Você não possui permissão para acessar este recurso."), JsonSerializerOptions.Web);

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
                        BaseResponseDTO<string>.Error(null, message), JsonSerializerOptions.Web);

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