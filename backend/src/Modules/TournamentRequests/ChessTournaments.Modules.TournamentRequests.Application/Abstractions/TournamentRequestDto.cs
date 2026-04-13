using ChessTournaments.Modules.TournamentRequests.Domain.Enums;

namespace ChessTournaments.Modules.TournamentRequests.Application.Abstractions;

public class TournamentRequestDto
{
    public Guid Id { get; set; }
    public Guid TournamentId { get; set; }
    public string RequestedBy { get; set; } = string.Empty;
    public RequestStatus Status { get; set; }
    public string? ReviewedBy { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public string? RejectionReason { get; set; }
    public DateTime CreatedAt { get; set; }
}
