using System.Text.Json.Serialization;
using System.Threading.RateLimiting;
using Carter;
using ChessTournaments.API.Infrastructure.OpenApi;
using ChessTournaments.API.Models;
using ChessTournaments.Modules.Matches.API;
using ChessTournaments.Modules.Players.API;
using ChessTournaments.Modules.TournamentRequests;
using ChessTournaments.Modules.Tournaments;
using ChessTournaments.Shared.IntegrationEvents;
using ChessTournaments.Shared.IntegrationEvents.Extensions;
using Microsoft.AspNetCore.RateLimiting;
using OpenIddict.Abstractions;
using OpenIddict.Validation.AspNetCore;

namespace ChessTournaments.API.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        // Configure settings
        services.Configure<OidcSettings>(configuration.GetSection("Oidc"));

        // Add core services
        services
            .AddControllers()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            });

        // Configure JSON serialization for minimal APIs
        services.ConfigureHttpJsonOptions(options =>
        {
            options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
        });

        // Add Carter for endpoint discovery
        services.AddCarter();

        return services;
    }

    public static IServiceCollection AddModules(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        // Each module registers its own integration event publisher using OutboxIntegrationEventPublisher
        // This ensures reliable messaging with the Outbox/Inbox pattern across all modules

        services.AddTournamentsModule(configuration);
        services.AddTournamentRequestsModule(configuration);
        services.AddPlayersApiModule(configuration);
        services.AddMatchesModule(configuration);

        // Register the ModuleAwareInboxPublisher as the application-wide INotificationPublisher
        // This routes integration events to the correct module's DbContext for Inbox storage
        services.AddModuleAwareInboxPublisher();

        return services;
    }

    public static IServiceCollection AddCorsConfiguration(this IServiceCollection services)
    {
        services.AddCors(options =>
        {
            options.AddPolicy(
                "CorsPolicy",
                policy =>
                {
                    policy
                        .WithOrigins(
                            "http://localhost:4200",
                            "https://localhost:4200",
                            "https://orange-glacier-06e66a203.6.azurestaticapps.net",
                            "http://orange-glacier-06e66a203.6.azurestaticapps.net"
                        )
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                        .AllowCredentials();
                }
            );
        });

        return services;
    }

    public static IServiceCollection AddOpenIddictValidation(
        this IServiceCollection services,
        OidcSettings oidcSettings
    )
    {
        services
            .AddOpenIddict()
            .AddValidation(options =>
            {
                // Set the expected issuer (as it appears in tokens)
                var issuer = !string.IsNullOrEmpty(oidcSettings.Issuer)
                    ? oidcSettings.Issuer
                    : oidcSettings.Authority;
                options.SetIssuer(issuer);

                options.AddAudiences(oidcSettings.API.ClientId);

                // Configure introspection
                options
                    .UseIntrospection()
                    .SetClientId(oidcSettings.API.ClientId)
                    .SetClientSecret(oidcSettings.API.ClientSecret);

                // If Authority differs from Issuer, manually configure the server configuration
                // to point to the correct introspection endpoint
                if (
                    !string.IsNullOrEmpty(oidcSettings.Authority)
                    && oidcSettings.Authority != issuer
                )
                {
                    var authorityUri = new Uri(oidcSettings.Authority);
                    options.SetConfiguration(
                        new OpenIddictConfiguration
                        {
                            Issuer = new Uri(issuer),
                            IntrospectionEndpoint = new Uri(authorityUri, "connect/introspect"),
                        }
                    );
                }

                options.UseSystemNetHttp();
                options.UseAspNetCore();
            });

        services.AddAuthentication(OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme);
        services.AddAuthorization(options =>
        {
            // Default policy requires authentication
            options.FallbackPolicy = options.DefaultPolicy;

            // Admin policy requires Admin role
            options.AddPolicy("AdminPolicy", policy => policy.RequireRole("Admin"));

            // User policy requires authenticated user
            options.AddPolicy("UserPolicy", policy => policy.RequireAuthenticatedUser());
        });

        return services;
    }

    public static IServiceCollection AddApiDocumentation(this IServiceCollection services)
    {
        services.AddCustomizedOpenApi();

        return services;
    }

    public static IServiceCollection AddHealthChecksConfiguration(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        services
            .AddHealthChecks()
            .AddSqlServer(connectionString!, name: "database", tags: ["db", "sql", "sqlserver"]);

        return services;
    }

    public static IServiceCollection AddRateLimiterConfiguration(this IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            options.AddFixedWindowLimiter(
                "fixed",
                limiterOptions =>
                {
                    limiterOptions.PermitLimit = 100; // requests
                    limiterOptions.Window = TimeSpan.FromMinutes(1);
                    limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                    limiterOptions.QueueLimit = 0;
                }
            );

            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
        });

        return services;
    }
}
