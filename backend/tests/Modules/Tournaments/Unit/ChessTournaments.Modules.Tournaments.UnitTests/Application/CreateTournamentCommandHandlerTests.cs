using ChessTournaments.Modules.Tournaments.Application.Features.CreateTournament;
using ChessTournaments.Modules.Tournaments.Domain.Enums;
using ChessTournaments.Modules.Tournaments.Domain.Tournaments;
using ChessTournaments.Shared.Domain.Enums;
using Moq;

namespace ChessTournaments.Modules.Tournaments.UnitTests.Application;

public class CreateTournamentCommandHandlerTests
{
    private Mock<ITournamentRepository> _repositoryMock = null!;
    private CreateTournamentCommandHandler _handler = null!;

    [Before(Test)]
    public void Setup()
    {
        _repositoryMock = new Mock<ITournamentRepository>();
        _handler = new CreateTournamentCommandHandler(_repositoryMock.Object);
    }

    [Test]
    public async Task Handle_ShouldCreateTournament_WhenCommandIsValid()
    {
        // Arrange
        var command = new CreateTournamentCommand(
            Name: "Test Tournament",
            Description: "Test Description",
            StartDate: DateTime.UtcNow.AddDays(7),
            Location: "Test Location",
            OrganizerId: "organizer123",
            Format: TournamentFormat.Swiss,
            TimeControl: TimeControl.Rapid,
            TimeInMinutes: 15,
            IncrementInSeconds: 10,
            NumberOfRounds: 5,
            MaxPlayers: 16,
            MinPlayers: 4,
            AllowByes: true,
            EntryFee: 0
        );

        Tournament? capturedTournament = null;
        _repositoryMock
            .Setup(r => r.AddAsync(It.IsAny<Tournament>(), It.IsAny<CancellationToken>()))
            .Callback<Tournament, CancellationToken>((t, _) => capturedTournament = t)
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Name.Should().Be(command.Name);
        result.Value.Description.Should().Be(command.Description);
        result.Value.Location.Should().Be(command.Location);
        result.Value.OrganizerId.Should().Be(command.OrganizerId);
        result.Value.Status.Should().Be(TournamentStatus.Draft);

        _repositoryMock.Verify(
            r => r.AddAsync(It.IsAny<Tournament>(), It.IsAny<CancellationToken>()),
            Times.Once
        );

        capturedTournament.Should().NotBeNull();
        capturedTournament!.Name.Should().Be(command.Name);
        capturedTournament.Status.Should().Be(TournamentStatus.Draft);
    }

    [Test]
    public async Task Handle_ShouldCreateTournamentWithCorrectSettings()
    {
        // Arrange
        var command = new CreateTournamentCommand(
            Name: "Test Tournament",
            Description: "Test Description",
            StartDate: DateTime.UtcNow.AddDays(7),
            Location: "Test Location",
            OrganizerId: "organizer123",
            Format: TournamentFormat.RoundRobin,
            TimeControl: TimeControl.Blitz,
            TimeInMinutes: 5,
            IncrementInSeconds: 3,
            NumberOfRounds: 7,
            MaxPlayers: 8,
            MinPlayers: 4,
            AllowByes: false,
            EntryFee: 10
        );

        Tournament? capturedTournament = null;
        _repositoryMock
            .Setup(r => r.AddAsync(It.IsAny<Tournament>(), It.IsAny<CancellationToken>()))
            .Callback<Tournament, CancellationToken>((t, _) => capturedTournament = t)
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        capturedTournament.Should().NotBeNull();
        capturedTournament!.Settings.Format.Should().Be(TournamentFormat.RoundRobin);
        capturedTournament.Settings.TimeControl.Should().Be(TimeControl.Blitz);
        capturedTournament.Settings.MaxPlayers.Should().Be(8);
        capturedTournament.Settings.TimeInMinutes.Should().Be(5);
        capturedTournament.Settings.IncrementInSeconds.Should().Be(3);

        result.Value.Settings.Format.Should().Be(TournamentFormat.RoundRobin);
        result.Value.Settings.TimeControl.Should().Be(TimeControl.Blitz);
        result.Value.Settings.MaxPlayers.Should().Be(8);
    }

    [Test]
    public async Task Handle_ShouldGenerateUniqueId_ForEachTournament()
    {
        // Arrange
        var command1 = CreateTestCommand();
        var command2 = CreateTestCommand();

        var tournaments = new List<Tournament>();

        _repositoryMock
            .Setup(r => r.AddAsync(It.IsAny<Tournament>(), It.IsAny<CancellationToken>()))
            .Callback<Tournament, CancellationToken>((t, _) => tournaments.Add(t))
            .Returns(Task.CompletedTask);

        // Act
        await _handler.Handle(command1, CancellationToken.None);
        await _handler.Handle(command2, CancellationToken.None);

        // Assert
        tournaments.Should().HaveCount(2);
        tournaments[0].Id.Should().NotBe(tournaments[1].Id);
        tournaments[0].Id.Should().NotBe(Guid.Empty);
        tournaments[1].Id.Should().NotBe(Guid.Empty);
    }

    private static CreateTournamentCommand CreateTestCommand() =>
        new(
            Name: "Test Tournament",
            Description: "Test Description",
            StartDate: DateTime.UtcNow.AddDays(7),
            Location: "Test Location",
            OrganizerId: "organizer123",
            Format: TournamentFormat.Swiss,
            TimeControl: TimeControl.Rapid,
            TimeInMinutes: 15,
            IncrementInSeconds: 10,
            NumberOfRounds: 5,
            MaxPlayers: 16,
            MinPlayers: 4,
            AllowByes: true,
            EntryFee: 0
        );
}
