using ChessTournaments.Modules.Tournaments.Application.Features.CreateTournament;
using ChessTournaments.Modules.Tournaments.Domain.Enums;
using ChessTournaments.Shared.Domain.Enums;
using FluentValidation.TestHelper;

namespace ChessTournaments.Modules.Tournaments.UnitTests.Application;

public class CreateTournamentCommandValidatorTests
{
    private CreateTournamentCommandValidator _validator = null!;

    [Before(Test)]
    public void Setup()
    {
        _validator = new CreateTournamentCommandValidator();
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
    public void Should_Have_Error_When_StartDate_Is_In_Past()
    {
        // Arrange
        var command = CreateValidCommand() with
        {
            StartDate = DateTime.UtcNow.AddDays(-2),
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.StartDate);
    }

    [Test]
    public void Should_Not_Have_Error_When_StartDate_Is_Today()
    {
        // Arrange
        var command = CreateValidCommand() with
        {
            StartDate = DateTime.UtcNow,
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.StartDate);
    }

    [Test]
    public void Should_Have_Error_When_OrganizerId_Is_Empty()
    {
        // Arrange
        var command = CreateValidCommand() with
        {
            OrganizerId = string.Empty,
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.OrganizerId);
    }

    [Test]
    [Arguments(0)]
    [Arguments(-1)]
    public void Should_Have_Error_When_TimeInMinutes_Is_Invalid(int timeInMinutes)
    {
        // Arrange
        var command = CreateValidCommand() with
        {
            TimeInMinutes = timeInMinutes,
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.TimeInMinutes);
    }

    [Test]
    public void Should_Have_Error_When_IncrementInSeconds_Is_Negative()
    {
        // Arrange
        var command = CreateValidCommand() with
        {
            IncrementInSeconds = -1,
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.IncrementInSeconds);
    }

    [Test]
    [Arguments(0)]
    [Arguments(-1)]
    [Arguments(21)]
    public void Should_Have_Error_When_NumberOfRounds_Is_Invalid(int numberOfRounds)
    {
        // Arrange
        var command = CreateValidCommand() with
        {
            NumberOfRounds = numberOfRounds,
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.NumberOfRounds);
    }

    [Test]
    [Arguments(1)]
    [Arguments(0)]
    [Arguments(-1)]
    [Arguments(1001)]
    public void Should_Have_Error_When_MaxPlayers_Is_Invalid(int maxPlayers)
    {
        // Arrange
        var command = CreateValidCommand() with
        {
            MaxPlayers = maxPlayers,
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.MaxPlayers);
    }

    [Test]
    [Arguments(1)]
    [Arguments(0)]
    [Arguments(-1)]
    public void Should_Have_Error_When_MinPlayers_Is_Invalid(int minPlayers)
    {
        // Arrange
        var command = CreateValidCommand() with
        {
            MinPlayers = minPlayers,
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.MinPlayers);
    }

    [Test]
    public void Should_Have_Error_When_MinPlayers_Exceeds_MaxPlayers()
    {
        // Arrange
        var command = CreateValidCommand() with
        {
            MinPlayers = 10,
            MaxPlayers = 5,
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.MinPlayers);
    }

    [Test]
    public void Should_Have_Error_When_EntryFee_Is_Negative()
    {
        // Arrange
        var command = CreateValidCommand() with
        {
            EntryFee = -1,
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.EntryFee);
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

    private static CreateTournamentCommand CreateValidCommand()
    {
        return new CreateTournamentCommand(
            Name: "Test Tournament",
            Description: "Test Description",
            StartDate: DateTime.UtcNow.AddDays(7),
            Location: "Test Location",
            OrganizerId: Guid.NewGuid().ToString(),
            Format: TournamentFormat.Swiss,
            TimeControl: TimeControl.Rapid,
            TimeInMinutes: 15,
            IncrementInSeconds: 10,
            NumberOfRounds: 5,
            MaxPlayers: 20,
            MinPlayers: 4,
            AllowByes: true,
            EntryFee: 0
        );
    }
}
