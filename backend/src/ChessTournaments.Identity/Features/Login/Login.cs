using Carter;
using ChessTournaments.Identity.Configurations;
using ChessTournaments.Identity.Database.Entities;
using ChessTournaments.Identity.Features.Login.Contracts;
using ChessTournaments.Identity.Shared.Infrastructure;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace ChessTournaments.Identity.Features.Login;

public static class Login
{
    public record Command(string Username, string Password, bool RememberMe)
        : IRequest<Result<LoginResponse>>;

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Username).NotEmpty().WithMessage("Username could not be empty");

            RuleFor(x => x.Password).NotEmpty().WithMessage("Password could not be empty");
        }
    }

    internal sealed class Handler(
        SignInManager<ApplicationUser> signInManager,
        UserManager<ApplicationUser> userManager,
        IValidator<Command> validator,
        ILogger<Handler> logger,
        IOptions<AccountSettings> accountOptions
    ) : IRequestHandler<Command, Result<LoginResponse>>
    {
        private readonly AccountSettings _accountSettings = accountOptions.Value;

        public async Task<Result<LoginResponse>> Handle(
            Command request,
            CancellationToken cancellationToken
        )
        {
            logger.LogDebug("Processing login request for user: {Username}.", request.Username);

            var validationResult = await validator.ValidateAsync(request, cancellationToken);

            if (!validationResult.IsValid)
                return Result.Failure<LoginResponse>(
                    Errors.ValidationError(validationResult.ToString())
                );

            var user = await userManager.FindByNameAsync(request.Username);

            var shouldPersist = _accountSettings.AllowRememberLogin && request.RememberMe;
            var signInResult = await signInManager.PasswordSignInAsync(
                request.Username,
                request.Password,
                shouldPersist,
                lockoutOnFailure: true
            );

            logger.LogDebug(
                signInResult.Succeeded
                    ? "Successfully processed login request."
                    : "Login request failed."
            );

            if (signInResult.Succeeded)
            {
                return Result.Success(
                    new LoginResponse
                    {
                        Status = LoginStatus.Succeeded,
                        Message = $"Login successful! Welcome  {request.Username}.",
                    }
                );
            }

            if (signInResult.IsLockedOut)
            {
                return Result.Failure<LoginResponse>(Errors.UserLocked);
            }

            if (signInResult.IsNotAllowed)
            {
                return Result.Failure<LoginResponse>(Errors.UserIsNotAllowed);
            }

            return Result.Failure<LoginResponse>(Errors.LoginFailed);
        }
    }

    public static class LoginConstants
    {
        public const string LoginValidationErrorCode = "Login.Validation";
        public const string LoginFailedErrorCode = "Login.Failed";
        public const string UserIsNotAllowedErrorCode = "Login.IsNotAllowed";
        public const string UserLockedErrorCode = "Login.UserLocked";

        public const string LoginFailedErrorMessage = "Invalid username or password.";
        public const string UserIsNotAllowedErrorMessage = "Login is not allowed for this account.";
        public const string UserLockedErrorMessage = "User Account is locked out.";
    }

    public static class Errors
    {
        public static Error ValidationError(string message) =>
            new(LoginConstants.LoginValidationErrorCode, message);

        public static readonly Error LoginFailed = new(
            LoginConstants.LoginFailedErrorCode,
            LoginConstants.LoginFailedErrorMessage
        );
        public static readonly Error UserIsNotAllowed = new(
            LoginConstants.UserIsNotAllowedErrorCode,
            LoginConstants.UserIsNotAllowedErrorMessage
        );
        public static readonly Error UserLocked = new(
            LoginConstants.UserLockedErrorCode,
            LoginConstants.UserLockedErrorMessage
        );
    }

    public class LoginEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost(
                "api/login",
                async (LoginRequest request, ISender sender) =>
                {
                    var command = new Command(
                        request.Username,
                        request.Password,
                        request.RememberMe
                    );
                    var result = await sender.Send(command);

                    return result.IsFailure
                        ? Results.BadRequest(result.Error)
                        : Results.Ok(result.Value);
                }
            );
        }
    }
}
