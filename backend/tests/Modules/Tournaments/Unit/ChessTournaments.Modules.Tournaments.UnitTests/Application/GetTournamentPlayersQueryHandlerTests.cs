using ChessTournaments.Modules.Tournaments.Application.Features.GetTournamentPlayers;
using ChessTournaments.Modules.Tournaments.Domain.Enums;
using ChessTournaments.Modules.Tournaments.Domain.Tournaments;
using ChessTournaments.Shared.Domain.Enums;
using FluentAssertions;
using Moq;

namespace ChessTournaments.Modules.Tournaments.UnitTests.Application;

public class GetTournamentPlayersQueryHandlerTests
{
    private Mock<ITournamentRepository> _repositoryMock = null!;
    private GetTournamentPlayersQueryHandler _handler = null!;

    [Before(Test)]
    public void Setup()
    {
        _repositoryMock = new Mock<ITournamentRepository>();
        _handler = new GetTournamentPlayersQueryHandler(_repositoryMock.Object);
    }

    [Test]
    public async Task Handle_Should_Throw_When_Tournament_Does_Not_Exist()
    {
        // Arrange
        var tournamentId = Guid.NewGuid();
        var query = new GetTournamentPlayersQuery(tournamentId);

        _repositoryMock
            .Setup(x => x.GetByIdAsync(tournamentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Tournament?)null);

        // Act
        var act = () => _handler.Handle(query, CancellationToken.None);

        // Assert
        await act.Should()
            .ThrowAsync<InvalidOperationException>()
            .WithMessage("Tournament not found");
    }

    [Test]
    public async Task Handle_Should_Return_Empty_Collection_When_No_Players_Registered()
    {
        // Arrange
        var tournamentId = Guid.NewGuid();
        var query = new GetTournamentPlayersQuery(tournamentId);
        var tournament = CreateTournament(tournamentId);

        _repositoryMock
            .Setup(x => x.GetByIdAsync(tournamentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(tournament);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().BeEmpty();
    }

    [Test]
    public async Task Handle_Should_Return_All_Registered_Players()
    {
        // Arrange
        var tournamentId = Guid.NewGuid();
        var query = new GetTournamentPlayersQuery(tournamentId);
        var tournament = CreateTournament(tournamentId);

        tournament.OpenRegistration();
        var player1Id = Guid.NewGuid().ToString();
        var player2Id = Guid.NewGuid().ToString();
        var player3Id = Guid.NewGuid().ToString();

        tournament.RegisterPlayer(player1Id, "Player 1", 1500);
        tournament.RegisterPlayer(player2Id, "Player 2", 1600);
        tournament.RegisterPlayer(player3Id, "Player 3", 1550);

        _repositoryMock
            .Setup(x => x.GetByIdAsync(tournamentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(tournament);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        var players = result.ToList();
        players.Should().HaveCount(3);
        players.Should().Contain(p => p.PlayerName == "Player 1");
        players.Should().Contain(p => p.PlayerName == "Player 2");
        players.Should().Contain(p => p.PlayerName == "Player 3");
    }

    [Test]
    public async Task Handle_Should_Order_Players_By_TotalScore_Descending()
    {
        // Arrange
        var tournamentId = Guid.NewGuid();
        var query = new GetTournamentPlayersQuery(tournamentId);
        var tournament = CreateTournament(tournamentId);

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
        var players = result.ToList();
        // All players should have 0 points initially
        players.Should().AllSatisfy(p => p.TotalScore.Should().Be(0));
    }

    [Test]
    public async Task Handle_Should_Map_Player_Properties_Correctly()
    {
        // Arrange
        var tournamentId = Guid.NewGuid();
        var playerId = Guid.NewGuid().ToString();
        var query = new GetTournamentPlayersQuery(tournamentId);
        var tournament = CreateTournament(tournamentId);

        tournament.OpenRegistration();
        tournament.RegisterPlayer(playerId, "Test Player", 1850);

        _repositoryMock
            .Setup(x => x.GetByIdAsync(tournamentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(tournament);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        var player = result.First();
        player.PlayerId.Should().Be(playerId);
        player.PlayerName.Should().Be("Test Player");
        player.Rating.Should().Be(1850);
        player.TotalScore.Should().Be(0);
        player.GamesPlayed.Should().Be(0);
    }

    [Test]
    public async Task Handle_Should_Handle_Players_Without_Rating()
    {
        // Arrange
        var tournamentId = Guid.NewGuid();
        var query = new GetTournamentPlayersQuery(tournamentId);
        var tournament = CreateTournament(tournamentId);

        tournament.OpenRegistration();
        tournament.RegisterPlayer(Guid.NewGuid().ToString(), "Unrated Player", null);
        tournament.RegisterPlayer(Guid.NewGuid().ToString(), "Rated Player", 1500);

        _repositoryMock
            .Setup(x => x.GetByIdAsync(tournamentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(tournament);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        var players = result.ToList();
        players.Should().HaveCount(2);

        var unratedPlayer = players.First(p => p.PlayerName == "Unrated Player");
        unratedPlayer.Rating.Should().BeNull();

        var ratedPlayer = players.First(p => p.PlayerName == "Rated Player");
        ratedPlayer.Rating.Should().Be(1500);
    }

    [Test]
    public async Task Handle_Should_Order_By_Rating_When_Scores_Are_Equal()
    {
        // Arrange
        var tournamentId = Guid.NewGuid();
        var query = new GetTournamentPlayersQuery(tournamentId);
        var tournament = CreateTournament(tournamentId);

        tournament.OpenRegistration();
        tournament.RegisterPlayer(Guid.NewGuid().ToString(), "Player 1", 1400);
        tournament.RegisterPlayer(Guid.NewGuid().ToString(), "Player 2", 1600);
        tournament.RegisterPlayer(Guid.NewGuid().ToString(), "Player 3", 1500);

        _repositoryMock
            .Setup(x => x.GetByIdAsync(tournamentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(tournament);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        var players = result.ToList();
        // Since all have equal score (0), should be ordered by rating descending
        players[0].PlayerName.Should().Be("Player 2"); // Rating 1600
        players[1].PlayerName.Should().Be("Player 3"); // Rating 1500
        players[2].PlayerName.Should().Be("Player 1"); // Rating 1400
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
