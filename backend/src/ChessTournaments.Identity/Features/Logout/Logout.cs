using Carter;
using ChessTournaments.Identity.Database.Entities;
using ChessTournaments.Identity.Shared.Infrastructure;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;

namespace ChessTournaments.Identity.Features.Logout;

public static class Logout
{
    public record Command(string? PostLogoutRedirectUri, string? CurrentClientId)
        : IRequest<Result<IResult>>;

    internal sealed class Handler(
        SignInManager<ApplicationUser> signInManager,
        ILogger<Handler> logger
    ) : IRequestHandler<Command, Result<IResult>>
    {
        public async Task<Result<IResult>> Handle(
            Command request,
            CancellationToken cancellationToken
        )
        {
            logger.LogDebug(
                "Processing logout request for client: {ClientId}",
                request.CurrentClientId ?? "Unknown"
            );

            await signInManager.SignOutAsync();

            if (!string.IsNullOrEmpty(request.PostLogoutRedirectUri))
            {
                return Result.Success(
                    Results.SignOut(
                        properties: new AuthenticationProperties
                        {
                            RedirectUri = request.PostLogoutRedirectUri,
                        },
                        authenticationSchemes:
                        [
                            OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                        ]
                    )
                );
            }

            logger.LogDebug("Direct logout - redirecting to home page.");
            return Result.Success(Results.Redirect("/"));
        }
    }

    public class LogoutEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet(
                "account/logout",
                [IgnoreAntiforgeryToken]
                [Authorize(
                    AuthenticationSchemes = OpenIddictServerAspNetCoreDefaults.AuthenticationScheme
                )]
                async (ISender sender, HttpContext httpContext, ILogger<LogoutEndpoint> logger) =>
                {
                    var oidcContext = httpContext.GetOpenIddictServerRequest();

                    var currentClientId = await IdentifyClientAsync(
                        httpContext,
                        oidcContext,
                        logger
                    );

                    var result = await sender.Send(
                        new Command(oidcContext?.PostLogoutRedirectUri, currentClientId)
                    );

                    return result.IsFailure ? Results.BadRequest(result.Error) : result.Value;
                }
            );
        }

        private static async Task<string?> IdentifyClientAsync(
            HttpContext httpContext,
            OpenIddictRequest? oidcRequest,
            ILogger logger
        )
        {
            var authenticateResult = await httpContext.AuthenticateAsync(
                OpenIddictServerAspNetCoreDefaults.AuthenticationScheme
            );

            if (authenticateResult is { Succeeded: true, Principal: not null })
            {
                var clientId = authenticateResult.Principal.GetAudiences().FirstOrDefault();

                if (!string.IsNullOrEmpty(clientId))
                {
                    logger.LogDebug("Identified client {ClientId} from id_token_hint", clientId);
                    return clientId;
                }
            }

            if (!string.IsNullOrEmpty(oidcRequest?.ClientId))
            {
                logger.LogDebug(
                    "Identified client {ClientId} from OIDC request",
                    oidcRequest.ClientId
                );
                return oidcRequest.ClientId;
            }

            logger.LogWarning(
                "Could not identify client for logout. All non-excluded clients will be logged out"
            );
            return null;
        }
    }
}
