using System.Security.Claims;
using Carter;
using ChessTournaments.Identity.Configurations;
using ChessTournaments.Identity.Database.Entities;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace ChessTournaments.Identity.Features.External;

public class CallbackEndpoint : ICarterModule
{
    private const string RootUrl = "~/";
    private const string LoginUrl = "/Account/Login";
    private const string EmailClaim = "emailaddress";
    private const string PreferredUserNameClaim = "preferred_username";
    private const string ReturnUrlProperty = "returnUrl";
    private const string SsoAccountClaimType = "ssoAccount";
    private const string MicrosoftClaimValue = "Microsoft";
    private const string MicrosoftExternalSchema = "Microsoft";

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet(
            "external/callback",
            [AllowAnonymous]
            async (
                HttpContext httpContext,
                ILogger<CallbackEndpoint> logger,
                IOptions<AppSettings> appSettings,
                IOptions<ExternalLoginMicrosoftSettings> externalLoginMicrosoftSettings,
                SignInManager<ApplicationUser> signInManager,
                UserManager<ApplicationUser> userManager
            ) =>
            {
                // read external identity from the temporary cookie
                var schema = externalLoginMicrosoftSettings.Value.IsSingleTenant
                    ? MicrosoftExternalSchema
                    : IdentityConstants.ExternalScheme;
                var result = await httpContext.AuthenticateAsync(schema);

                if (result.Succeeded != true || result.Principal.Identity?.IsAuthenticated != true)
                {
                    return Results.Redirect($"{LoginUrl}?elError=External authentication error");
                }

                logger.LogDebug(
                    "External claims: {@claims}",
                    result.Principal?.Claims.Select(c => $"{c.Type}: {c.Value}") ?? []
                );
                var returnUrl = GetReturnUrl(result);
                var email = GetUserEmail(
                    result,
                    externalLoginMicrosoftSettings.Value.IsSingleTenant
                );

                // delete temporary cookie used during external authentication
                await httpContext.SignOutAsync(IdentityConstants.ExternalScheme);

                if (!string.IsNullOrEmpty(email))
                {
                    var user = await userManager.FindByEmailAsync(email);
                    if (user != null)
                    {
                        await signInManager.SignInWithClaimsAsync(
                            user,
                            true,
                            [new Claim(SsoAccountClaimType, MicrosoftClaimValue)]
                        );
                    }
                    else
                    {
                        return Results.Redirect(
                            $"{returnUrl}&elError=User with email {email} is not setup"
                        );
                    }
                }
                else
                {
                    return Results.Redirect(
                        $"{returnUrl}&elError=External Login has not been configured for the user"
                    );
                }

                if (
                    (
                        string.IsNullOrWhiteSpace(returnUrl)
                        || returnUrl.Equals(RootUrl, StringComparison.OrdinalIgnoreCase)
                    ) && !string.IsNullOrWhiteSpace(appSettings.Value.AssetsUrl)
                )
                {
                    returnUrl = appSettings.Value.AssetsUrl;
                }

                return Results.Redirect(returnUrl);
            }
        );
    }

    private static string? GetUserEmail(AuthenticateResult? result, bool isSingleTenant) =>
        result
            ?.Principal?.Claims.FirstOrDefault(x =>
                x.Type.Contains(isSingleTenant ? PreferredUserNameClaim : EmailClaim)
            )
            ?.Value;

    private static string GetReturnUrl(AuthenticateResult? result) =>
        result?.Properties?.Items[ReturnUrlProperty] ?? RootUrl;
}
