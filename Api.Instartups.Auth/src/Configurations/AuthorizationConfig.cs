using Api.Instartups.Auth.Constants;
using Microsoft.AspNetCore.Authorization;

namespace Api.Instartups.Auth.Configurations;

public static class AuthorizationConfig
{
    public static IServiceCollection AddAuthorizationConfig(this IServiceCollection services)
    {
        services.AddAuthorization(options =>
        {
            foreach (var permission in PermissionConst.All)
            {
                AddPermissionPolicy(options, permission.Name);
            }
        });

        return services;
    }

    private static void AddPermissionPolicy(AuthorizationOptions options, string permission)
    {
        options.AddPolicy(permission,
            policy =>
            {
                policy.RequireAuthenticatedUser();
                policy.RequireRole(permission);
            });
    }
}
