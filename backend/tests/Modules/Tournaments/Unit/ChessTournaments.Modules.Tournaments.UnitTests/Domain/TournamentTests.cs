using ChessTournaments.Modules.Tournaments.Domain.Enums;
using ChessTournaments.Modules.Tournaments.Domain.Tournaments;
using ChessTournaments.Shared.Domain.Enums;

namespace ChessTournaments.Modules.Tournaments.UnitTests.Domain;

public class TournamentTests
{
    [Test]
    public void Create_ShouldCreateTournamentInDraftStatus()
    {
        // Arrange
        var name = "Test Tournament";
        var description = "Test Description";
        var startDate = DateTime.UtcNow.AddDays(7);
        var location = "Test Location";
        var organizerId = "organizer123";
        var settings = new TournamentSettings(
            format: TournamentFormat.Swiss,
            timeControl: TimeControl.Rapid,
            timeInMinutes: 15,
            incrementInSeconds: 10,
            numberOfRounds: 7,
            maxPlayers: 16,
            minPlayers: 4,
            allowByes: true,
            entryFee: 0m
        );

        // Act
        var result = Tournament.Create(
            name,
            description,
            startDate,
            settings,
            organizerId,
            location
        );

        // Assert
        result.IsSuccess.Should().BeTrue();
        var tournament = result.Value;
        tournament.Should().NotBeNull();
        tournament.Name.Should().Be(name);
        tournament.Description.Should().Be(description);
        tournament.StartDate.Should().Be(startDate);
        tournament.Location.Should().Be(location);
        tournament.OrganizerId.Should().Be(organizerId);
        tournament.Settings.Should().Be(settings);
        tournament.Status.Should().Be(TournamentStatus.Draft);
        tournament.Players.Should().BeEmpty();
        tournament.Rounds.Should().BeEmpty();
    }

    [Test]
    public void OpenRegistration_ShouldChangeStatusToRegistration_WhenInDraftStatus()
    {
        // Arrange
        var tournament = CreateTestTournament();

        // Act
        var result = tournament.OpenRegistration();

        // Assert
        result.IsSuccess.Should().BeTrue();
        tournament.Status.Should().Be(TournamentStatus.Registration);
    }

    [Test]
    public void OpenRegistration_ShouldReturnFailure_WhenNotInDraftStatus()
    {
        // Arrange
        var tournament = CreateTestTournament();
        tournament.OpenRegistration(); // Move to Registration status

        // Act
        var result = tournament.OpenRegistration();

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Tournament must be in draft status");
    }

    [Test]
    public void CloseRegistration_ShouldChangeStatusToInProgress_WhenInRegistrationStatus()
    {
        // Arrange
        var tournament = CreateTestTournament();
        tournament.OpenRegistration();

        // Add minimum required players
        tournament.RegisterPlayer("player1", "Player 1", 1500);
        tournament.RegisterPlayer("player2", "Player 2", 1600);
        tournament.RegisterPlayer("player3", "Player 3", 1400);
        tournament.RegisterPlayer("player4", "Player 4", 1700);

        // Act
        var result = tournament.CloseRegistration();

        // Assert
        result.IsSuccess.Should().BeTrue();
        tournament.Status.Should().Be(TournamentStatus.InProgress);
    }

    [Test]
    public void CloseRegistration_ShouldReturnFailure_WhenNotInRegistrationStatus()
    {
        // Arrange
        var tournament = CreateTestTournament();

        // Act
        var result = tournament.CloseRegistration();

        // Assert
        result.IsFailure.Should().BeTrue();
        result
            .Error.Should()
            .Be("Cannot close registration for tournament not accepting registrations");
    }

    [Test]
    public void Complete_ShouldChangeStatusToCompleted_WhenInProgressStatus()
    {
        // Arrange
        var tournament = CreateTestTournament();
        tournament.OpenRegistration();

        // Add minimum required players
        tournament.RegisterPlayer("player1", "Player 1", 1500);
        tournament.RegisterPlayer("player2", "Player 2", 1600);
        tournament.RegisterPlayer("player3", "Player 3", 1400);
        tournament.RegisterPlayer("player4", "Player 4", 1700);

        tournament.CloseRegistration();

        // Act
        var result = tournament.Complete();

        // Assert
        result.IsSuccess.Should().BeTrue();
        tournament.Status.Should().Be(TournamentStatus.Completed);
    }

    [Test]
    public void Complete_ShouldReturnFailure_WhenNotInProgressStatus()
    {
        // Arrange
        var tournament = CreateTestTournament();

        // Act
        var result = tournament.Complete();

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Cannot complete tournament that is not in progress");
    }

    [Test]
    public void Cancel_ShouldChangeStatusToCancelled_WhenNotCompleted()
    {
        // Arrange
        var tournament = CreateTestTournament();

        // Act
        var result = tournament.Cancel();

        // Assert
        result.IsSuccess.Should().BeTrue();
        tournament.Status.Should().Be(TournamentStatus.Cancelled);
    }

    [Test]
    public void Cancel_ShouldReturnFailure_WhenAlreadyCompleted()
    {
        // Arrange
        var tournament = CreateTestTournament();
        tournament.OpenRegistration();

        // Add minimum required players
        tournament.RegisterPlayer("player1", "Player 1", 1500);
        tournament.RegisterPlayer("player2", "Player 2", 1600);
        tournament.RegisterPlayer("player3", "Player 3", 1400);
        tournament.RegisterPlayer("player4", "Player 4", 1700);

        tournament.CloseRegistration();
        tournament.Complete();

        // Act
        var result = tournament.Cancel();

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Cannot cancel completed tournament");
    }

    [Test]
    public void Update_ShouldUpdateTournamentDetails()
    {
        // Arrange
        var tournament = CreateTestTournament();
        var newName = "Updated Tournament";
        var newDescription = "Updated Description";
        var newLocation = "Updated Location";

        // Act
        tournament.UpdateDetails(newName, newDescription, newLocation);

        // Assert
        tournament.Name.Should().Be(newName);
        tournament.Description.Should().Be(newDescription);
        tournament.Location.Should().Be(newLocation);
    }

    private static Tournament CreateTestTournament()
    {
        var result = Tournament.Create(
            "Test Tournament",
            "Test Description",
            DateTime.UtcNow.AddDays(7),
            new TournamentSettings(
                TournamentFormat.Swiss,
                TimeControl.Rapid,
                15,
                10,
                5,
                16,
                4,
                true,
                0
            ),
            "organizer123",
            "Test Location"
        );
        return result.Value;
    }
}
