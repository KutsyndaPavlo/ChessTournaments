using ChessTournaments.Modules.Tournaments.Application.Features.UpdateTournament;
using FluentValidation.TestHelper;

namespace ChessTournaments.Modules.Tournaments.UnitTests.Application;

public class UpdateTournamentCommandValidatorTests
{
    private UpdateTournamentCommandValidator _validator = null!;

    [Before(Test)]
    public void Setup()
    {
        _validator = new UpdateTournamentCommandValidator();
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
    public void Should_Have_Error_When_Name_Is_Empty()
    {
        // Arrange
        var command = CreateValidCommand() with
        {
            Name = string.Empty,
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Test]
    public void Should_Have_Error_When_Name_Is_Null()
    {
        // Arrange
        var command = CreateValidCommand() with
        {
            Name = null!,
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Test]
    public void Should_Have_Error_When_Name_Exceeds_MaxLength()
    {
        // Arrange
        var command = CreateValidCommand() with
        {
            Name = new string('a', 201),
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Test]
    public void Should_Have_Error_When_Description_Exceeds_MaxLength()
    {
        // Arrange
        var command = CreateValidCommand() with
        {
            Description = new string('a', 2001),
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Description);
    }

    [Test]
    public void Should_Not_Have_Error_When_Description_Is_Null()
    {
        // Arrange
        var command = CreateValidCommand() with
        {
            Description = null,
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Description);
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

    [Test]
    public void Should_Not_Have_Errors_When_Description_Is_Within_Limit()
    {
        // Arrange
        var command = CreateValidCommand() with
        {
            Description = new string('a', 2000),
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Description);
    }

    private static UpdateTournamentCommand CreateValidCommand()
    {
        return new UpdateTournamentCommand(
            TournamentId: Guid.NewGuid(),
            Name: "Updated Tournament",
            Description: "Updated Description",
            Location: "Updated Location"
        );
    }
}
