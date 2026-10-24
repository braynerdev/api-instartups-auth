using Api.Instartups.Auth.Constants;
using Api.Instartups.Auth.DTOs;
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
            options.Filters.Add(new ProducesResponseTypeAttribute<BaseResponseDTO<string>>(StatusCodes.Status429TooManyRequests));
            options.Filters.Add(new ProducesResponseTypeAttribute<BaseResponseDTO<string>>(StatusCodes.Status401Unauthorized));

            options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true;
            options.AllowEmptyInputInBodyModelBinding = false;
        });

        services.Configure<ApiBehaviorOptions>(options =>
        {
            options.InvalidModelStateResponseFactory = context =>
            {
                var errors = context.ModelState
                    .Where(e => e.Value?.Errors.Count > 0)
                    .Select(e => new ValidateErrorDTO(
                        e.Key,
                        CodeError.Format,
                        [MessageError.InvalidRequestFormat()]
                    ))
                    .ToList();

                var body = BaseResponseDTO<IEnumerable<ValidateErrorDTO>>.Error(
                    errors, MessageError.InvalidRequestFormat());

                return new BadRequestObjectResult(body)
                {
                    ContentTypes = { "application/json" }
                };
            };
        });

        return services;
    }
}
