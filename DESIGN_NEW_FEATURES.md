# Design Document: Tournament Requests, Pairings, and Match Results

## Overview
This document outlines the design for adding the following features to the Chess Tournaments application:
1. **Tournament Requests Module** - A separate module for managing tournament creation requests
2. **Admin Approval/Rejection** - Admin workflow for approving/rejecting tournament requests
3. **Pairings Enhancement** - Enhanced pairing generation and management
4. **Match Results** - Recording and displaying match results

## Architecture Decisions

### Module Structure

We will create a **new TournamentRequests module** alongside the existing Tournaments module:

```
backend/src/Modules/
├── Tournaments/              # Existing - manages approved/active tournaments
│   ├── Domain/
│   ├── Application/
│   ├── Infrastructure/
│   └── API/
└── TournamentRequests/       # NEW - manages tournament creation requests
    ├── Domain/
    ├── Application/
    ├── Infrastructure/
    └── API/
```

**Rationale**:
- **Separation of Concerns**: Tournament requests have different lifecycle, business rules, and access patterns
- **Single Responsibility**: Tournaments module focuses on running tournaments; Requests module focuses on approval workflow
- **Independent Scaling**: If request volume grows, this module can be scaled separately
- **Clear Boundaries**: Approved requests create Tournaments via domain events/integration

---

## 1. Tournament Requests Module

### Domain Layer

#### Entities

**TournamentRequest** (Aggregate Root)
```csharp
public class TournamentRequest : Entity
{
    public string Name { get; private set; }
    public string Description { get; private set; }
    public DateTime StartDate { get; private set; }
    public TournamentSettings Settings { get; private set; }
    public string RequesterId { get; private set; }  // User who requested
    public string RequesterName { get; private set; }
    public string RequesterEmail { get; private set; }
    public string Location { get; private set; }
    public RequestStatus Status { get; private set; }  // NEW
    public string? RejectionReason { get; private set; }
    public string? ReviewedBy { get; private set; }   // Admin who reviewed
    public DateTime? ReviewedAt { get; private set; }

    // Factory method
    public static Result<TournamentRequest> Create(...)

    // Business methods
    public Result Approve(string adminId)
    public Result Reject(string adminId, string reason)
    public Result Cancel()  // Requester can cancel pending requests
}
```

**RequestStatus** (Enum)
```csharp
public enum RequestStatus
{
    Pending,      // Initial state
    Approved,     // Admin approved - tournament created
    Rejected,     // Admin rejected with reason
    Cancelled     // Requester cancelled before review
}
```

#### Domain Events
```csharp
public record TournamentRequestCreatedDomainEvent(Guid RequestId) : DomainEventBase;
public record TournamentRequestApprovedDomainEvent(Guid RequestId, string AdminId) : DomainEventBase;
public record TournamentRequestRejectedDomainEvent(Guid RequestId, string AdminId, string Reason) : DomainEventBase;
```

#### Domain Errors
```csharp
public static class DomainErrors
{
    public static class TournamentRequest
    {
        public static readonly Error NotFound = new("TournamentRequest.NotFound", "Tournament request not found");
        public static readonly Error AlreadyReviewed = new("TournamentRequest.AlreadyReviewed", "Request has already been reviewed");
        public static readonly Error CannotCancelReviewed = new("TournamentRequest.CannotCancelReviewed", "Cannot cancel a reviewed request");
        public static readonly Error StartDateInPast = new("TournamentRequest.StartDateInPast", "Start date must be in the future");
        // ... more errors
    }
}
```

### Application Layer

#### Commands/Queries

**Commands**:
- `CreateTournamentRequestCommand` - Any authenticated user
- `ApproveTournamentRequestCommand` - Admin only
- `RejectTournamentRequestCommand` - Admin only
- `CancelTournamentRequestCommand` - Requester only

**Queries**:
- `GetAllTournamentRequestsQuery` - Admin only (all requests)
- `GetMyTournamentRequestsQuery` - Authenticated user (their requests)
- `GetTournamentRequestByIdQuery` - Admin or requester
- `GetPendingTournamentRequestsQuery` - Admin only (for review queue)

#### DTOs
```csharp
public record TournamentRequestDto
{
    public Guid Id { get; init; }
    public string Name { get; init; }
    public string Description { get; init; }
    public DateTime StartDate { get; init; }
    public TournamentSettingsDto Settings { get; init; }
    public string RequesterId { get; init; }
    public string RequesterName { get; init; }
    public string RequesterEmail { get; init; }
    public string Location { get; init; }
    public RequestStatus Status { get; init; }
    public string? RejectionReason { get; init; }
    public string? ReviewedBy { get; init; }
    public DateTime? ReviewedAt { get; init; }
    public DateTime CreatedAt { get; init; }
}
```

#### Handlers

**ApproveTournamentRequestCommandHandler**:
```csharp
public async Task<Result> Handle(ApproveTournamentRequestCommand request, CancellationToken ct)
{
    // 1. Get request
    var tournamentRequest = await _requestRepository.GetByIdAsync(request.RequestId, ct);
    if (tournamentRequest == null)
        return Result.Failure(DomainErrors.TournamentRequest.NotFound.Message);

    // 2. Approve request
    var approveResult = tournamentRequest.Approve(request.AdminId);
    if (approveResult.IsFailure)
        return approveResult;

    // 3. Create actual tournament
    var tournamentResult = Tournament.Create(
        tournamentRequest.Name,
        tournamentRequest.Description,
        tournamentRequest.StartDate,
        tournamentRequest.Settings,
        tournamentRequest.RequesterId,  // Original requester becomes organizer
        tournamentRequest.Location
    );

    if (tournamentResult.IsFailure)
        return Result.Failure(tournamentResult.Error);

    // 4. Save both
    await _tournamentRepository.AddAsync(tournamentResult.Value, ct);
    await _requestRepository.UpdateAsync(tournamentRequest, ct);
    await _requestRepository.SaveChangesAsync(ct);

    return Result.Success();
}
```

### Infrastructure Layer

**Database Context**:
```csharp
public class TournamentRequestsDbContext : DbContext
{
    public DbSet<TournamentRequest> TournamentRequests { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("TournamentRequests");
        // Apply configurations
    }
}
```

**Repository**:
```csharp
public interface ITournamentRequestRepository
{
    Task<TournamentRequest?> GetByIdAsync(Guid id, CancellationToken ct);
    Task<List<TournamentRequest>> GetAllAsync(CancellationToken ct);
    Task<List<TournamentRequest>> GetByRequesterIdAsync(string requesterId, CancellationToken ct);
    Task<List<TournamentRequest>> GetPendingAsync(CancellationToken ct);
    Task AddAsync(TournamentRequest request, CancellationToken ct);
    Task UpdateAsync(TournamentRequest request, CancellationToken ct);
    Task SaveChangesAsync(CancellationToken ct);
}
```

### API Layer

**Endpoints**:
```
POST   /api/tournament-requests                    # Create request (authenticated)
GET    /api/tournament-requests                    # Get all (admin) or my requests (user)
GET    /api/tournament-requests/pending            # Get pending (admin)
GET    /api/tournament-requests/{id}               # Get by ID
POST   /api/tournament-requests/{id}/approve       # Approve (admin)
POST   /api/tournament-requests/{id}/reject        # Reject (admin)
DELETE /api/tournament-requests/{id}               # Cancel (requester)
```

**Authorization**:
- Create: Requires authentication
- Approve/Reject: Requires AdminPolicy
- Get all/pending: Requires AdminPolicy
- Get my requests: Requires authentication
- Cancel: Requires authentication + ownership check

---

## 2. Pairings Enhancement (Tournaments Module)

### Current State
- `Round` entity exists with collection of `Match` entities
- Basic match creation exists but no pairing logic

### Enhancements Needed

#### Domain Layer

**Match Entity** (enhance existing):
```csharp
public class Match : Entity
{
    public Guid RoundId { get; private set; }
    public string WhitePlayerId { get; private set; }
    public string? BlackPlayerId { get; private set; }  // Null for BYE
    public int BoardNumber { get; private set; }        // Position in pairing list
    public GameResult Result { get; private set; }

    // NEW: Enhanced result recording
    public Result RecordResult(GameResult result, string recordedBy)
    {
        if (Status == MatchStatus.Completed)
            return Result.Failure(DomainErrors.Match.AlreadyCompleted.Message);

        Result = result;
        Status = MatchStatus.Completed;
        MarkAsUpdated();

        // Raise domain event
        RaiseDomainEvent(new MatchResultRecordedDomainEvent(Id, result));
        return Result.Success();
    }

    public bool IsBye => BlackPlayerId == null;
}
```

**Round Entity** (enhance existing):
```csharp
public class Round : Entity
{
    private readonly List<Match> _matches = new();
    public IReadOnlyList<Match> Matches => _matches.AsReadOnly();

    // NEW: Pairing generation
    public static Result<Round> CreateWithPairings(
        int roundNumber,
        List<TournamentPlayer> players,
        TournamentFormat format)
    {
        var round = new Round(roundNumber);

        var pairings = format switch
        {
            TournamentFormat.Swiss => GenerateSwissPairings(players, roundNumber),
            TournamentFormat.RoundRobin => GenerateRoundRobinPairings(players, roundNumber),
            _ => throw new NotSupportedException($"Format {format} not supported")
        };

        round._matches.AddRange(pairings);
        return Result.Success(round);
    }

    private static List<Match> GenerateSwissPairings(List<TournamentPlayer> players, int roundNumber)
    {
        // Swiss pairing algorithm:
        // 1. Group by score
        // 2. Within each group, pair highest vs lowest rating
        // 3. Avoid repeat pairings if possible
        // 4. Handle odd number with BYE

        var sorted = players.OrderByDescending(p => p.TotalScore)
                            .ThenByDescending(p => p.Rating ?? 0)
                            .ToList();

        // Implementation details...
    }

    public Result StartRound()
    {
        if (Status != RoundStatus.NotStarted)
            return Result.Failure(DomainErrors.Round.AlreadyStarted.Message);

        Status = RoundStatus.InProgress;
        MarkAsUpdated();
        RaiseDomainEvent(new RoundStartedDomainEvent(Id));
        return Result.Success();
    }

    public Result CompleteRound()
    {
        if (_matches.Any(m => m.Status != MatchStatus.Completed))
            return Result.Failure(DomainErrors.Round.IncompleteMatches.Message);

        Status = RoundStatus.Completed;
        MarkAsUpdated();
        RaiseDomainEvent(new RoundCompletedDomainEvent(Id));
        return Result.Success();
    }
}
```

**RoundStatus** (new enum):
```csharp
public enum RoundStatus
{
    NotStarted,
    InProgress,
    Completed
}
```

**MatchStatus** (new enum):
```csharp
public enum MatchStatus
{
    Scheduled,
    InProgress,
    Completed
}
```

### Application Layer

**New Commands**:
- `GenerateNextRoundCommand` - Creates round with automatic pairings
- `RecordMatchResultCommand` - Records result for a match
- `StartRoundCommand` - Marks round as started
- `CompleteRoundCommand` - Marks round as completed

**New Queries**:
- `GetRoundPairingsQuery` - Gets pairings for a specific round
- `GetTournamentStandingsQuery` - Gets current standings sorted by score

**Handlers**:
```csharp
public class GenerateNextRoundCommandHandler : IRequestHandler<GenerateNextRoundCommand, Result<RoundDto>>
{
    public async Task<Result<RoundDto>> Handle(GenerateNextRoundCommand request, CancellationToken ct)
    {
        var tournament = await _repository.GetByIdAsync(request.TournamentId, ct);
        if (tournament == null)
            return Result.Failure<RoundDto>(DomainErrors.Tournament.NotFound.Message);

        // Generate round with pairings
        var roundNumber = tournament.Rounds.Count + 1;
        var roundResult = Round.CreateWithPairings(
            roundNumber,
            tournament.Players.ToList(),
            tournament.Settings.Format
        );

        if (roundResult.IsFailure)
            return Result.Failure<RoundDto>(roundResult.Error);

        tournament.AddRound(roundResult.Value);
        await _repository.UpdateAsync(tournament, ct);
        await _repository.SaveChangesAsync(ct);

        // Map to DTO and return
        return Result.Success(MapToDto(roundResult.Value));
    }
}
```

### API Layer

**New Endpoints**:
```
POST   /api/tournaments/{id}/rounds                 # Generate next round (admin)
POST   /api/tournaments/{id}/rounds/{roundId}/start # Start round (admin)
POST   /api/tournaments/{id}/rounds/{roundId}/complete # Complete round (admin)
POST   /api/tournaments/{id}/matches/{matchId}/result  # Record result (admin)
GET    /api/tournaments/{id}/rounds/{roundId}/pairings # Get pairings
GET    /api/tournaments/{id}/standings               # Get current standings
```

---

## 3. Frontend Implementation

### New Components

#### Tournament Requests Module
```
src/app/tournament-requests/
├── tournament-request-list/
│   └── tournament-request-list.component.ts
├── tournament-request-create/
│   └── tournament-request-create.component.ts
├── tournament-request-detail/
│   └── tournament-request-detail.component.ts
├── admin-request-review/
│   └── admin-request-review.component.ts
└── services/
    └── tournament-request.service.ts
```

#### Enhanced Tournament Components
```
src/app/tournaments/
├── tournament-pairings/
│   └── tournament-pairings.component.ts     # NEW: Display pairings
├── tournament-results/
│   └── tournament-results.component.ts      # NEW: Record results
├── tournament-standings/
│   └── tournament-standings.component.ts    # NEW: Display standings
```

### Routes
```typescript
export const routes: Routes = [
  // ... existing routes

  // Tournament Requests (user)
  {
    path: 'tournament-requests',
    canActivate: [authGuard],
    children: [
      { path: '', component: TournamentRequestListComponent },
      { path: 'create', component: TournamentRequestCreateComponent },
      { path: ':id', component: TournamentRequestDetailComponent }
    ]
  },

  // Admin request review
  {
    path: 'admin',
    canActivate: [authGuard, adminGuard],  // NEW: admin guard
    children: [
      { path: 'requests', component: AdminRequestReviewComponent },
      { path: 'requests/:id', component: TournamentRequestDetailComponent }
    ]
  },

  // Enhanced tournament routes
  {
    path: 'tournaments/:id',
    canActivate: [authGuard],
    children: [
      { path: '', component: TournamentDetailComponent },
      { path: 'players', component: TournamentPlayersComponent },
      { path: 'rounds', component: TournamentRoundsComponent },
      { path: 'rounds/:roundId/pairings', component: TournamentPairingsComponent }, // NEW
      { path: 'rounds/:roundId/results', component: TournamentResultsComponent },   // NEW
      { path: 'standings', component: TournamentStandingsComponent }                // NEW
    ]
  }
];
```

### Services

**TournamentRequestService**:
```typescript
@Injectable({ providedIn: 'root' })
export class TournamentRequestService {
  private apiUrl = `${environment.apiUrl}/tournament-requests`;

  create(request: CreateTournamentRequest): Observable<TournamentRequestDto> { }
  getAll(): Observable<TournamentRequestDto[]> { }
  getMy(): Observable<TournamentRequestDto[]> { }
  getPending(): Observable<TournamentRequestDto[]> { }
  getById(id: string): Observable<TournamentRequestDto> { }
  approve(id: string): Observable<void> { }
  reject(id: string, reason: string): Observable<void> { }
  cancel(id: string): Observable<void> { }
}
```

**Enhanced TournamentService**:
```typescript
@Injectable({ providedIn: 'root' })
export class TournamentService {
  // ... existing methods

  // NEW: Pairings and results
  generateNextRound(tournamentId: string): Observable<RoundDto> { }
  startRound(tournamentId: string, roundId: string): Observable<void> { }
  completeRound(tournamentId: string, roundId: string): Observable<void> { }
  recordMatchResult(tournamentId: string, matchId: string, result: GameResult): Observable<void> { }
  getPairings(tournamentId: string, roundId: string): Observable<MatchDto[]> { }
  getStandings(tournamentId: string): Observable<PlayerStandingDto[]> { }
}
```

### UI Components

**AdminRequestReviewComponent**:
- List of pending requests with filters (pending/approved/rejected)
- Quick approve/reject actions
- Rejection reason modal
- Real-time updates with signals

**TournamentPairingsComponent**:
- Display pairing table with board numbers
- Show player names, ratings
- Indicate BYEs
- Links to record results

**TournamentResultsComponent**:
- Form to select match and enter result
- Dropdown for game result (WhiteWin/BlackWin/Draw/Forfeit)
- Validation
- Confirmation

**TournamentStandingsComponent**:
- Sortable table with:
  - Rank
  - Player name
  - Score
  - Rating (if available)
  - Tiebreak criteria (Buchholz, etc.)

---

## 4. Database Schema

### TournamentRequests Schema
```sql
CREATE SCHEMA TournamentRequests;

CREATE TABLE TournamentRequests.TournamentRequests (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    Name NVARCHAR(200) NOT NULL,
    Description NVARCHAR(MAX),
    StartDate DATETIME2 NOT NULL,
    RequesterId NVARCHAR(450) NOT NULL,
    RequesterName NVARCHAR(200) NOT NULL,
    RequesterEmail NVARCHAR(200) NOT NULL,
    Location NVARCHAR(500),
    Status INT NOT NULL,
    RejectionReason NVARCHAR(MAX),
    ReviewedBy NVARCHAR(450),
    ReviewedAt DATETIME2,
    CreatedAt DATETIME2 NOT NULL,
    UpdatedAt DATETIME2 NOT NULL,

    -- Settings stored as owned entity or JSON
    Format INT NOT NULL,
    TimeControl INT NOT NULL,
    TimeInMinutes INT NOT NULL,
    IncrementInSeconds INT NOT NULL,
    NumberOfRounds INT NOT NULL,
    MaxPlayers INT NOT NULL,
    MinPlayers INT NOT NULL,
    AllowByes BIT NOT NULL,
    EntryFee DECIMAL(18,2) NOT NULL,

    INDEX IX_Status (Status),
    INDEX IX_RequesterId (RequesterId),
    INDEX IX_CreatedAt (CreatedAt)
);
```

### Tournaments Schema Updates
```sql
-- Add new columns to existing tables
ALTER TABLE Tournaments.Rounds
ADD Status INT NOT NULL DEFAULT 0;  -- RoundStatus

ALTER TABLE Tournaments.Matches
ADD BoardNumber INT NOT NULL DEFAULT 0,
    Status INT NOT NULL DEFAULT 0;  -- MatchStatus
```

---

## 5. Security & Authorization

### New Guard

**admin.guard.ts**:
```typescript
export const adminGuard: CanActivateFn = () => {
  const authService = inject(AuthService);

  if (!authService.isAuthenticated()) {
    return false;
  }

  const roles = authService.getUserRoles();
  if (!roles.includes('Admin')) {
    // Redirect or show error
    return false;
  }

  return true;
};
```

### Backend Policies

All admin endpoints use `RequireAuthorization("AdminPolicy")`:
- Approve/Reject requests
- Generate rounds
- Record results
- Start/Complete rounds

User endpoints require authentication but check ownership:
```csharp
// In handler
if (request.RequesterId != currentUserId && !isAdmin)
    return Result.Failure("Not authorized");
```

---

## 6. Implementation Plan

### Phase 1: Backend - TournamentRequests Module (2-3 days)
1. Create module structure
2. Implement domain entities and errors
3. Implement application commands/queries/handlers
4. Implement infrastructure (DbContext, repository, migrations)
5. Implement API endpoints
6. Unit tests

### Phase 2: Backend - Pairings Enhancement (2-3 days)
1. Enhance Match and Round entities
2. Implement pairing algorithms (Swiss, Round Robin)
3. Create new commands/queries/handlers
4. Add new API endpoints
5. Unit tests

### Phase 3: Frontend - Tournament Requests (2-3 days)
1. Create service
2. Create components (list, create, detail, admin review)
3. Add routes and guards
4. Implement UI with Angular Material
5. Integration with backend

### Phase 4: Frontend - Pairings and Results (2-3 days)
1. Enhance tournament service
2. Create pairing/result/standing components
3. Add routes
4. Implement UI
5. Integration with backend

### Phase 5: Testing & Refinement (1-2 days)
1. Integration tests
2. E2E tests
3. Bug fixes
4. Documentation

---

## 7. Future Enhancements

- Email notifications for request status changes
- Batch approval of requests
- Request templates
- Advanced pairing options (color balance, rating restrictions)
- Automatic result import from PGN
- Live game tracking
- Tournament reports and analytics
