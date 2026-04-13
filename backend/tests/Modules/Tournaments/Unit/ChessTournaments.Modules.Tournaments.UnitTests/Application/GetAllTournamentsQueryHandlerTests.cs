using ChessTournaments.Modules.Tournaments.Application.Abstractions;
using ChessTournaments.Modules.Tournaments.Application.Features.GetAllTournaments;
using ChessTournaments.Modules.Tournaments.Domain.Enums;
using ChessTournaments.Modules.Tournaments.Domain.Tournaments;
using ChessTournaments.Shared.Domain.Enums;
using FluentAssertions;
using Moq;

namespace ChessTournaments.Modules.Tournaments.UnitTests.Application;

public class GetAllTournamentsQueryHandlerTests
{
    private Mock<ITournamentRepository> _repositoryMock = null!;
    private GetAllTournamentsQueryHandler _handler = null!;

    [Before(Test)]
    public void Setup()
    {
        _repositoryMock = new Mock<ITournamentRepository>();
        _handler = new GetAllTournamentsQueryHandler(_repositoryMock.Object);
    }

    [Test]
    public async Task Handle_Should_Return_Empty_Collection_When_No_Tournaments_Exist()
    {
        // Arrange
        var query = new GetAllTournamentsQuery();
        _repositoryMock
            .Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Tournament>());

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().BeEmpty();
        _repositoryMock.Verify(x => x.GetAllAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task Handle_Should_Return_All_Tournaments_As_Dtos()
    {
        // Arrange
        var query = new GetAllTournamentsQuery();
        var tournaments = new List<Tournament>
        {
            CreateTournament("Tournament 1"),
            CreateTournament("Tournament 2"),
            CreateTournament("Tournament 3"),
        };

        _repositoryMock
            .Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(tournaments);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        var resultList = result.ToList();
        resultList.Should().HaveCount(3);
        resultList[0].Name.Should().Be("Tournament 1");
        resultList[1].Name.Should().Be("Tournament 2");
        resultList[2].Name.Should().Be("Tournament 3");
    }

    [Test]
    public async Task Handle_Should_Map_Tournament_Properties_Correctly()
    {
        // Arrange
        var query = new GetAllTournamentsQuery();
        var tournamentId = Guid.NewGuid();
        var startDate = DateTime.UtcNow.AddDays(7);

        var tournamentResult = Tournament.Create(
            "Test Tournament",
            "Test Description",
            startDate,
            new TournamentSettings(
                TournamentFormat.Swiss,
                TimeControl.Rapid,
                15,
                10,
                5,
                20,
                4,
                true,
                10
            ),
            Guid.NewGuid().ToString(),
            "Test Location"
        );

        var tournament = tournamentResult.Value;

        // Set the Id using reflection for testing purposes
        var idProperty = typeof(Tournament).BaseType!.GetProperty("Id");
        idProperty!.SetValue(tournament, tournamentId);

        _repositoryMock
            .Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Tournament> { tournament });

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        var dto = result.First();
        dto.Id.Should().Be(tournamentId);
        dto.Name.Should().Be("Test Tournament");
        dto.Description.Should().Be("Test Description");
        dto.StartDate.Should().Be(startDate);
        dto.Status.Should().Be(TournamentStatus.Draft);
        dto.OrganizerId.Should().NotBeEmpty();
        dto.Location.Should().Be("Test Location");
        dto.Settings.Format.Should().Be(TournamentFormat.Swiss);
        dto.Settings.TimeControl.Should().Be(TimeControl.Rapid);
        dto.Settings.TimeInMinutes.Should().Be(15);
        dto.Settings.IncrementInSeconds.Should().Be(10);
        dto.Settings.NumberOfRounds.Should().Be(5);
        dto.Settings.MaxPlayers.Should().Be(20);
        dto.Settings.MinPlayers.Should().Be(4);
        dto.Settings.AllowByes.Should().BeTrue();
        dto.Settings.EntryFee.Should().Be(10);
        dto.PlayerCount.Should().Be(0);
        dto.RoundCount.Should().Be(0);
    }

    [Test]
    public async Task Handle_Should_Include_Player_And_Round_Counts()
    {
        // Arrange
        var query = new GetAllTournamentsQuery();
        var tournament = CreateTournament("Test Tournament");

        tournament.OpenRegistration();
        tournament.RegisterPlayer(Guid.NewGuid().ToString(), "Player 1", 1500);
        tournament.RegisterPlayer(Guid.NewGuid().ToString(), "Player 2", 1600);

        _repositoryMock
            .Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Tournament> { tournament });

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        var dto = result.First();
        dto.PlayerCount.Should().Be(2);
    }

    private static Tournament CreateTournament(string name)
    {
        var result = Tournament.Create(
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
        return result.Value;
    }
}
