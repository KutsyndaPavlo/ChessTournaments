using ChessTournaments.Identity.Configurations;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.Extensions.Primitives;
using Microsoft.Identity.Web;

namespace ChessTournaments.Identity.Shared.Helpers;

public static class AuthenticationBuilderExtensions
{
    private const string MicrosoftSchema = "Microsoft";
    private const string Instance = "https://login.microsoftonline.com/";
    private const string Domain = ".onmicrosoft.com";

    public static AuthenticationBuilder AddMicrosoftMultitenantExternalLogin(
        this AuthenticationBuilder builder,
        ExternalLoginMicrosoftSettings externalLoginMicrosoftSettings
    )
    {
        ArgumentNullException.ThrowIfNull(externalLoginMicrosoftSettings.ClientId);
        ArgumentNullException.ThrowIfNull(externalLoginMicrosoftSettings.ClientSecret);

        return builder.AddMicrosoftAccount(options =>
        {
            options.ClientId = externalLoginMicrosoftSettings.ClientId;
            options.ClientSecret = externalLoginMicrosoftSettings.ClientSecret;
            options.Scope.Add("https://graph.microsoft.com/User.Read");

            var clientsRequiringPrompt = externalLoginMicrosoftSettings.ClientIdsRequiringPrompt;

            if ((clientsRequiringPrompt?.Length ?? 0) > 0)
            {
                options.Events = new OAuthEvents
                {
                    OnRedirectToAuthorizationEndpoint = async context =>
                    {
                        var originalRedirectUri = context.RedirectUri;
                        var updatedRedirectUri = new UriBuilder(originalRedirectUri);
                        var authQuery = Microsoft.AspNetCore.WebUtilities.QueryHelpers.ParseQuery(
                            updatedRedirectUri.Query
                        );
                        var newQuery = new Dictionary<string, string>(authQuery.Count);

                        foreach (
                            var kvp in authQuery.Where(kvp => !string.IsNullOrWhiteSpace(kvp.Value))
                        )
                        {
                            newQuery[kvp.Key] = kvp.Value!;
                        }

                        var shouldAddPrompt = false;

                        context.Properties.Items.TryGetValue("returnUrl", out var returnUrl);
                        if (!string.IsNullOrEmpty(returnUrl))
                        {
                            if (returnUrl.StartsWith("~"))
                            {
                                returnUrl = returnUrl.TrimStart('~');
                            }

                            var returnUri = new Uri(returnUrl, UriKind.RelativeOrAbsolute);

                            var queryString = returnUri.IsAbsoluteUri
                                ? returnUri.Query
                                : new Uri("http://placeholder.com" + returnUrl).Query;

                            var returnQuery =
                                Microsoft.AspNetCore.WebUtilities.QueryHelpers.ParseQuery(
                                    queryString
                                );

                            if (returnQuery.TryGetValue("client_id", out StringValues clientId))
                            {
                                if (
                                    clientsRequiringPrompt != null
                                    && clientsRequiringPrompt.Contains(clientId.ToString())
                                )
                                {
                                    shouldAddPrompt = true;
                                }
                            }
                        }

                        if (shouldAddPrompt)
                        {
                            newQuery["prompt"] = "login";
                            updatedRedirectUri.Query = string.Join(
                                "&",
                                newQuery.Select(kvp => $"{kvp.Key}={kvp.Value}")
                            );
                            context.Response.Redirect(updatedRedirectUri.ToString());
                        }
                        else
                        {
                            context.Response.Redirect(originalRedirectUri);
                        }

                        await Task.CompletedTask;
                    },
                };
            }
        });
    }

    public static MicrosoftIdentityWebAppAuthenticationBuilder AddMicrosoftSingleTenantExternalLogin(
        this AuthenticationBuilder builder,
        ExternalLoginMicrosoftSettings externalLoginMicrosoftSettings
    )
    {
        ArgumentNullException.ThrowIfNull(externalLoginMicrosoftSettings.ClientId);
        ArgumentNullException.ThrowIfNull(externalLoginMicrosoftSettings.ClientSecret);

        return builder.AddMicrosoftIdentityWebApp(
            options =>
            {
                options.Instance = Instance;
                options.Domain = $"{externalLoginMicrosoftSettings.TenantId}{Domain}";
                options.TenantId = externalLoginMicrosoftSettings.TenantId;
                options.ClientId = externalLoginMicrosoftSettings.ClientId;
                options.ClientSecret = externalLoginMicrosoftSettings.ClientSecret;
                options.CallbackPath = "/signin-microsoft";

                options.Scope.Add("https://graph.microsoft.com/User.Read");

                var clientsRequiringPrompt =
                    externalLoginMicrosoftSettings.ClientIdsRequiringPrompt;

                if ((clientsRequiringPrompt?.Length ?? 0) > 0)
                {
                    options.Events =
                        new Microsoft.AspNetCore.Authentication.OpenIdConnect.OpenIdConnectEvents
                        {
                            OnRedirectToIdentityProvider = context =>
                            {
                                bool shouldAddPrompt = false;

                                // Try to read "returnUrl" from authentication properties
                                if (
                                    context.Properties.Items.TryGetValue(
                                        "returnUrl",
                                        out var returnUrl
                                    ) && !string.IsNullOrEmpty(returnUrl)
                                )
                                {
                                    if (returnUrl.StartsWith("~"))
                                    {
                                        returnUrl = returnUrl.TrimStart('~');
                                    }

                                    // Parse the query string from the return URL
                                    var returnUri = new Uri(returnUrl, UriKind.RelativeOrAbsolute);
                                    var queryString = returnUri.IsAbsoluteUri
                                        ? returnUri.Query
                                        : new Uri("http://placeholder.com" + returnUrl).Query;

                                    var returnQuery =
                                        Microsoft.AspNetCore.WebUtilities.QueryHelpers.ParseQuery(
                                            queryString
                                        );

                                    if (returnQuery.TryGetValue("client_id", out var clientId))
                                    {
                                        if (
                                            clientsRequiringPrompt != null
                                            && clientsRequiringPrompt.Contains(clientId.ToString())
                                        )
                                        {
                                            shouldAddPrompt = true;
                                        }
                                    }
                                }

                                // Conditionally add `prompt=login`
                                if (shouldAddPrompt)
                                {
                                    context.ProtocolMessage.Prompt = "login";
                                }

                                return Task.CompletedTask;
                            },
                        };
                }
            },
            openIdConnectScheme: MicrosoftSchema
        );
    }
}
