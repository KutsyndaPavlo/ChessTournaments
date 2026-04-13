using ChessTournaments.Identity.Configurations;
using ChessTournaments.Identity.Database;
using ChessTournaments.Identity.Database.Entities;
using ChessTournaments.Identity.Shared.Helpers;
using Microsoft.AspNetCore.Identity;

namespace ChessTournaments.Identity.Extensions;

public static class IdentityExtensions
{
    public static IServiceCollection AddIdentityConfiguration(
        this IServiceCollection services,
        IdentitySettings identitySettings,
        AccountSettings accountSettings
    )
    {
        services
            .AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromHours(
                    identitySettings.DefaultLockoutTimeSpanInHours
                );
                options.Lockout.MaxFailedAccessAttempts = identitySettings.MaxFailedPasswordCount;
                options.Lockout.AllowedForNewUsers = true;

                options.User.RequireUniqueEmail = true;
            })
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

        services.ConfigureApplicationCookie(options =>
        {
            options.LoginPath = "/Account/Login";
            options.SlidingExpiration = true;
            options.ExpireTimeSpan = TimeSpan.FromDays(accountSettings.RememberMeLoginDurationDays);
        });

        return services;
    }

    public static IServiceCollection AddExternalAuthentication(
        this IServiceCollection services,
        ExternalLoginMicrosoftSettings externalLoginMicrosoftSettings
    )
    {
        if (externalLoginMicrosoftSettings.Enabled)
        {
            var authBuilder = services.AddAuthentication();

            if (externalLoginMicrosoftSettings.IsSingleTenant)
            {
                authBuilder.AddMicrosoftSingleTenantExternalLogin(externalLoginMicrosoftSettings);
            }
            else
            {
                authBuilder.AddMicrosoftMultitenantExternalLogin(externalLoginMicrosoftSettings);
            }
        }
        else
        {
            services.AddAuthentication();
        }

        return services;
    }
}
