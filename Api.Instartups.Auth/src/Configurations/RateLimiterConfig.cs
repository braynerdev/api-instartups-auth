using Api.Instartups.Auth.Constants;
using Api.Instartups.Auth.DTOs;
using Microsoft.AspNetCore.RateLimiting;
using System.IdentityModel.Tokens.Jwt;
using System.Threading.RateLimiting;

namespace Api.Instartups.Auth.Configurations;

public static class RateLimiterConfig
{
    private const string AnonymousPartition = "anonymous";

    public static IServiceCollection AddRateLimiterConfig(this IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            options.OnRejected = async (context, ct) =>
            {
                await context.HttpContext.Response.WriteAsJsonAsync(
                    BaseResponseDTO<string>.Error(null, MessageError.RateLimit()), ct);
            };

            options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
            {
                var userId = GetUserId(httpContext);

                return RateLimitPartition.GetTokenBucketLimiter(
                    userId,
                    _ => new TokenBucketRateLimiterOptions
                    {
                        TokenLimit = 30,
                        TokensPerPeriod = 30,
                        ReplenishmentPeriod = TimeSpan.FromSeconds(10),
                        AutoReplenishment = true,
                        QueueLimit = 5,
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst
                    });
            });

            // Login, refresh e cadastro chegam sem token pelo BFF, então todos caem na mesma
            // partição: o limite é folgado e a força bruta por conta fica com o lockout do Identity.
            options.AddPolicy(RateLimitConst.Anonymous, httpContext =>
                RateLimitPartition.GetFixedWindowLimiter(
                    GetUserId(httpContext),
                    _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 60,
                        Window = TimeSpan.FromMinutes(1),
                        QueueLimit = 10,
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst
                    }));
        });

        return services;
    }

    private static string GetUserId(HttpContext httpContext)
    {
        return httpContext.User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
            ?? AnonymousPartition;
    }
}
