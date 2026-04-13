using Carter;
using ChessTournaments.Identity.Database.Entities;
using ChessTournaments.Identity.Features.Registration.Contracts;
using ChessTournaments.Identity.Shared.Infrastructure;
using FluentValidation;
using Microsoft.AspNetCore.Identity;

namespace ChessTournaments.Identity.Features.Registration;

public static class Register
{
    public sealed record Command(
        string Username,
        string Email,
        string Password,
        string ConfirmPassword,
        string? FirstName,
        string? LastName
    ) : IRequest<Result<RegisterResponse>>;

    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Username)
                .NotEmpty()
                .WithMessage("Username is required")
                .MinimumLength(3)
                .WithMessage("Username must be at least 3 characters")
                .MaximumLength(50)
                .WithMessage("Username cannot exceed 50 characters");

            RuleFor(x => x.Email)
                .NotEmpty()
                .WithMessage("Email is required")
                .EmailAddress()
                .WithMessage("Invalid email address");

            RuleFor(x => x.Password)
                .NotEmpty()
                .WithMessage("Password is required")
                .MinimumLength(8)
                .WithMessage("Password must be at least 8 characters");

            RuleFor(x => x.ConfirmPassword)
                .NotEmpty()
                .WithMessage("Password confirmation is required")
                .Equal(x => x.Password)
                .WithMessage("Passwords do not match");

            RuleFor(x => x.FirstName)
                .MaximumLength(50)
                .WithMessage("First name cannot exceed 50 characters")
                .When(x => !string.IsNullOrEmpty(x.FirstName));

            RuleFor(x => x.LastName)
                .MaximumLength(50)
                .WithMessage("Last name cannot exceed 50 characters")
                .When(x => !string.IsNullOrEmpty(x.LastName));
        }
    }

    internal sealed class Handler(
        UserManager<ApplicationUser> userManager,
        IValidator<Command> validator
    ) : IRequestHandler<Command, Result<RegisterResponse>>
    {
        public async Task<Result<RegisterResponse>> Handle(
            Command request,
            CancellationToken cancellationToken
        )
        {
            var validationResult = await validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors.Select(e => e.ErrorMessage);
                return Result.Success(RegisterResponse.ValidationError(errors));
            }

            var existingUser = await userManager.FindByNameAsync(request.Username);
            if (existingUser is not null)
            {
                return Result.Success(RegisterResponse.UserExists());
            }

            var existingEmail = await userManager.FindByEmailAsync(request.Email);
            if (existingEmail is not null)
            {
                return Result.Success(RegisterResponse.UserExists());
            }

            var newUser = new ApplicationUser
            {
                UserName = request.Username,
                Email = request.Email,
                EmailConfirmed = false,
                FirstName = request.FirstName,
                LastName = request.LastName,
            };

            var createResult = await userManager.CreateAsync(newUser, request.Password);
            if (!createResult.Succeeded)
            {
                var errors = createResult.Errors.Select(e => e.Description);
                return Result.Success(RegisterResponse.ValidationError(errors));
            }

            return Result.Success(RegisterResponse.Success());
        }
    }

    public sealed class RegisterEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost(
                    "api/register",
                    async (RegisterRequest request, ISender sender) =>
                    {
                        var command = new Command(
                            request.Username,
                            request.Email,
                            request.Password,
                            request.ConfirmPassword,
                            request.FirstName,
                            request.LastName
                        );

                        var result = await sender.Send(command);

                        return result.IsFailure
                            ? Results.BadRequest(result.Error)
                            : Results.Ok(result.Value);
                    }
                )
                .WithName("Register")
                .WithTags("Authentication")
                .Produces<RegisterResponse>(StatusCodes.Status200OK)
                .Produces<Error>(StatusCodes.Status400BadRequest);
        }
    }
}
