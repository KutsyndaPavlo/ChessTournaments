using System.Text.Json;
using ChessTournaments.Identity.Configurations;
using ChessTournaments.Identity.Database;
using Microsoft.Extensions.Options;
using OpenIddict.Abstractions;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace ChessTournaments.Identity.Services;

public class OpenIddictWorker : IHostedService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly OidcSettings _oidcSettings;

    public OpenIddictWorker(IServiceProvider serviceProvider, IOptions<OidcSettings> oidcOptions)
    {
        _serviceProvider = serviceProvider;
        _oidcSettings = oidcOptions.Value;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();

        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await context.Database.EnsureCreatedAsync(cancellationToken);

        await RegisterScopesAsync(scope.ServiceProvider, _oidcSettings);
        await RegisterApplicationsAsync(scope.ServiceProvider, _oidcSettings);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    private static async Task RegisterScopesAsync(IServiceProvider provider, OidcSettings settings)
    {
        var manager = provider.GetRequiredService<IOpenIddictScopeManager>();

        foreach (var scope in settings.CustomScopes)
        {
            if (await manager.FindByNameAsync(scope.Name) is null)
            {
                await manager.CreateAsync(
                    new OpenIddictScopeDescriptor
                    {
                        DisplayName = scope.DisplayName,
                        Name = scope.Name,
                        Resources = { settings.API.ClientId },
                    }
                );
            }
        }
    }

    private static async Task RegisterApplicationsAsync(
        IServiceProvider provider,
        OidcSettings settings
    )
    {
        var manager = provider.GetRequiredService<IOpenIddictApplicationManager>();

        // Register API Resource Client (Confidential)
        if (await manager.FindByClientIdAsync(settings.API.ClientId) is null)
        {
            var descriptor = new OpenIddictApplicationDescriptor
            {
                ClientId = settings.API.ClientId,
                ClientSecret = settings.API.ClientSecret,
                ClientType = ClientTypes.Confidential,
                Permissions = { Permissions.Endpoints.Introspection },
            };

            await manager.CreateAsync(descriptor);
        }

        // Register Angular SPA Client (Public SPA)
        if (await manager.FindByClientIdAsync(settings.SPA.ClientId) is null)
        {
            var descriptor = new OpenIddictApplicationDescriptor
            {
                ClientId = settings.SPA.ClientId,
                ConsentType = ConsentTypes.Explicit,
                DisplayName = settings.SPA.DisplayName,
                ClientType = ClientTypes.Public,
                Permissions =
                {
                    Permissions.Endpoints.Authorization,
                    Permissions.Endpoints.EndSession,
                    Permissions.Endpoints.Token,
                    Permissions.GrantTypes.AuthorizationCode,
                    Permissions.GrantTypes.RefreshToken,
                    Permissions.ResponseTypes.Code,
                    Permissions.Scopes.Email,
                    Permissions.Scopes.Profile,
                    Permissions.Scopes.Roles,
                    Permissions.Prefixes.Scope + "offline_access",
                },
                Requirements = { Requirements.Features.ProofKeyForCodeExchange },
            };

            // Add redirect URIs
            foreach (var uri in settings.SPA.LoginCallbackUris)
            {
                descriptor.RedirectUris.Add(new Uri(uri));
            }

            // Add post-logout redirect URIs
            foreach (var uri in settings.SPA.LogoutCallbackUris)
            {
                descriptor.PostLogoutRedirectUris.Add(new Uri(uri));
            }

            // Add custom scopes
            foreach (var scope in settings.CustomScopes)
            {
                descriptor.Permissions.Add(Permissions.Prefixes.Scope + scope.Name);
            }

            await manager.CreateAsync(descriptor);
        }
    }
}
