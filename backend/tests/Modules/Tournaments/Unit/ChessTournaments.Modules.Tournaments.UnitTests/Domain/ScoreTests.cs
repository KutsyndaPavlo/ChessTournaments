using ChessTournaments.Modules.Tournaments.Domain.TournamentPlayers;
using FluentAssertions;

namespace ChessTournaments.Modules.Tournaments.UnitTests.Domain;

public class ScoreTests
{
    [Test]
    public void Constructor_Should_Create_Score_With_Valid_Points()
    {
        // Arrange & Act
        var score = new Score(2.5m);

        // Assert
        score.Points.Should().Be(2.5m);
    }

    [Test]
    public void Constructor_Should_Allow_Zero_Points()
    {
        // Arrange & Act
        var score = new Score(0);

        // Assert
        score.Points.Should().Be(0);
    }

    [Test]
    public void Constructor_Should_Throw_When_Points_Are_Negative()
    {
        // Arrange & Act
        var act = () => new Score(-1);

        // Assert
        act.Should()
            .Throw<ArgumentException>()
            .WithMessage("Points cannot be negative*")
            .And.ParamName.Should()
            .Be("points");
    }

    [Test]
    public void Win_Should_Return_One_Point()
    {
        // Arrange & Act
        var score = Score.Win;

        // Assert
        score.Points.Should().Be(1.0m);
    }

    [Test]
    public void Draw_Should_Return_Half_Point()
    {
        // Arrange & Act
        var score = Score.Draw;

        // Assert
        score.Points.Should().Be(0.5m);
    }

    [Test]
    public void Loss_Should_Return_Zero_Points()
    {
        // Arrange & Act
        var score = Score.Loss;

        // Assert
        score.Points.Should().Be(0.0m);
    }

    [Test]
    public void Addition_Operator_Should_Add_Two_Scores()
    {
        // Arrange
        var score1 = new Score(1.5m);
        var score2 = new Score(2.0m);

        // Act
        var result = score1 + score2;

        // Assert
        result.Points.Should().Be(3.5m);
    }

    [Test]
    public void Addition_Operator_Should_Add_Win_And_Draw()
    {
        // Arrange & Act
        var result = Score.Win + Score.Draw;

        // Assert
        result.Points.Should().Be(1.5m);
    }

    [Test]
    public void Addition_Operator_Should_Add_Multiple_Scores()
    {
        // Arrange & Act
        var result = Score.Win + Score.Win + Score.Draw + Score.Loss;

        // Assert
        result.Points.Should().Be(2.5m);
    }

    [Test]
    public void Record_Equality_Should_Work_For_Same_Points()
    {
        // Arrange
        var score1 = new Score(1.5m);
        var score2 = new Score(1.5m);

        // Act & Assert
        score1.Should().Be(score2);
        (score1 == score2).Should().BeTrue();
    }

    [Test]
    public void Record_Equality_Should_Fail_For_Different_Points()
    {
        // Arrange
        var score1 = new Score(1.5m);
        var score2 = new Score(2.0m);

        // Act & Assert
        score1.Should().NotBe(score2);
        (score1 == score2).Should().BeFalse();
    }

    [Test]
    [Arguments(0)]
    [Arguments(0.5)]
    [Arguments(1.0)]
    [Arguments(2.5)]
    [Arguments(10.0)]
    public void Constructor_Should_Accept_Various_Valid_Points(decimal points)
    {
        // Arrange & Act
        var score = new Score(points);

        // Assert
        score.Points.Should().Be(points);
    }

    [Test]
    [Arguments(-0.5)]
    [Arguments(-1)]
    [Arguments(-10)]
    public void Constructor_Should_Throw_For_Any_Negative_Value(decimal points)
    {
        // Arrange & Act
        var act = () => new Score(points);

        // Assert
        act.Should().Throw<ArgumentException>();
    }
}
