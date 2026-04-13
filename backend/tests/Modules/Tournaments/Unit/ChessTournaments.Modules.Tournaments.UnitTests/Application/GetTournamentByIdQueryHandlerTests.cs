using ChessTournaments.Modules.Tournaments.Application.Abstractions;
using ChessTournaments.Modules.Tournaments.Application.Features.GetTournamentById;
using ChessTournaments.Modules.Tournaments.Domain.Enums;
using ChessTournaments.Modules.Tournaments.Domain.Tournaments;
using ChessTournaments.Shared.Domain.Enums;
using FluentAssertions;
using Moq;

namespace ChessTournaments.Modules.Tournaments.UnitTests.Application;

public class GetTournamentByIdQueryHandlerTests
{
    private Mock<ITournamentRepository> _repositoryMock = null!;
    private GetTournamentByIdQueryHandler _handler = null!;

    [Before(Test)]
    public void Setup()
    {
        _repositoryMock = new Mock<ITournamentRepository>();
        _handler = new GetTournamentByIdQueryHandler(_repositoryMock.Object);
    }

    [Test]
    public async Task Handle_Should_Return_Failure_When_Tournament_Does_Not_Exist()
    {
        // Arrange
        var tournamentId = Guid.NewGuid();
        var query = new GetTournamentByIdQuery(tournamentId);

        _repositoryMock
            .Setup(x => x.GetByIdAsync(tournamentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Tournament?)null);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        _repositoryMock.Verify(
            x => x.GetByIdAsync(tournamentId, It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Test]
    public async Task Handle_Should_Return_Tournament_Dto_When_Tournament_Exists()
    {
        // Arrange
        var tournamentId = Guid.NewGuid();
        var query = new GetTournamentByIdQuery(tournamentId);
        var tournament = CreateTournament(tournamentId, "Test Tournament");

        _repositoryMock
            .Setup(x => x.GetByIdAsync(tournamentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(tournament);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Id.Should().Be(tournamentId);
        result.Value.Name.Should().Be("Test Tournament");
    }

    [Test]
    public async Task Handle_Should_Map_All_Tournament_Properties_Correctly()
    {
        // Arrange
        var tournamentId = Guid.NewGuid();
        var organizerId = Guid.NewGuid().ToString();
        var startDate = DateTime.UtcNow.AddDays(7);
        var query = new GetTournamentByIdQuery(tournamentId);

        var tournament = CreateTournament(tournamentId, "Test Tournament");

        _repositoryMock
            .Setup(x => x.GetByIdAsync(tournamentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(tournament);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Id.Should().Be(tournamentId);
        result.Value.Name.Should().Be("Test Tournament");
        result.Value.Description.Should().Be("Description");
        result.Value.Status.Should().Be(TournamentStatus.Draft);
        result.Value.Location.Should().Be("Location");

        result.Value.Settings.Should().NotBeNull();
        result.Value.Settings.Format.Should().Be(TournamentFormat.Swiss);
        result.Value.Settings.TimeControl.Should().Be(TimeControl.Rapid);
        result.Value.Settings.TimeInMinutes.Should().Be(15);
        result.Value.Settings.IncrementInSeconds.Should().Be(10);
        result.Value.Settings.NumberOfRounds.Should().Be(5);
        result.Value.Settings.MaxPlayers.Should().Be(20);
        result.Value.Settings.MinPlayers.Should().Be(4);
        result.Value.Settings.AllowByes.Should().BeTrue();
        result.Value.Settings.EntryFee.Should().Be(0);
    }

    [Test]
    public async Task Handle_Should_Include_Player_And_Round_Counts()
    {
        // Arrange
        var tournamentId = Guid.NewGuid();
        var query = new GetTournamentByIdQuery(tournamentId);
        var tournament = CreateTournament(tournamentId, "Test Tournament");

        tournament.OpenRegistration();
        tournament.RegisterPlayer(Guid.NewGuid().ToString(), "Player 1", 1500);
        tournament.RegisterPlayer(Guid.NewGuid().ToString(), "Player 2", 1600);
        tournament.RegisterPlayer(Guid.NewGuid().ToString(), "Player 3", 1550);

        _repositoryMock
            .Setup(x => x.GetByIdAsync(tournamentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(tournament);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.PlayerCount.Should().Be(3);
        result.Value.RoundCount.Should().Be(0);
    }

    private static Tournament CreateTournament(Guid id, string name)
    {
        var tournamentResult = Tournament.Create(
            name,
            "Description",
            DateTime.UtcNow.AddDays(7),
            new TournamentSettings(
                TournamentFormat.Swiss,
                TimeControl.Rapid,
                15,
                10,
                5,
                20,
                4,
                true,
                0
            ),
            Guid.NewGuid().ToString(),
            "Location"
        );

        var tournament = tournamentResult.Value;

        // Set the Id using reflection for testing purposes
        var idProperty = typeof(Tournament).BaseType!.GetProperty("Id");
        idProperty!.SetValue(tournament, id);

        return tournament;
    }
}
