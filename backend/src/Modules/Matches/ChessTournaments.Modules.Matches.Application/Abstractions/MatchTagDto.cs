namespace ChessTournaments.Modules.Matches.Application.Abstractions;

public class MatchTagDto
{
    public Guid Id { get; set; }
    public Guid MatchId { get; set; }
    public string Name { get; set; } = string.Empty;
}
