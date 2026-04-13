using ChessTournaments.Modules.Tournaments.Domain.Common;
using ChessTournaments.Modules.Tournaments.Domain.Enums;
using ChessTournaments.Modules.Tournaments.Domain.Rounds;
using ChessTournaments.Modules.Tournaments.Domain.TournamentPlayers;
using ChessTournaments.Shared.Domain.Enums;
using CSharpFunctionalExtensions;
using BaseEntity = ChessTournaments.Shared.Domain.Entities.Entity;

namespace ChessTournaments.Modules.Tournaments.Domain.Tournaments;

public class Tournament : BaseEntity
{
    private readonly List<TournamentPlayer> _players = [];
    private readonly List<Round> _rounds = [];

    public string Name { get; private set; }
    public string Description { get; private set; }
    public DateTime StartDate { get; private set; }
    public DateTime? EndDate { get; private set; }
    public TournamentStatus Status { get; private set; }
    public TournamentSettings Settings { get; private set; }
    public string OrganizerId { get; private set; }
    public string Location { get; private set; }

    public IReadOnlyCollection<TournamentPlayer> Players => _players.AsReadOnly();
    public IReadOnlyCollection<Round> Rounds => _rounds.AsReadOnly();

    private Tournament()
    {
        // For EF
    }

    private Tournament(
        string name,
        string description,
        DateTime startDate,
        TournamentSettings settings,
        string organizerId,
        string location
    )
    {
        Name = name;
        Description = description ?? string.Empty;
        StartDate = startDate;
        Status = TournamentStatus.Draft;
        Settings = settings;
        OrganizerId = organizerId;
        Location = location ?? string.Empty;

        AddDomainEvent(new TournamentCreatedDomainEvent(Id, Name, organizerId));
    }

    public static Result<Tournament> Create(
        string name,
        string description,
        DateTime startDate,
        TournamentSettings settings,
        string organizerId,
        string location
    )
    {
        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure<Tournament>(DomainErrors.Tournament.NameRequired.Message);

        if (startDate < DateTime.UtcNow)
            return Result.Failure<Tournament>(DomainErrors.Tournament.StartDateInPast.Message);

        if (string.IsNullOrWhiteSpace(organizerId))
            return Result.Failure<Tournament>(DomainErrors.Tournament.OrganizerIdRequired.Message);

        if (settings == null)
            return Result.Failure<Tournament>(DomainErrors.Tournament.SettingsRequired.Message);

        var tournament = new Tournament(
            name,
            description,
            startDate,
            settings,
            organizerId,
            location
        );

        return Result.Success(tournament);
    }

    public Result UpdateDetails(string name, string description, string location)
    {
        if (Status != TournamentStatus.Draft && Status != TournamentStatus.Registration)
            return Result.Failure(DomainErrors.Tournament.CannotUpdateInProgress.Message);

        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure(DomainErrors.Tournament.NameRequired.Message);

        Name = name;
        Description = description ?? string.Empty;
        Location = location ?? string.Empty;
        MarkAsUpdated();

        return Result.Success();
    }

    public Result OpenRegistration()
    {
        if (Status != TournamentStatus.Draft)
            return Result.Failure("Tournament must be in draft status");

        Status = TournamentStatus.Registration;
        MarkAsUpdated();
        AddDomainEvent(new TournamentRegistrationOpenedDomainEvent(Id));

        return Result.Success();
    }

    public Result CloseRegistration()
    {
        if (Status != TournamentStatus.Registration)
            return Result.Failure(DomainErrors.Tournament.CannotCloseRegistration.Message);

        if (_players.Count < Settings.MinPlayers)
            return Result.Failure(DomainErrors.Tournament.InsufficientPlayers.Message);

        Status = TournamentStatus.InProgress;
        MarkAsUpdated();
        AddDomainEvent(new TournamentRegistrationClosedDomainEvent(Id, _players.Count));

        return Result.Success();
    }

    public Result RegisterPlayer(string playerId, string playerName, int? rating = null)
    {
        if (Status != TournamentStatus.Registration)
            return Result.Failure(DomainErrors.Tournament.RegistrationNotOpen.Message);

        if (_players.Count >= Settings.MaxPlayers)
            return Result.Failure(DomainErrors.Tournament.TournamentFull.Message);

        if (_players.Any(p => p.PlayerId == playerId))
            return Result.Failure(DomainErrors.Tournament.PlayerAlreadyRegistered.Message);

        var playerResult = TournamentPlayer.Create(Id, playerId, playerName, rating);
        if (playerResult.IsFailure)
            return Result.Failure(playerResult.Error);

        _players.Add(playerResult.Value);
        MarkAsUpdated();
        AddDomainEvent(new PlayerRegisteredDomainEvent(Id, playerId, playerName));

        return Result.Success();
    }

    public Result UnregisterPlayer(string playerId)
    {
        if (Status != TournamentStatus.Registration)
            return Result.Failure(DomainErrors.Tournament.RegistrationNotOpen.Message);

        var player = _players.FirstOrDefault(p => p.PlayerId == playerId);
        if (player == null)
            return Result.Failure("Player is not registered");

        _players.Remove(player);
        MarkAsUpdated();
        AddDomainEvent(new PlayerUnregisteredDomainEvent(Id, playerId));

        return Result.Success();
    }

    public Result<Round> CreateRound(int roundNumber)
    {
        if (Status != TournamentStatus.InProgress)
            return Result.Failure<Round>(DomainErrors.Tournament.CannotStartTournament.Message);

        if (_rounds.Any(r => r.RoundNumber == roundNumber))
            return Result.Failure<Round>($"Round {roundNumber} already exists");

        if (roundNumber > Settings.NumberOfRounds)
            return Result.Failure<Round>(
                $"Cannot create round {roundNumber}, maximum is {Settings.NumberOfRounds}"
            );

        var roundResult = Round.Create(Id, roundNumber);
        if (roundResult.IsFailure)
            return Result.Failure<Round>(roundResult.Error);

        _rounds.Add(roundResult.Value);
        MarkAsUpdated();
        AddDomainEvent(new RoundCreatedDomainEvent(Id, roundResult.Value.Id, roundNumber));

        return Result.Success(roundResult.Value);
    }

    public Result Complete()
    {
        if (Status != TournamentStatus.InProgress)
            return Result.Failure(DomainErrors.Tournament.CannotCompleteTournament.Message);

        Status = TournamentStatus.Completed;
        EndDate = DateTime.UtcNow;
        MarkAsUpdated();
        AddDomainEvent(new TournamentCompletedDomainEvent(Id));

        return Result.Success();
    }

    public Result Cancel()
    {
        if (Status == TournamentStatus.Completed)
            return Result.Failure(DomainErrors.Tournament.CannotCancelCompleted.Message);

        Status = TournamentStatus.Cancelled;
        MarkAsUpdated();
        AddDomainEvent(new TournamentCancelledDomainEvent(Id));

        return Result.Success();
    }
}
