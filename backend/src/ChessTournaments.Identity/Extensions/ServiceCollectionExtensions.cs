using Carter;
using ChessTournaments.Identity.Database.Entities;
using ChessTournaments.Identity.Shared.Helpers;
using ChessTournaments.Identity.Shared.Infrastructure;
using FluentValidation;
using Microsoft.AspNetCore.Identity;

namespace ChessTournaments.Identity.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCorsConfiguration(this IServiceCollection services)
    {
        services.AddCors(options =>
        {
            options.AddPolicy(
                "CorsPolicy",
                policy =>
                {
                    policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
                }
            );
        });

        return services;
    }

    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<ISender, Sender>();
        services.AddRequestHandlers(typeof(Program).Assembly);
        services.AddValidatorsFromAssemblyContaining<Program>();
        services.AddCarter();
        services.AddHttpContextAccessor();
        services.AddHttpClient();

        return services;
    }

    public static IServiceCollection AddRazorComponentsConfiguration(
        this IServiceCollection services
    )
    {
        services.AddRazorComponents().AddInteractiveServerComponents();

        services.AddScoped<
            IUserClaimsPrincipalFactory<ApplicationUser>,
            AppClaimsPrincipalFactory
        >();

        return services;
    }
}
