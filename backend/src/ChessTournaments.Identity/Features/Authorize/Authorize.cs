using System.Collections.Immutable;
using System.Security.Claims;
using Carter;
using ChessTournaments.Identity.Database.Entities;
using ChessTournaments.Identity.Shared.Helpers;
using ChessTournaments.Identity.Shared.Infrastructure;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Microsoft.IdentityModel.Tokens;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace ChessTournaments.Identity.Features.Authorize;

public static class Authorize
{
    public record Command(HttpContext HttpContext) : IRequest<Result<IResult>>;

    internal sealed class Handler(
        IOpenIddictApplicationManager applicationManager,
        IOpenIddictAuthorizationManager authorizationManager,
        IOpenIddictScopeManager scopeManager,
        UserManager<ApplicationUser> userManager
    ) : IRequestHandler<Command, Result<IResult>>
    {
        public async Task<Result<IResult>> Handle(
            Command command,
            CancellationToken cancellationAuthorize
        )
        {
            var request =
                command.HttpContext.GetOpenIddictServerRequest()
                ?? throw new InvalidOperationException(
                    "The OpenID Connect request cannot be retrieved."
                );

            // Try to retrieve the user principal stored in the authentication cookie and redirect
            // the user agent to the login page (or to an external provider) in the following cases:
            //
            //  - If the user principal can't be extracted or the cookie is too old.
            //  - If prompt=login was specified by the client application.
            //  - If a max_age parameter was provided and the authentication cookie is not considered "fresh" enough.
            //
            // For scenarios where the default authentication handler configured in the ASP.NET Core
            // authentication options shouldn't be used, a specific scheme can be specified here.
            var result = await command.HttpContext.AuthenticateAsync();
            if (
                result is not { Succeeded: true }
                || request.HasPromptValue(PromptValues.Login)
                || (
                    request.MaxAge != null
                    && result.Properties?.IssuedUtc != null
                    && DateTimeOffset.UtcNow - result.Properties.IssuedUtc
                        > TimeSpan.FromSeconds(request.MaxAge.Value)
                )
            )
            {
                // If the client application requested promptless authentication,
                // return an error indicating that the user is not logged in.
                if (request.HasPromptValue(PromptValues.None))
                {
                    return Result.Success(
                        Results.Forbid(
                            new AuthenticationProperties(
                                new Dictionary<string, string>
                                {
                                    [OpenIddictServerAspNetCoreConstants.Properties.Error] =
                                        Errors.LoginRequired,
                                    [
                                        OpenIddictServerAspNetCoreConstants
                                            .Properties
                                            .ErrorDescription
                                    ] = "The user is not logged in.",
                                }!
                            ),
                            [OpenIddictServerAspNetCoreDefaults.AuthenticationScheme]
                        )
                    );
                }

                // To avoid endless login -> authorization redirects, the prompt=login flag
                // is removed from the authorization request payload before redirecting the user.
                var prompt = string.Join(" ", request.GetPromptValues().Remove(PromptValues.Login));

                var parameters = command.HttpContext.Request.HasFormContentType
                    ? command
                        .HttpContext.Request.Form.Where(parameter =>
                            parameter.Key != Parameters.Prompt
                        )
                        .ToList()
                    : command
                        .HttpContext.Request.Query.Where(parameter =>
                            parameter.Key != Parameters.Prompt
                        )
                        .ToList();

                parameters.Add(KeyValuePair.Create(Parameters.Prompt, new StringValues(prompt)));

                // For scenarios where the default challenge handler configured in the ASP.NET Core
                // authentication options shouldn't be used, a specific scheme can be specified here.
                return Result.Success(
                    Results.Challenge(
                        new AuthenticationProperties
                        {
                            RedirectUri =
                                command.HttpContext.Request.PathBase
                                + command.HttpContext.Request.Path
                                + QueryString.Create(parameters),
                        }
                    )
                );
            }

            // Retrieve the profile of the logged in user.
            var user =
                await userManager.GetUserAsync(result.Principal)
                ?? throw new InvalidOperationException("The user details cannot be retrieved.");

            // Retrieve the application details from the database.
            var application =
                await applicationManager.FindByClientIdAsync(
                    request.ClientId!,
                    cancellationAuthorize
                )
                ?? throw new InvalidOperationException(
                    "Details concerning the calling client application cannot be found."
                );

            // Retrieve the permanent authorizations associated with the user and the calling client application.
            var authorizations = await authorizationManager
                .FindAsync(
                    subject: await userManager.GetUserIdAsync(user),
                    client: await applicationManager.GetIdAsync(application),
                    status: Statuses.Valid,
                    type: AuthorizationTypes.Permanent,
                    scopes: request.GetScopes()
                )
                .ToListAsync();

            switch (
                await applicationManager.GetConsentTypeAsync(application, cancellationAuthorize)
            )
            {
                // If the consent is external (e.g when authorizations are granted by a sysadmin),
                // immediately return an error if no authorization can be found in the database.
                case ConsentTypes.External when authorizations.Count is 0:
                    return Result.Success(
                        Results.Forbid(
                            new AuthenticationProperties(
                                new Dictionary<string, string>
                                {
                                    [OpenIddictServerAspNetCoreConstants.Properties.Error] =
                                        Errors.ConsentRequired,
                                    [
                                        OpenIddictServerAspNetCoreConstants
                                            .Properties
                                            .ErrorDescription
                                    ] =
                                        "The logged in user is not allowed to access this client application.",
                                }!
                            ),
                            [OpenIddictServerAspNetCoreDefaults.AuthenticationScheme]
                        )
                    );

                // If the consent is implicit or if an authorization was found,
                // return an authorization response without displaying the consent form.
                case ConsentTypes.Implicit:
                case ConsentTypes.External when authorizations.Count is not 0:
                case ConsentTypes.Explicit
                    when authorizations.Count is not 0
                        && !request.HasPromptValue(PromptValues.Consent):
                    // Create the claims-based identity that will be used by OpenIddict to generate tokens.
                    var identity = new ClaimsIdentity(
                        authenticationType: TokenValidationParameters.DefaultAuthenticationType,
                        nameType: Claims.Name,
                        roleType: Claims.Role
                    );

                    // Add the claims that will be persisted in the tokens.
                    identity
                        .SetClaim(Claims.Subject, await userManager.GetUserIdAsync(user))
                        .SetClaim(Claims.Email, await userManager.GetEmailAsync(user))
                        .SetClaim(Claims.Name, await userManager.GetUserNameAsync(user))
                        .SetClaim(
                            Claims.PreferredUsername,
                            await userManager.GetUserNameAsync(user)
                        )
                        .SetClaims(
                            Claims.Role,
                            (await userManager.GetRolesAsync(user)).ToImmutableArray()
                        )
                        .SetClaims(
                            Claims.AuthenticationMethodReference,
                            result
                                .Principal.FindAll(Claims.AuthenticationMethodReference)
                                .Select(c => c.Value)
                                .ToImmutableArray()
                        );

                    // Note: in this sample, the granted scopes match the requested scope
                    // but you may want to allow the user to uncheck specific scopes.
                    // For that, simply restrict the list of scopes before calling SetScopes.
                    identity.SetScopes(request.GetScopes());
                    identity.SetResources(
                        await scopeManager.ListResourcesAsync(identity.GetScopes()).ToListAsync()
                    );

                    // Automatically create a permanent authorization to avoid requiring explicit consent
                    // for future authorization or token requests containing the same scopes.
                    var authorization = authorizations.LastOrDefault();
                    authorization ??= await authorizationManager.CreateAsync(
                        identity: identity,
                        subject: await userManager.GetUserIdAsync(user),
                        client: (
                            await applicationManager.GetIdAsync(application, cancellationAuthorize)
                        )!,
                        type: AuthorizationTypes.Permanent,
                        scopes: identity.GetScopes()
                    );

                    identity.SetAuthorizationId(
                        await authorizationManager.GetIdAsync(authorization)
                    );
                    identity.SetDestinations(GetDestinations);

                    return Result.Success(
                        Results.SignIn(
                            new ClaimsPrincipal(identity),
                            null,
                            OpenIddictServerAspNetCoreDefaults.AuthenticationScheme
                        )
                    );

                // At this point, no authorization was found in the database and an error must be returned
                // if the client application specified prompt=none in the authorization request.
                case ConsentTypes.Explicit when request.HasPromptValue(PromptValues.None):
                case ConsentTypes.Systematic when request.HasPromptValue(PromptValues.None):
                    return Result.Success(
                        Results.Forbid(
                            new AuthenticationProperties(
                                new Dictionary<string, string>
                                {
                                    [OpenIddictServerAspNetCoreConstants.Properties.Error] =
                                        Errors.ConsentRequired,
                                    [
                                        OpenIddictServerAspNetCoreConstants
                                            .Properties
                                            .ErrorDescription
                                    ] = "Interactive user consent is required.",
                                }!
                            ),
                            [OpenIddictServerAspNetCoreDefaults.AuthenticationScheme]
                        )
                    );

                default:
                    return await AcceptAsync(command);
            }
        }

        private static IEnumerable<string> GetDestinations(Claim claim)
        {
            switch (claim.Type)
            {
                case Claims.Name or Claims.PreferredUsername:
                    yield return Destinations.AccessToken;

                    if (claim.Subject != null && claim.Subject.HasScope(Scopes.Profile))
                        yield return Destinations.IdentityToken;

                    yield break;

                case Claims.Email:
                    yield return Destinations.AccessToken;

                    if (claim.Subject != null && claim.Subject.HasScope(Scopes.Email))
                        yield return Destinations.IdentityToken;

                    yield break;

                case Claims.Role:
                    yield return Destinations.AccessToken;

                    if (claim.Subject != null && claim.Subject.HasScope(Scopes.Roles))
                        yield return Destinations.IdentityToken;

                    yield break;

                case "AspNet.Identity.SecurityStamp":
                    yield break;

                default:
                    yield return Destinations.AccessToken;
                    yield break;
            }
        }

        private async Task<Result<IResult>> AcceptAsync(Command command)
        {
            var request =
                command.HttpContext.GetOpenIddictServerRequest()
                ?? throw new InvalidOperationException(
                    "The OpenID Connect request cannot be retrieved."
                );

            var user =
                await userManager.GetUserAsync(command.HttpContext.User)
                ?? throw new InvalidOperationException("The user details cannot be retrieved.");

            var application =
                await applicationManager.FindByClientIdAsync(request.ClientId!)
                ?? throw new InvalidOperationException(
                    "Details concerning the calling client application cannot be found."
                );

            var authorizations = await authorizationManager
                .FindAsync(
                    subject: await userManager.GetUserIdAsync(user),
                    client: await applicationManager.GetIdAsync(application),
                    status: Statuses.Valid,
                    type: AuthorizationTypes.Permanent,
                    scopes: request.GetScopes()
                )
                .ToListAsync();

            if (
                authorizations.Count is 0
                && await applicationManager.HasConsentTypeAsync(application, ConsentTypes.External)
            )
            {
                return Result.Success(
                    Results.Forbid(
                        new AuthenticationProperties(
                            new Dictionary<string, string>
                            {
                                [OpenIddictServerAspNetCoreConstants.Properties.Error] =
                                    Errors.ConsentRequired,
                                [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] =
                                    "The logged in user is not allowed to access this client application.",
                            }!
                        ),
                        [OpenIddictServerAspNetCoreDefaults.AuthenticationScheme]
                    )
                );
            }

            var identity = new ClaimsIdentity(
                authenticationType: TokenValidationParameters.DefaultAuthenticationType,
                nameType: Claims.Name,
                roleType: Claims.Role
            );

            identity
                .SetClaim(Claims.Subject, await userManager.GetUserIdAsync(user))
                .SetClaim(Claims.Email, await userManager.GetEmailAsync(user))
                .SetClaim(Claims.Name, await userManager.GetUserNameAsync(user))
                .SetClaim(Claims.PreferredUsername, await userManager.GetUserNameAsync(user))
                .SetClaims(Claims.Role, (await userManager.GetRolesAsync(user)).ToImmutableArray());

            identity.SetScopes(request.GetScopes());
            identity.SetResources(
                await scopeManager.ListResourcesAsync(identity.GetScopes()).ToListAsync()
            );

            var authorization = authorizations.LastOrDefault();
            authorization ??= await authorizationManager.CreateAsync(
                identity: identity,
                subject: await userManager.GetUserIdAsync(user),
                client: (await applicationManager.GetIdAsync(application))!,
                type: AuthorizationTypes.Permanent,
                scopes: identity.GetScopes()
            );

            identity.SetAuthorizationId(await authorizationManager.GetIdAsync(authorization));
            identity.SetDestinations(GetDestinations);

            return Result.Success(
                Results.SignIn(
                    new ClaimsPrincipal(identity),
                    authenticationScheme: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme
                )
            );
        }
    }

    public class AuthorizeEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet(
                "connect/authorize",
                [IgnoreAntiforgeryToken]
                async (HttpContext httpContext, ISender sender) =>
                {
                    var result = await sender.Send(new Authorize.Command(httpContext));

                    return result.IsFailure ? Results.BadRequest(result.Error) : result.Value;
                }
            );
        }
    }
}
