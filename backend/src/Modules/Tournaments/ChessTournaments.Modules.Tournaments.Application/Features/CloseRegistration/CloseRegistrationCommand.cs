using CSharpFunctionalExtensions;
using MediatR;

namespace ChessTournaments.Modules.Tournaments.Application.Features.CloseRegistration;

public record CloseRegistrationCommand(Guid TournamentId) : IRequest<Result>;
