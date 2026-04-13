# Chess Tournaments - Architecture Documentation

## Table of Contents
1. [Overview](#overview)
2. [Architectural Patterns](#architectural-patterns)
3. [Module Structure](#module-structure)
4. [Integration Events](#integration-events)
5. [Application Flow](#application-flow)
6. [Data Flow](#data-flow)
7. [Technology Stack](#technology-stack)

## Overview

Chess Tournaments is built using a **Modular Monolith Architecture** with clean separation of concerns across four main modules:
- **Tournaments Module**: Tournament management and rounds
- **Matches Module**: Match scheduling and results
- **Players Module**: Player profiles, statistics, and achievements
- **Tournament Requests Module**: Player participation requests

### Key Architectural Principles

- **Loose Coupling**: Modules communicate through integration events
- **High Cohesion**: Each module owns its domain and data
- **Explicit Dependencies**: No direct references between modules
- **Event-Driven**: Asynchronous communication via MediatR
- **Domain-Driven Design**: Rich domain models with business logic

## Architectural Patterns

### 1. Modular Monolith

The application is structured as a single deployable unit (monolith) but internally organized into independent modules.

**Benefits:**
- ✅ Simpler deployment than microservices
- ✅ Easier to develop and test
- ✅ Can evolve to microservices if needed
- ✅ Maintains clear boundaries between modules

**Module Boundaries:**
```
┌─────────────────────────────────────────────────┐
│              ChessTournaments.API               │
├─────────────────────────────────────────────────┤
│  ┌──────────┐  ┌──────────┐  ┌──────────┐     │
│  │Tournament│  │ Matches  │  │ Players  │     │
│  │  Module  │  │  Module  │  │  Module  │     │
│  └────┬─────┘  └────┬─────┘  └────┬─────┘     │
│       │             │              │            │
│       └─────────────┼──────────────┘            │
│          Integration Events (MediatR)           │
└─────────────────────────────────────────────────┘
```

### 2. CQRS (Command Query Responsibility Segregation)

Separates read (queries) and write (commands) operations using MediatR.

**Commands** (Write Operations):
- `CreateTournamentCommand`
- `RecordMatchResultCommand`
- `UpdatePlayerProfileCommand`

**Queries** (Read Operations):
- `GetTournamentByIdQuery`
- `SearchMatchesQuery`
- `GetPlayerAchievementsQuery`

**Example Flow:**
```csharp
// Command Handler
public class RecordMatchResultCommandHandler
    : IRequestHandler<RecordMatchResultCommand, Result>
{
    public async Task<Result> Handle(
        RecordMatchResultCommand command,
        CancellationToken cancellationToken)
    {
        // 1. Get match
        var match = await _repository.GetByIdAsync(command.MatchId);

        // 2. Update domain model
        match.RecordResult(command.Result, command.Moves);

        // 3. Persist changes
        await _repository.SaveChangesAsync();

        return Result.Success();
    }
}
```

### 3. Domain-Driven Design (DDD)

Each module follows DDD tactical patterns:

- **Entities**: Objects with identity (Tournament, Match, Player)
- **Value Objects**: Immutable objects without identity (TournamentSettings)
- **Aggregates**: Transaction boundaries (Tournament is aggregate root)
- **Domain Events**: Capture business events (MatchCompletedDomainEvent)
- **Repositories**: Data access abstraction
- **Domain Services**: Stateless operations (RatingCalculationService)

### 4. Clean Architecture Layers

Each module is organized into four layers:

```
┌─────────────────────────────────────┐
│          API Layer                  │
│  (HTTP Endpoints, DTOs)             │
├─────────────────────────────────────┤
│      Application Layer              │
│  (Commands, Queries, Handlers)      │
├─────────────────────────────────────┤
│        Domain Layer                 │
│  (Entities, Value Objects, Events)  │
├─────────────────────────────────────┤
│    Infrastructure Layer             │
│  (DbContext, Repositories, EF)      │
└─────────────────────────────────────┘
```

**Dependency Rule**: Inner layers don't depend on outer layers.

## Module Structure

### Tournaments Module

**Responsibilities:**
- Create and manage tournaments (Swiss, Round Robin)
- Manage tournament rounds
- Track tournament state (Draft, Registration, InProgress, Completed)
- Publish tournament completion events

**Key Entities:**
- `Tournament` (Aggregate Root)
- `Round`
- `TournamentPlayer`
- `TournamentSettings`

**Integration Events Published:**
- `TournamentCompletedIntegrationEvent` - When tournament finishes, contains top 3 winners

**Integration Events Consumed:**
- `MatchCreatedIntegrationEvent` - Updates match count in rounds
- `MatchCompletedIntegrationEvent` - Updates tournament standings
- `TournamentParticipationApprovedIntegrationEvent` - Adds player to tournament

### Matches Module

**Responsibilities:**
- Create and schedule matches
- Record match results (WhiteWin, BlackWin, Draw, Forfeit)
- Manage match moves and metadata
- Tag management for matches

**Key Entities:**
- `Match` (Aggregate Root)
- `MatchTag`

**Integration Events Published:**
- `MatchCreatedIntegrationEvent` - When match is scheduled
- `MatchCompletedIntegrationEvent` - When match result is recorded (contains player IDs and result)

**Integration Events Consumed:**
- `CreateMatchRequestedIntegrationEvent` - Creates match from tournament round

### Players Module

**Responsibilities:**
- Manage player profiles and ratings
- Track player statistics (wins, losses, draws, rating)
- Calculate Elo ratings after each match
- Store tournament achievements (1st, 2nd, 3rd place)

**Key Entities:**
- `Player` (Aggregate Root)
- `Achievement`

**Integration Events Consumed:**
- `MatchCompletedIntegrationEvent` - Updates player stats and recalculates Elo ratings
- `TournamentCompletedIntegrationEvent` - Records achievements for top 3 players

**Elo Rating Calculation:**
```csharp
private static (int, int) CalculateEloRatings(
    int whiteRating, int blackRating, int result)
{
    const int K = 32; // K-factor for active players

    // Expected score = 1 / (1 + 10^((OpponentRating - PlayerRating) / 400))
    var whiteExpected = 1.0 / (1.0 + Math.Pow(10, (blackRating - whiteRating) / 400.0));
    var blackExpected = 1.0 - whiteExpected;

    // Actual score: 1.0 (win), 0.5 (draw), 0.0 (loss)
    var (whiteActual, blackActual) = result switch {
        1 => (1.0, 0.0), // White wins
        2 => (0.0, 1.0), // Black wins
        3 => (0.5, 0.5), // Draw
        _ => return (whiteRating, blackRating)
    };

    // New Rating = Old Rating + K * (Actual - Expected)
    var whiteNew = (int)Math.Round(whiteRating + K * (whiteActual - whiteExpected));
    var blackNew = (int)Math.Round(blackRating + K * (blackActual - blackExpected));

    return (whiteNew, blackNew);
}
```

### Tournament Requests Module

**Responsibilities:**
- Handle player tournament participation requests
- Admin approval/rejection workflow
- Track request status (Pending, Approved, Rejected)

**Key Entities:**
- `TournamentRequest` (Aggregate Root)

**Integration Events Published:**
- `TournamentParticipationApprovedIntegrationEvent` - When admin approves request

## Integration Events

Integration events enable **asynchronous, loosely-coupled communication** between modules.

### Event Flow Architecture

```
┌──────────────────────────────────────────────────────────┐
│                     Event Publisher                       │
│  Domain Event → Integration Event → MediatR Notification │
└────────────────────────┬─────────────────────────────────┘
                         │
                         ▼
┌──────────────────────────────────────────────────────────┐
│              MediatR (In-Process Event Bus)              │
│         IIntegrationEventPublisher (MediatR-based)       │
└────────────┬──────────────┬──────────────┬───────────────┘
             │              │              │
             ▼              ▼              ▼
    ┌────────────┐  ┌────────────┐  ┌────────────┐
    │  Handler 1 │  │  Handler 2 │  │  Handler 3 │
    │ (Module A) │  │ (Module B) │  │ (Module C) │
    └────────────┘  └────────────┘  └────────────┘
```

### Key Integration Events

#### 1. MatchCompletedIntegrationEvent

**Published By**: Matches Module
**Consumed By**: Players Module, Tournaments Module

```csharp
public record MatchCompletedIntegrationEvent(
    Guid MatchId,
    Guid TournamentId,
    Guid RoundId,
    string WhitePlayerId,  // Auth0 User ID
    string BlackPlayerId,  // Auth0 User ID
    int Result,            // 0=Ongoing, 1=WhiteWin, 2=BlackWin, 3=Draw, 4=Forfeit
    DateTime CompletedAt
) : IIntegrationEvent;
```

**Handler in Players Module:**
```csharp
public class MatchCompletedIntegrationEventHandler
    : INotificationHandler<MatchCompletedIntegrationEvent>
{
    public async Task Handle(MatchCompletedIntegrationEvent notification)
    {
        // 1. Get both players
        var whitePlayer = await _repo.GetByUserIdAsync(notification.WhitePlayerId);
        var blackPlayer = await _repo.GetByUserIdAsync(notification.BlackPlayerId);

        // 2. Calculate new Elo ratings
        var (whiteNewRating, blackNewRating) = CalculateEloRatings(
            whitePlayer.Rating, blackPlayer.Rating, notification.Result);

        // 3. Update player statistics
        switch (notification.Result) {
            case 1: // White wins
                whitePlayer.RecordGameResult(won: true, draw: false);
                blackPlayer.RecordGameResult(won: false, draw: false);
                break;
            case 2: // Black wins
                whitePlayer.RecordGameResult(won: false, draw: false);
                blackPlayer.RecordGameResult(won: true, draw: false);
                break;
            case 3: // Draw
                whitePlayer.RecordGameResult(won: false, draw: true);
                blackPlayer.RecordGameResult(won: false, draw: true);
                break;
        }

        // 4. Update ratings
        whitePlayer.UpdateRating(whiteNewRating);
        blackPlayer.UpdateRating(blackNewRating);

        // 5. Save changes
        await _repo.SaveChangesAsync();
    }
}
```

#### 2. TournamentCompletedIntegrationEvent

**Published By**: Tournaments Module
**Consumed By**: Players Module

```csharp
public record TournamentCompletedIntegrationEvent(
    Guid TournamentId,
    string TournamentName,
    DateTime CompletedAt,
    List<WinnerInfo> TopWinners // Top 3 winners
) : IIntegrationEvent;

public record WinnerInfo(
    int Position,      // 1, 2, or 3
    string PlayerId,   // Auth0 User ID
    string PlayerName,
    decimal Score
);
```

**Handler in Players Module:**
```csharp
public class TournamentCompletedIntegrationEventHandler
    : INotificationHandler<TournamentCompletedIntegrationEvent>
{
    public async Task Handle(TournamentCompletedIntegrationEvent notification)
    {
        // Process each winner concurrently
        await Task.WhenAll(
            notification.TopWinners.Select(winner =>
                ProcessWinnerAsync(notification, winner)
            )
        );
    }

    private async Task ProcessWinnerAsync(WinnerInfo winner)
    {
        var player = await _playerRepo.GetByUserIdAsync(winner.PlayerId);

        // 1. Update tournament participation stats
        player.RecordTournamentParticipation(won: winner.Position == 1);

        // 2. Create achievement record (if not exists)
        var achievement = Achievement.Create(
            player.Id,
            notification.TournamentId,
            notification.TournamentName,
            winner.Position,
            winner.Score,
            notification.CompletedAt
        );

        await _achievementRepo.AddAsync(achievement);
        await _repo.SaveChangesAsync();
    }
}
```

#### 3. TournamentParticipationApprovedIntegrationEvent

**Published By**: Tournament Requests Module
**Consumed By**: Tournaments Module

```csharp
public record TournamentParticipationApprovedIntegrationEvent(
    Guid TournamentId,
    string PlayerId,
    string PlayerName,
    int Rating
) : IIntegrationEvent;
```

### Event Publishing Pattern

Events are published using the **Domain Events → Integration Events** pattern:

```csharp
// 1. Domain Event (in Domain Layer)
public record MatchCompletedDomainEvent(
    Guid TournamentId,
    Guid RoundId,
    Guid MatchId,
    string WhitePlayerId,
    string BlackPlayerId,
    GameResult Result
) : IDomainEvent;

// 2. Raise Domain Event in Entity
public class Match : Entity
{
    public Result RecordResult(GameResult result, string? moves = null)
    {
        // Business logic...
        Result = result;
        IsCompleted = true;
        CompletedAt = DateTime.UtcNow;

        // Raise domain event
        AddDomainEvent(new MatchCompletedDomainEvent(
            TournamentId, RoundId, Id,
            WhitePlayerId, BlackPlayerId, result
        ));

        return Result.Success();
    }
}

// 3. Domain Event Handler publishes Integration Event
public class MatchCompletedDomainEventHandler
    : INotificationHandler<MatchCompletedDomainEvent>
{
    private readonly IIntegrationEventPublisher _eventPublisher;

    public async Task Handle(MatchCompletedDomainEvent domainEvent)
    {
        // Transform domain event to integration event
        var integrationEvent = new MatchCompletedIntegrationEvent(
            domainEvent.MatchId,
            domainEvent.TournamentId,
            domainEvent.RoundId,
            domainEvent.WhitePlayerId,
            domainEvent.BlackPlayerId,
            (int)domainEvent.Result,
            DateTime.UtcNow
        );

        // Publish to other modules
        await _eventPublisher.PublishAsync(integrationEvent);
    }
}
```

## Application Flow

### Complete Tournament Lifecycle

```
┌──────────────────────────────────────────────────────────────────┐
│                    1. Tournament Creation                        │
│  Admin → API → CreateTournamentCommand → Tournament Module      │
└────────────────────────┬─────────────────────────────────────────┘
                         │
                         ▼
┌──────────────────────────────────────────────────────────────────┐
│                  2. Player Registration                          │
│  Player → API → CreateTournamentRequestCommand                  │
│       → Tournament Requests Module                               │
└────────────────────────┬─────────────────────────────────────────┘
                         │
                         ▼
┌──────────────────────────────────────────────────────────────────┐
│               3. Admin Approval                                  │
│  Admin → API → ApproveTournamentRequestCommand                  │
│       → Publishes TournamentParticipationApprovedEvent           │
│       → Tournament Module adds player to tournament              │
└────────────────────────┬─────────────────────────────────────────┘
                         │
                         ▼
┌──────────────────────────────────────────────────────────────────┐
│              4. Round & Match Creation                           │
│  Admin → API → CreateRoundCommand → Tournament Module           │
│       → Publishes CreateMatchRequestedEvent                      │
│       → Matches Module creates matches                           │
│       → Publishes MatchCreatedEvent                              │
│       → Tournament Module updates match count                    │
└────────────────────────┬─────────────────────────────────────────┘
                         │
                         ▼
┌──────────────────────────────────────────────────────────────────┐
│               5. Record Match Results                            │
│  Player/Admin → API → RecordMatchResultCommand                  │
│       → Matches Module updates match                             │
│       → Publishes MatchCompletedEvent                            │
│          ├─→ Players Module: Update stats & Elo ratings          │
│          └─→ Tournament Module: Update standings                 │
└────────────────────────┬─────────────────────────────────────────┘
                         │
                         ▼
┌──────────────────────────────────────────────────────────────────┐
│             6. Tournament Completion                             │
│  System → CompleteTournamentCommand → Tournament Module         │
│       → Publishes TournamentCompletedEvent                       │
│       → Players Module:                                          │
│          ├─→ Updates tournament participation count              │
│          └─→ Creates achievements for top 3 players              │
└──────────────────────────────────────────────────────────────────┘
```

### Player Statistics Update Flow

```
Match Result Recorded
        │
        ▼
┌─────────────────────────────────┐
│  MatchCompletedDomainEvent      │
│  (Within Matches Module)        │
└──────────┬──────────────────────┘
           │
           ▼
┌─────────────────────────────────┐
│  Domain Event Handler           │
│  Publishes Integration Event    │
└──────────┬──────────────────────┘
           │
           ▼
┌──────────────────────────────────────────────────────────┐
│        MatchCompletedIntegrationEvent (MediatR)          │
└─────┬────────────────────────────────────────┬───────────┘
      │                                        │
      ▼                                        ▼
┌─────────────────────┐            ┌─────────────────────┐
│  Players Module     │            │ Tournaments Module  │
│  Handler            │            │  Handler            │
├─────────────────────┤            ├─────────────────────┤
│ 1. Get Players      │            │ 1. Get Tournament   │
│ 2. Calculate Elo    │            │ 2. Update Standings │
│ 3. Update Stats:    │            │ 3. Check Round      │
│    - Games Played   │            │    Completion       │
│    - Wins/Losses    │            └─────────────────────┘
│    - Draws          │
│    - Rating         │
│    - Peak Rating    │
│ 4. Save Changes     │
└─────────────────────┘
```

### Achievement Creation Flow

```
Tournament Completed
        │
        ▼
┌──────────────────────────────────┐
│  CompleteTournamentCommand       │
│  (Tournaments Module)            │
└──────────┬───────────────────────┘
           │
           ▼
┌──────────────────────────────────┐
│  Calculate Final Standings       │
│  Get Top 3 Winners               │
└──────────┬───────────────────────┘
           │
           ▼
┌──────────────────────────────────────────────┐
│  TournamentCompletedIntegrationEvent         │
│  Contains: Top 3 Winners (Position, Score)   │
└──────────┬───────────────────────────────────┘
           │
           ▼
┌──────────────────────────────────┐
│  Players Module Handler          │
│  (Processes concurrently)        │
├──────────────────────────────────┤
│  For each winner (Top 3):        │
│  1. Get Player by UserId         │
│  2. RecordTournamentParticipation│
│     (won = true for 1st place)   │
│  3. Create Achievement:          │
│     - Tournament ID & Name       │
│     - Position (1st 🥇, 2nd 🥈, 3rd 🥉)│
│     - Score                      │
│     - Achieved Date              │
│  4. Save to DB                   │
└──────────────────────────────────┘
```

## Data Flow

### Frontend → Backend API Flow

```
┌─────────────────┐
│  Angular App    │
│  (Port 4200)    │
└────────┬────────┘
         │ HTTP Request
         │ (Bearer Token)
         ▼
┌─────────────────────────────────┐
│  ASP.NET Core API               │
│  (Port 5000)                    │
├─────────────────────────────────┤
│  1. Authentication Middleware   │
│     OpenIddict Validation       │
│  2. Authorization               │
│  3. Carter Endpoint Routing     │
└────────┬────────────────────────┘
         │
         ▼
┌─────────────────────────────────┐
│  Module API Endpoint            │
│  (e.g., GetMatchesEndpoint)     │
├─────────────────────────────────┤
│  1. Map Request → Query/Command │
│  2. Send via MediatR            │
│  3. Map Result → DTO            │
│  4. Return HTTP Response        │
└────────┬────────────────────────┘
         │
         ▼
┌─────────────────────────────────┐
│  Query/Command Handler          │
│  (Application Layer)            │
├─────────────────────────────────┤
│  1. Validate Input              │
│  2. Call Repository             │
│  3. Execute Business Logic      │
│  4. Publish Events (if needed)  │
│  5. Return Result               │
└────────┬────────────────────────┘
         │
         ▼
┌─────────────────────────────────┐
│  Repository                     │
│  (Infrastructure Layer)         │
├─────────────────────────────────┤
│  1. Query Database via EF Core  │
│  2. Map to Domain Entities      │
│  3. Return to Handler           │
└────────┬────────────────────────┘
         │
         ▼
┌─────────────────────────────────┐
│  SQL Server Database            │
│  - TournamentsDb                │
│  - MatchesDb                    │
│  - PlayersDb                    │
│  - TournamentRequestsDb         │
└─────────────────────────────────┘
```

### Database Schema per Module

Each module has its own database context and schema:

**Tournaments Module:**
- `Tournaments` table
- `Rounds` table
- `TournamentPlayers` table

**Matches Module:**
- `Matches` table
- `MatchTags` table

**Players Module:**
- `Players` table
- `Achievements` table

**Tournament Requests Module:**
- `TournamentRequests` table

## Technology Stack

### Backend Technologies

| Layer | Technology | Purpose |
|-------|------------|---------|
| **API** | ASP.NET Core 10.0 | Web framework |
| | Carter | Minimal API routing |
| | OpenIddict | OIDC authentication |
| **Application** | MediatR | CQRS pattern |
| | FluentValidation | Input validation |
| **Domain** | C# 12 | Domain models |
| | CSharpFunctionalExtensions | Result pattern |
| **Infrastructure** | Entity Framework Core 10.0 | ORM |
| | SQL Server 2022 | Database |
| **Logging** | Serilog | Structured logging |
| | Seq | Log aggregation |
| **Testing** | xUnit | Test framework |
| | FluentAssertions | Test assertions |
| | Moq | Mocking |
| | SpecFlow | BDD testing |

### Frontend Technologies

| Category | Technology |
|----------|------------|
| **Framework** | Angular 18 |
| **Language** | TypeScript |
| **State Management** | Signals |
| **HTTP** | HttpClient |
| **Routing** | Angular Router |
| **Authentication** | Auth0 Angular SDK |

### Infrastructure

| Service | Technology | Port |
|---------|------------|------|
| **API** | ASP.NET Core | 5000 |
| **Identity** | ASP.NET Core 8 | 5001 |
| **Frontend** | Angular | 4200 |
| **Database** | SQL Server 2022 | 1433 |
| **Logging** | Seq | 5341 |

## Best Practices

### 1. Integration Event Design

✅ **Do:**
- Keep events immutable (use `record` types)
- Include all necessary data in the event
- Use clear, business-meaningful names
- Version events for breaking changes
- Make events idempotent-safe

❌ **Don't:**
- Include entity references (use IDs)
- Make events too granular
- Couple events to implementation details
- Forget to handle event failures

### 2. Module Communication

✅ **Do:**
- Communicate only via integration events
- Use eventual consistency
- Handle event failures gracefully
- Make operations idempotent

❌ **Don't:**
- Direct database access across modules
- Direct service calls between modules
- Synchronous coupling
- Shared domain models

### 3. Domain Model Design

✅ **Do:**
- Encapsulate business logic in entities
- Use value objects for concepts without identity
- Validate invariants in domain
- Raise domain events for significant changes

❌ **Don't:**
- Anemic domain models (data bags)
- Public setters on entities
- Business logic in handlers
- Direct database updates

## Future Enhancements

Potential architectural improvements:

1. **Outbox Pattern**: Reliable event publishing with database transactions
2. **API Gateway**: Unified entry point for microservices migration
3. **Event Sourcing**: Audit trail for critical aggregates
4. **CQRS Read Models**: Optimized read databases
5. **Message Queue**: RabbitMQ/Azure Service Bus for distributed events
6. **Saga Pattern**: Distributed transactions across modules

---

*Last Updated: December 31, 2025*
