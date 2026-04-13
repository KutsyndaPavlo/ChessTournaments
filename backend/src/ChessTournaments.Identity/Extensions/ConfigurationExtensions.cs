using ChessTournaments.Identity.Configurations;

namespace ChessTournaments.Identity.Extensions;

public static class ConfigurationExtensions
{
    public static IServiceCollection AddApplicationSettings(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        services.Configure<AccountSettings>(configuration.GetSection("Account"));
        services
            .AddOptions<ExternalLoginMicrosoftSettings>()
            .Bind(configuration.GetSection("ExternalLoginMicrosoft"));
        services.AddOptions<AppSettings>().Bind(configuration.GetSection("AppSettings"));

        return services;
    }
}
