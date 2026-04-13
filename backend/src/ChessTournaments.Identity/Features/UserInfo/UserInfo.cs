using Carter;
using ChessTournaments.Identity.Database.Entities;
using ChessTournaments.Identity.Shared.Infrastructure;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OpenIddict.Server.AspNetCore;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace ChessTournaments.Identity.Features.UserInfo;

public static class UserInfo
{
    public record Command(string Username) : IRequest<Result<Dictionary<string, string>>>;

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Username).NotEmpty();
        }
    }

    internal sealed class Handler(
        IValidator<Command> validator,
        UserManager<ApplicationUser> userManager,
        ILogger<Handler> logger
    ) : IRequestHandler<Command, Result<Dictionary<string, string>>>
    {
        public async Task<Result<Dictionary<string, string>>> Handle(
            Command request,
            CancellationToken cancellationToken
        )
        {
            logger.LogDebug(
                "Processing userinfo request for a user: {Username}.",
                request.Username
            );

            var validationResult = await validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                logger.LogDebug("Get token request failed due to invalid parameter.");
                return Result.Failure<Dictionary<string, string>>(
                    new Error("UserInfo.Validation", validationResult.ToString())
                );
            }

            var result = await userManager.FindByNameAsync(request.Username);

            if (result == null)
            {
                logger.LogDebug(
                    "Get token request failed due to missing user: {user}",
                    request.Username
                );

                return Result.Failure<Dictionary<string, string>>(
                    new Error("UserInfo.NotFound", validationResult.ToString())
                );
            }

            var claims = new Dictionary<string, string>()
            {
                [Claims.Subject] = result.Id,
                [Claims.Username] = result.UserName ?? string.Empty,
                [Claims.Email] = result.Email ?? string.Empty,
            };

            logger.LogDebug(
                "Successfully processed get user info request for user: {Username}.",
                request.Username
            );
            return Result.Success(claims);
        }
    }
}

public class UserInfoEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet(
            "connect/userinfo",
            [IgnoreAntiforgeryToken]
            [Authorize(
                AuthenticationSchemes = OpenIddictServerAspNetCoreDefaults.AuthenticationScheme
            )]
            async (HttpContext httpContext, ISender sender) =>
            {
                var result = await sender.Send(
                    new UserInfo.Command(httpContext.User.Identity!.Name!)
                );

                return result.IsFailure
                    ? Results.BadRequest(result.Error)
                    : Results.Ok(result.Value);
            }
        );
    }
}
