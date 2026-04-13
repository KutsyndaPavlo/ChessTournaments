using Carter;
using ChessTournaments.Identity.Configurations;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace ChessTournaments.Identity.Features.External;

public class ChallengeEndpoint : ICarterModule
{
    private const string ExternalCallbackEndpoint = "/external/callback";
    private const string ReturnQueryParameter = "returnUrl=";
    private const string RootUrl = "~/";

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet(
            "external/challenge",
            [AllowAnonymous]
            (
                HttpContext httpContext,
                string scheme,
                IOptions<ExternalLoginMicrosoftSettings> externalLoginMicrosoftSettings
            ) =>
            {
                var queryString = httpContext.Request.QueryString.ToString();
                var returnUrl = queryString[
                    queryString.IndexOf(ReturnQueryParameter, StringComparison.OrdinalIgnoreCase)..
                ]
                    .Replace(ReturnQueryParameter, string.Empty);

                if (string.IsNullOrEmpty(returnUrl))
                {
                    returnUrl = RootUrl;
                }

                var props = new AuthenticationProperties
                {
                    RedirectUri = ExternalCallbackEndpoint,
                    Items = { { "returnUrl", returnUrl }, { "scheme", scheme } },
                };

                return Results.Challenge(props, [scheme]);
            }
        );
    }
}
