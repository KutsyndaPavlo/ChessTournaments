using ChessTournaments.Modules.Tournaments.Application.Features.OpenRegistration;
using ChessTournaments.Modules.Tournaments.Domain.Enums;
using ChessTournaments.Modules.Tournaments.Domain.Tournaments;
using ChessTournaments.Shared.Domain.Enums;
using FluentAssertions;
using MediatR;
using Moq;

namespace ChessTournaments.Modules.Tournaments.UnitTests.Application;

public class OpenRegistrationCommandHandlerTests
{
    private Mock<ITournamentRepository> _repositoryMock = null!;
    private OpenRegistrationCommandHandler _handler = null!;

    [Before(Test)]
    public void Setup()
    {
        _repositoryMock = new Mock<ITournamentRepository>();
        _handler = new OpenRegistrationCommandHandler(_repositoryMock.Object);
    }

    [Test]
    public async Task Handle_Should_Return_Failure_When_Tournament_Does_Not_Exist()
    {
        // Arrange
        var command = new OpenRegistrationCommand(Guid.NewGuid());

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
    public async Task Handle_Should_Open_Registration_For_Tournament()
    {
        // Arrange
        var tournamentId = Guid.NewGuid();
        var tournament = CreateTournament(tournamentId);
        var command = new OpenRegistrationCommand(tournamentId);

        _repositoryMock
            .Setup(x => x.GetByIdAsync(tournamentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(tournament);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        tournament.Status.Should().Be(TournamentStatus.Registration);
        _repositoryMock.Verify(
            x => x.UpdateAsync(tournament, It.IsAny<CancellationToken>()),
            Times.Once
        );
        _repositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task Handle_Should_Change_Status_To_Registration()
    {
        // Arrange
        var tournamentId = Guid.NewGuid();
        var tournament = CreateTournament(tournamentId);
        tournament.Status.Should().Be(TournamentStatus.Draft);

        var command = new OpenRegistrationCommand(tournamentId);

        _repositoryMock
            .Setup(x => x.GetByIdAsync(tournamentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(tournament);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        tournament.Status.Should().Be(TournamentStatus.Registration);
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
