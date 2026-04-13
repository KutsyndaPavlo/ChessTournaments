using ChessTournaments.Modules.Matches.Domain.Matches;
using ChessTournaments.Shared.Domain.Enums;

namespace ChessTournaments.Modules.Matches.UnitTests.Domain;

public class MatchTests
{
    [Test]
    public void Create_ShouldCreateMatch_WhenParametersAreValid()
    {
        // Arrange
        var roundId = Guid.NewGuid();
        var tournamentId = Guid.NewGuid();
        var whitePlayerId = "player1";
        var blackPlayerId = "player2";
        var boardNumber = 1;

        // Act
        var result = Match.Create(roundId, tournamentId, whitePlayerId, blackPlayerId, boardNumber);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var match = result.Value;
        match.RoundId.Should().Be(roundId);
        match.TournamentId.Should().Be(tournamentId);
        match.WhitePlayerId.Should().Be(whitePlayerId);
        match.BlackPlayerId.Should().Be(blackPlayerId);
        match.BoardNumber.Should().Be(boardNumber);
        match.Result.Should().Be(GameResult.Ongoing);
        match.IsCompleted.Should().BeFalse();
        match.CompletedAt.Should().BeNull();
        match.Tags.Should().BeEmpty();
    }

    [Test]
    public void Create_ShouldReturnFailure_WhenRoundIdIsEmpty()
    {
        // Arrange
        var roundId = Guid.Empty;
        var tournamentId = Guid.NewGuid();
        var whitePlayerId = "player1";
        var blackPlayerId = "player2";
        var boardNumber = 1;

        // Act
        var result = Match.Create(roundId, tournamentId, whitePlayerId, blackPlayerId, boardNumber);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Round");
    }

    [Test]
    public void Create_ShouldReturnFailure_WhenTournamentIdIsEmpty()
    {
        // Arrange
        var roundId = Guid.NewGuid();
        var tournamentId = Guid.Empty;
        var whitePlayerId = "player1";
        var blackPlayerId = "player2";
        var boardNumber = 1;

        // Act
        var result = Match.Create(roundId, tournamentId, whitePlayerId, blackPlayerId, boardNumber);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Tournament");
    }

    [Test]
    public void Create_ShouldReturnFailure_WhenWhitePlayerIdIsEmpty()
    {
        // Arrange
        var roundId = Guid.NewGuid();
        var tournamentId = Guid.NewGuid();
        var whitePlayerId = "";
        var blackPlayerId = "player2";
        var boardNumber = 1;

        // Act
        var result = Match.Create(roundId, tournamentId, whitePlayerId, blackPlayerId, boardNumber);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("White");
    }

    [Test]
    public void Create_ShouldReturnFailure_WhenBlackPlayerIdIsEmpty()
    {
        // Arrange
        var roundId = Guid.NewGuid();
        var tournamentId = Guid.NewGuid();
        var whitePlayerId = "player1";
        var blackPlayerId = "";
        var boardNumber = 1;

        // Act
        var result = Match.Create(roundId, tournamentId, whitePlayerId, blackPlayerId, boardNumber);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Black");
    }

    [Test]
    public void Create_ShouldReturnFailure_WhenPlayersAreSame()
    {
        // Arrange
        var roundId = Guid.NewGuid();
        var tournamentId = Guid.NewGuid();
        var whitePlayerId = "player1";
        var blackPlayerId = "player1";
        var boardNumber = 1;

        // Act
        var result = Match.Create(roundId, tournamentId, whitePlayerId, blackPlayerId, boardNumber);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("different");
    }

    [Test]
    [Arguments(0)]
    [Arguments(-1)]
    public void Create_ShouldReturnFailure_WhenBoardNumberIsInvalid(int boardNumber)
    {
        // Arrange
        var roundId = Guid.NewGuid();
        var tournamentId = Guid.NewGuid();
        var whitePlayerId = "player1";
        var blackPlayerId = "player2";

        // Act
        var result = Match.Create(roundId, tournamentId, whitePlayerId, blackPlayerId, boardNumber);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("board");
    }

    [Test]
    public void RecordResult_ShouldRecordResult_WhenMatchIsNotCompleted()
    {
        // Arrange
        var match = CreateTestMatch();
        var gameResult = GameResult.WhiteWin;
        var moves = "1. e4 e5 2. Nf3 Nc6";

        // Act
        var result = match.RecordResult(gameResult, moves);

        // Assert
        result.IsSuccess.Should().BeTrue();
        match.Result.Should().Be(gameResult);
        match.IsCompleted.Should().BeTrue();
        match.CompletedAt.Should().NotBeNull();
        match.CompletedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        match.Moves.Should().Be(moves);
    }

    [Test]
    [Arguments(GameResult.WhiteWin)]
    [Arguments(GameResult.BlackWin)]
    [Arguments(GameResult.Draw)]
    public void RecordResult_ShouldAcceptAllValidResults(GameResult gameResult)
    {
        // Arrange
        var match = CreateTestMatch();

        // Act
        var result = match.RecordResult(gameResult);

        // Assert
        result.IsSuccess.Should().BeTrue();
        match.Result.Should().Be(gameResult);
        match.IsCompleted.Should().BeTrue();
    }

    [Test]
    public void RecordResult_ShouldReturnFailure_WhenResultIsOngoing()
    {
        // Arrange
        var match = CreateTestMatch();

        // Act
        var result = match.RecordResult(GameResult.Ongoing);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("ongoing");
        match.IsCompleted.Should().BeFalse();
    }

    [Test]
    public void RecordResult_ShouldReturnFailure_WhenMatchIsAlreadyCompleted()
    {
        // Arrange
        var match = CreateTestMatch();
        match.RecordResult(GameResult.WhiteWin);

        // Act
        var result = match.RecordResult(GameResult.BlackWin);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("completed");
        match.Result.Should().Be(GameResult.WhiteWin);
    }

    [Test]
    public void UpdateMoves_ShouldUpdateMoves_WhenMatchIsNotCompleted()
    {
        // Arrange
        var match = CreateTestMatch();
        var moves = "1. e4 e5 2. Nf3 Nc6";

        // Act
        var result = match.UpdateMoves(moves);

        // Assert
        result.IsSuccess.Should().BeTrue();
        match.Moves.Should().Be(moves);
    }

    [Test]
    public void UpdateMoves_ShouldReturnFailure_WhenMatchIsCompleted()
    {
        // Arrange
        var match = CreateTestMatch();
        match.RecordResult(GameResult.WhiteWin, "1. e4 e5");

        // Act
        var result = match.UpdateMoves("1. d4 d5");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("completed");
        match.Moves.Should().Be("1. e4 e5");
    }

    [Test]
    public void AddTag_ShouldAddTag_WhenTagIsValid()
    {
        // Arrange
        var match = CreateTestMatch();
        var tagName = "Opening: Sicilian Defense";

        // Act
        var result = match.AddTag(tagName);

        // Assert
        result.IsSuccess.Should().BeTrue();
        match.Tags.Should().ContainSingle();
        match.Tags.First().Name.Should().Be(tagName);
    }

    [Test]
    public void AddTag_ShouldAddMultipleTags()
    {
        // Arrange
        var match = CreateTestMatch();

        // Act
        match.AddTag("Opening: Sicilian Defense");
        match.AddTag("Endgame: Rook and Pawn");
        var result = match.AddTag("Time Control: Blitz");

        // Assert
        result.IsSuccess.Should().BeTrue();
        match.Tags.Should().HaveCount(3);
    }

    [Test]
    public void AddTag_ShouldReturnFailure_WhenTagNameIsEmpty()
    {
        // Arrange
        var match = CreateTestMatch();

        // Act
        var result = match.AddTag("");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("empty");
        match.Tags.Should().BeEmpty();
    }

    [Test]
    public void AddTag_ShouldReturnFailure_WhenTagAlreadyExists()
    {
        // Arrange
        var match = CreateTestMatch();
        var tagName = "Opening: Sicilian Defense";
        match.AddTag(tagName);

        // Act
        var result = match.AddTag(tagName);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("already exists");
        match.Tags.Should().ContainSingle();
    }

    [Test]
    public void RemoveTag_ShouldRemoveTag_WhenTagExists()
    {
        // Arrange
        var match = CreateTestMatch();
        var tagName = "Opening: Sicilian Defense";
        match.AddTag(tagName);

        // Act
        var result = match.RemoveTag(tagName);

        // Assert
        result.IsSuccess.Should().BeTrue();
        match.Tags.Should().BeEmpty();
    }

    [Test]
    public void RemoveTag_ShouldReturnFailure_WhenTagDoesNotExist()
    {
        // Arrange
        var match = CreateTestMatch();

        // Act
        var result = match.RemoveTag("NonExistent Tag");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("not found");
    }

    private static Match CreateTestMatch()
    {
        var result = Match.Create(Guid.NewGuid(), Guid.NewGuid(), "player1", "player2", 1);
        return result.Value;
    }
}
