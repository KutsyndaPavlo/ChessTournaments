using ChessTournaments.Modules.Tournaments.Application.Features.RegisterPlayer;
using ChessTournaments.Modules.Tournaments.Domain.Enums;
using ChessTournaments.Modules.Tournaments.Domain.Tournaments;
using ChessTournaments.Shared.Domain.Enums;
using FluentAssertions;
using MediatR;
using Moq;

namespace ChessTournaments.Modules.Tournaments.UnitTests.Application;

public class RegisterPlayerCommandHandlerTests
{
    private Mock<ITournamentRepository> _repositoryMock = null!;
    private RegisterPlayerCommandHandler _handler = null!;

    [Before(Test)]
    public void Setup()
    {
        _repositoryMock = new Mock<ITournamentRepository>();
        _handler = new RegisterPlayerCommandHandler(_repositoryMock.Object);
    }

    [Test]
    public async Task Handle_Should_Return_Failure_When_Tournament_Does_Not_Exist()
    {
        // Arrange
        var command = new RegisterPlayerCommand(
            Guid.NewGuid(),
            Guid.NewGuid().ToString(),
            "Player Name",
            1500
        );

        _repositoryMock
            .Setup(x => x.GetByIdAsync(command.TournamentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Tournament?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("not found");
    }

    [Test]
    public async Task Handle_Should_Register_Player_To_Tournament()
    {
        // Arrange
        var tournamentId = Guid.NewGuid();
        var playerId = Guid.NewGuid().ToString();
        var tournament = CreateTournament(tournamentId);
        tournament.OpenRegistration();

        var command = new RegisterPlayerCommand(tournamentId, playerId, "John Doe", 1500);

        _repositoryMock
            .Setup(x => x.GetByIdAsync(tournamentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(tournament);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        tournament.Players.Should().HaveCount(1);
        tournament.Players.First().PlayerId.Should().Be(playerId);
        tournament.Players.First().PlayerName.Should().Be("John Doe");
        tournament.Players.First().Rating.Should().Be(1500);
        _repositoryMock.Verify(
            x => x.UpdateAsync(tournament, It.IsAny<CancellationToken>()),
            Times.Once
        );
        _repositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task Handle_Should_Register_Player_Without_Rating()
    {
        // Arrange
        var tournamentId = Guid.NewGuid();
        var playerId = Guid.NewGuid().ToString();
        var tournament = CreateTournament(tournamentId);
        tournament.OpenRegistration();

        var command = new RegisterPlayerCommand(tournamentId, playerId, "Jane Doe", null);

        _repositoryMock
            .Setup(x => x.GetByIdAsync(tournamentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(tournament);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        tournament.Players.Should().HaveCount(1);
        tournament.Players.First().Rating.Should().BeNull();
    }

    [Test]
    public async Task Handle_Should_Register_Multiple_Players()
    {
        // Arrange
        var tournamentId = Guid.NewGuid();
        var tournament = CreateTournament(tournamentId);
        tournament.OpenRegistration();

        _repositoryMock
            .Setup(x => x.GetByIdAsync(tournamentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(tournament);

        // Act - Register first player
        await _handler.Handle(
            new RegisterPlayerCommand(tournamentId, Guid.NewGuid().ToString(), "Player 1", 1500),
            CancellationToken.None
        );

        // Act - Register second player
        await _handler.Handle(
            new RegisterPlayerCommand(tournamentId, Guid.NewGuid().ToString(), "Player 2", 1600),
            CancellationToken.None
        );

        // Assert
        tournament.Players.Should().HaveCount(2);
    }

    private static Tournament CreateTournament(Guid id)
    {
        var tournamentResult = Tournament.Create(
            "Test Tournament",
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
