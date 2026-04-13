using ChessTournaments.Modules.Tournaments.Application.Features.UpdateTournament;
using ChessTournaments.Modules.Tournaments.Domain.Enums;
using ChessTournaments.Modules.Tournaments.Domain.Tournaments;
using ChessTournaments.Shared.Domain.Enums;
using FluentAssertions;
using Moq;

namespace ChessTournaments.Modules.Tournaments.UnitTests.Application;

public class UpdateTournamentCommandHandlerTests
{
    private Mock<ITournamentRepository> _repositoryMock = null!;
    private UpdateTournamentCommandHandler _handler = null!;

    [Before(Test)]
    public void Setup()
    {
        _repositoryMock = new Mock<ITournamentRepository>();
        _handler = new UpdateTournamentCommandHandler(_repositoryMock.Object);
    }

    [Test]
    public async Task Handle_Should_Return_Failure_When_Tournament_Does_Not_Exist()
    {
        // Arrange
        var command = new UpdateTournamentCommand(
            Guid.NewGuid(),
            "Updated Name",
            "Updated Description",
            "Updated Location"
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
    public async Task Handle_Should_Update_Tournament_Details()
    {
        // Arrange
        var tournamentId = Guid.NewGuid();
        var tournament = CreateTournament(tournamentId);

        var command = new UpdateTournamentCommand(
            tournamentId,
            "Updated Name",
            "Updated Description",
            "Updated Location"
        );

        _repositoryMock
            .Setup(x => x.GetByIdAsync(tournamentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(tournament);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be("Updated Name");
        result.Value.Description.Should().Be("Updated Description");
        _repositoryMock.Verify(
            x => x.UpdateAsync(tournament, It.IsAny<CancellationToken>()),
            Times.Once
        );
        _repositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task Handle_Should_Return_Updated_Tournament_Dto()
    {
        // Arrange
        var tournamentId = Guid.NewGuid();
        var tournament = CreateTournament(tournamentId);

        var command = new UpdateTournamentCommand(
            tournamentId,
            "Updated Name",
            "Updated Description",
            "Updated Location"
        );

        _repositoryMock
            .Setup(x => x.GetByIdAsync(tournamentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(tournament);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Id.Should().Be(tournamentId);
        result.Value.Name.Should().Be("Updated Name");
        result.Value.Description.Should().Be("Updated Description");
    }

    [Test]
    public async Task Handle_Should_Preserve_Other_Tournament_Properties()
    {
        // Arrange
        var tournamentId = Guid.NewGuid();
        var tournament = CreateTournament(tournamentId);
        var originalStatus = tournament.Status;
        var originalStartDate = tournament.StartDate;

        var command = new UpdateTournamentCommand(
            tournamentId,
            "Updated Name",
            "Updated Description",
            "Updated Location"
        );

        _repositoryMock
            .Setup(x => x.GetByIdAsync(tournamentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(tournament);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().Be(originalStatus);
        result.Value.StartDate.Should().Be(originalStartDate);
        result.Value.Settings.Should().NotBeNull();
    }

    private static Tournament CreateTournament(Guid id)
    {
        var tournamentResult = Tournament.Create(
            "Original Name",
            "Original Description",
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
