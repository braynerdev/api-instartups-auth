using Api.Instartups.Auth.Configurations.Database;
using Api.Instartups.Auth.Models;
using Api.Instartups.Auth.src.UseCases.User.RegisterUserCommand;
using JasperFx;
using JasperFx.CodeGeneration;
using Microsoft.AspNetCore.Identity;
using Wolverine;
using Wolverine.FluentValidation;

namespace Api.Instartups.Auth.Configurations;

public static class WolverineConf
{
    public static WebApplicationBuilder AddWolverineConf(this WebApplicationBuilder builder)
    {
        builder.Host.UseWolverine(opt =>
        {
            opt.Durability.Mode = DurabilityMode.MediatorOnly;

            opt.Discovery.IncludeAssembly(typeof(RegisterUserCommand).Assembly);
            opt.UseFluentValidation();

            opt.CodeGeneration.AlwaysUseServiceLocationFor<UserManager<ApplicationUser>>();
            opt.CodeGeneration.AlwaysUseServiceLocationFor<AppDbContext>();

            opt.Policies.MessageExecutionLogLevel(LogLevel.None);
            opt.Policies.MessageSuccessLogLevel(LogLevel.None);

            opt.Services.CritterStackDefaults(x =>
            {
                x.Production.GeneratedCodeMode = TypeLoadMode.Static;
                x.Production.AssertAllPreGeneratedTypesExist = true;

                x.Development.GeneratedCodeMode = TypeLoadMode.Dynamic;
            });
        });

        return builder;
    }
}