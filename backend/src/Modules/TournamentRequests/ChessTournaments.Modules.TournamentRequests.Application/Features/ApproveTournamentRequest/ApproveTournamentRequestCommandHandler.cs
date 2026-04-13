using ChessTournaments.Modules.TournamentRequests.Application.Abstractions;
using ChessTournaments.Modules.TournamentRequests.Domain.Common;
using ChessTournaments.Modules.TournamentRequests.Domain.TournamentRequests;
using ChessTournaments.Shared.IntegrationEvents;
using ChessTournaments.Shared.IntegrationEvents.Outbox;
using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace ChessTournaments.Modules.TournamentRequests.Application.Features.ApproveTournamentRequest;

public class ApproveTournamentRequestCommandHandler
    : IRequestHandler<ApproveTournamentRequestCommand, Result<TournamentRequestDto>>
{
    private readonly ITournamentRequestRepository _repository;
    private readonly IOutboxMessagePublisher _outboxPublisher;

    public ApproveTournamentRequestCommandHandler(
        ITournamentRequestRepository repository,
        [FromKeyedServices("TournamentRequestsDbContext")] IOutboxMessagePublisher outboxPublisher
    )
    {
        _repository = repository;
        _outboxPublisher = outboxPublisher;
    }

    public async Task<Result<TournamentRequestDto>> Handle(
        ApproveTournamentRequestCommand request,
        CancellationToken cancellationToken
    )
    {
        var tournamentRequest = await _repository.GetByIdAsync(
            request.RequestId,
            cancellationToken
        );

        if (tournamentRequest == null)
            return Result.Failure<TournamentRequestDto>(
                DomainErrors.TournamentRequest.NotFound.Message
            );

        // Approve the request
        var approveResult = tournamentRequest.Approve(request.AdminId);

        if (approveResult.IsFailure)
            return Result.Failure<TournamentRequestDto>(approveResult.Error);

        await _repository.UpdateAsync(tournamentRequest, cancellationToken);

        // Publish integration event to outbox (will be saved in same transaction)
        var integrationEvent = new TournamentParticipationApprovedIntegrationEvent(
            tournamentRequest.TournamentId,
            tournamentRequest.RequestedBy,
            tournamentRequest.Id
        );

        await _outboxPublisher.PublishAsync(integrationEvent, cancellationToken);

        // Save both domain changes and outbox message in single transaction
        await _repository.SaveChangesAsync(cancellationToken);

        return Result.Success(
            new TournamentRequestDto
            {
                Id = tournamentRequest.Id,
                TournamentId = tournamentRequest.TournamentId,
                RequestedBy = tournamentRequest.RequestedBy,
                Status = tournamentRequest.Status,
                ReviewedBy = tournamentRequest.ReviewedBy,
                ReviewedAt = tournamentRequest.ReviewedAt,
                CreatedAt = tournamentRequest.CreatedAt,
            }
        );
    }
}
