using ChessTournaments.Modules.Tournaments.Application.Features.CreateRoundWithPairings;
using ChessTournaments.Modules.Tournaments.Domain.Tournaments;
using MediatR;

namespace ChessTournaments.Modules.Tournaments.Application.DomainEventHandlers;

public class TournamentRegistrationClosedDomainEventHandler
    : INotificationHandler<TournamentRegistrationClosedDomainEvent>
{
    private readonly ISender _sender;

    public TournamentRegistrationClosedDomainEventHandler(ISender sender)
    {
        _sender = sender;
    }

    public async Task Handle(
        TournamentRegistrationClosedDomainEvent notification,
        CancellationToken cancellationToken
    )
    {
        // Create the first round when registration is closed
        var command = new CreateRoundWithPairingsCommand(notification.TournamentId);
        await _sender.Send(command, cancellationToken);
    }
}
