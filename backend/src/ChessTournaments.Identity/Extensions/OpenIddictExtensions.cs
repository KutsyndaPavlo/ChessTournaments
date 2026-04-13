using ChessTournaments.Identity.Configurations;
using ChessTournaments.Identity.Database;
using ChessTournaments.Identity.Shared.Helpers;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace ChessTournaments.Identity.Extensions;

public static class OpenIddictExtensions
{
    public static IServiceCollection AddOpenIddictConfiguration(
        this IServiceCollection services,
        OidcSettings oidcSettings,
        bool isProductionEnvironment,
        IConfigurationManager configuration
    )
    {
        services
            .AddOpenIddict()
            .AddCore(options =>
            {
                options.UseEntityFrameworkCore().UseDbContext<ApplicationDbContext>();
            })
            .AddServer(options =>
            {
                // Set explicit issuer to ensure consistency across different access methods
                if (!string.IsNullOrEmpty(oidcSettings.Issuer))
                {
                    options.SetIssuer(new Uri(oidcSettings.Issuer));
                }

                options
                    .SetAuthorizationEndpointUris("connect/authorize")
                    .SetEndSessionEndpointUris("account/logout")
                    .SetIntrospectionEndpointUris("connect/introspect")
                    .SetTokenEndpointUris("connect/token")
                    .SetUserInfoEndpointUris("connect/userinfo")
                    .SetEndUserVerificationEndpointUris("connect/verify");

                options.RegisterScopes(
                    Scopes.Email,
                    Scopes.Profile,
                    Scopes.Roles,
                    Scopes.OfflineAccess
                );

                options
                    .AllowAuthorizationCodeFlow()
                    .RequireProofKeyForCodeExchange()
                    .AllowRefreshTokenFlow();

                options.SetAccessTokenLifetime(
                    TimeSpan.FromHours(oidcSettings.AccessTokenLifetimeHours)
                );
                options.SetRefreshTokenLifetime(
                    TimeSpan.FromHours(oidcSettings.RefreshTokenLifetimeHours)
                );

                if (isProductionEnvironment)
                {
                    options.AddSigningCertificate(
                        options.GetCertificateFromKeyVault(
                            configuration,
                            "Identity:Certificates:SigningCertificate"
                        )
                    );

                    options.AddEncryptionCertificate(
                        options.GetCertificateFromKeyVault(
                            configuration,
                            "Identity:Certificates:EncryptionCertificate"
                        )
                    );
                }
                else
                {
                    // Use ephemeral (in-memory) keys for non-Production environments
                    // This works in Azure without file system write access
                    options.AddEphemeralEncryptionKey().AddEphemeralSigningKey();
                    //options
                    //    .AddDevelopmentEncryptionCertificate()
                    //    .AddDevelopmentSigningCertificate();
                }

                var aspNetCoreBuilder = options
                    .UseAspNetCore()
                    .EnableAuthorizationEndpointPassthrough()
                    .EnableEndSessionEndpointPassthrough()
                    .EnableUserInfoEndpointPassthrough();

                if (!isProductionEnvironment)
                {
                    aspNetCoreBuilder.DisableTransportSecurityRequirement();
                }
            });

        return services;
    }
}
