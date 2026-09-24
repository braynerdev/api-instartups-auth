using Microsoft.AspNetCore.Mvc;

namespace Api.Instartups.Auth.Configurations;

public static class ControllersConfig
{
    public static IServiceCollection AddControllersConfig(this IServiceCollection services)
    {
        services.AddControllers(options =>
        {
            options.ReturnHttpNotAcceptable = true;

            options.Filters.Add(new ProducesAttribute("application/json"));
            options.Filters.Add(new ConsumesAttribute("application/json"));

            options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = false;
            options.AllowEmptyInputInBodyModelBinding = false;
        });

        return services;
    }
}
