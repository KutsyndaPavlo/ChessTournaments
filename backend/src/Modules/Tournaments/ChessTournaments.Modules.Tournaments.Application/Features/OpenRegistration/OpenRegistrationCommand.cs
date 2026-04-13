using CSharpFunctionalExtensions;
using MediatR;

namespace ChessTournaments.Modules.Tournaments.Application.Features.OpenRegistration;

public record OpenRegistrationCommand(Guid TournamentId) : IRequest<Result>;
