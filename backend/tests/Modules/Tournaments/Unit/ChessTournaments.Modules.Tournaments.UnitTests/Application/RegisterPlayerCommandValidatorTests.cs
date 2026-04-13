using ChessTournaments.Modules.Tournaments.Application.Features.RegisterPlayer;
using FluentValidation.TestHelper;

namespace ChessTournaments.Modules.Tournaments.UnitTests.Application;

public class RegisterPlayerCommandValidatorTests
{
    private RegisterPlayerCommandValidator _validator = null!;

    [Before(Test)]
    public void Setup()
    {
        _validator = new RegisterPlayerCommandValidator();
    }

    [Test]
    public void Should_Have_Error_When_TournamentId_Is_Empty()
    {
        // Arrange
        var command = CreateValidCommand() with
        {
            TournamentId = Guid.Empty,
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.TournamentId);
    }

    [Test]
    public void Should_Have_Error_When_PlayerId_Is_Empty()
    {
        // Arrange
        var command = CreateValidCommand() with
        {
            PlayerId = string.Empty,
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.PlayerId);
    }

    [Test]
    public void Should_Have_Error_When_PlayerName_Is_Empty()
    {
        // Arrange
        var command = CreateValidCommand() with
        {
            PlayerName = string.Empty,
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.PlayerName);
    }

    [Test]
    public void Should_Have_Error_When_PlayerName_Is_Null()
    {
        // Arrange
        var command = CreateValidCommand() with
        {
            PlayerName = null!,
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.PlayerName);
    }

    [Test]
    public void Should_Have_Error_When_PlayerName_Exceeds_MaxLength()
    {
        // Arrange
        var command = CreateValidCommand() with
        {
            PlayerName = new string('a', 201),
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.PlayerName);
    }

    [Test]
    public void Should_Have_Error_When_Rating_Is_Negative()
    {
        // Arrange
        var command = CreateValidCommand() with
        {
            Rating = -1,
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Rating);
    }

    [Test]
    public void Should_Not_Have_Error_When_Rating_Is_Zero()
    {
        // Arrange
        var command = CreateValidCommand() with
        {
            Rating = 0,
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Rating);
    }

    [Test]
    public void Should_Not_Have_Error_When_Rating_Is_Positive()
    {
        // Arrange
        var command = CreateValidCommand() with
        {
            Rating = 1500,
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Rating);
    }

    [Test]
    public void Should_Not_Have_Error_When_Rating_Is_Null()
    {
        // Arrange
        var command = CreateValidCommand() with
        {
            Rating = null,
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Rating);
    }

    [Test]
    public void Should_Not_Have_Errors_When_Command_Is_Valid()
    {
        // Arrange
        var command = CreateValidCommand();

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    private static RegisterPlayerCommand CreateValidCommand()
    {
        return new RegisterPlayerCommand(
            TournamentId: Guid.NewGuid(),
            PlayerId: Guid.NewGuid().ToString(),
            PlayerName: "John Doe",
            Rating: 1500
        );
    }
}
