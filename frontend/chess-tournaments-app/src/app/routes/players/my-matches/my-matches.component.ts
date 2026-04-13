import { Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { AuthService } from '@app/auth/auth.service';
import { MatchesService } from '@app/infrastructure/api/services/matches.service';
import { TournamentMatchDto } from '@app/infrastructure/api/models/tournament-match-dto';

@Component({
  selector: 'app-my-matches',
  imports: [CommonModule, RouterModule],
  template: `
    <div class="my-matches-container">
      <div class="my-matches-header">
        <h1>My Matches</h1>
        <p class="subtitle">Your match history and results</p>
      </div>

      @if (isLoading()) {
        <div class="loading-container">
          <p>Loading matches...</p>
        </div>
      }

      @if (error()) {
        <div class="error-container">
          <p class="error-message">{{ error() }}</p>
        </div>
      }

      @if (!isLoading() && matches().length === 0) {
        <div class="empty-state">
          <div class="empty-icon">♟</div>
          <h2>No Matches Yet</h2>
          <p>Join a tournament to start playing matches!</p>
          <a routerLink="/tournaments" class="btn btn-primary">Browse Tournaments</a>
        </div>
      }

      @if (!isLoading() && matches().length > 0) {
        <div class="matches-stats">
          <h2>Statistics</h2>
          <div class="stats-grid">
            <div class="stat-card">
              <span class="stat-value">{{ getTotalMatches() }}</span>
              <span class="stat-label">Total Matches</span>
            </div>
            <div class="stat-card wins">
              <span class="stat-value">{{ getWins() }}</span>
              <span class="stat-label">Wins</span>
            </div>
            <div class="stat-card losses">
              <span class="stat-value">{{ getLosses() }}</span>
              <span class="stat-label">Losses</span>
            </div>
            <div class="stat-card draws">
              <span class="stat-value">{{ getDraws() }}</span>
              <span class="stat-label">Draws</span>
            </div>
          </div>
        </div>
        <div class="matches-list">
          @for (match of matches(); track match.id) {
            <div
              class="match-card"
              [class.my-win]="isMyWin(match)"
              [class.my-loss]="isMyLoss(match)"
            >
              <div class="match-header">
                <span class="match-date">{{ match.scheduledTime | date: 'medium' }}</span>
                <span class="match-result-badge" [class]="getResultClass(match)">
                  {{ getResultText(match) }}
                </span>
              </div>

              <div class="match-players">
                <div
                  class="player"
                  [class.winner]="isWhiteWinner(match)"
                  [class.me]="isMe(match.whitePlayerId)"
                >
                  <span class="player-color">⬜</span>
                  <span class="player-name">
                    {{ isMe(match.whitePlayerId) ? 'You' : 'White Player' }}
                    @if (isMe(match.whitePlayerId)) {
                      <span class="me-badge">ME</span>
                    }
                  </span>
                </div>

                <div class="vs-divider">vs</div>

                <div
                  class="player"
                  [class.winner]="isBlackWinner(match)"
                  [class.me]="isMe(match.blackPlayerId)"
                >
                  <span class="player-color">⬛</span>
                  <span class="player-name">
                    {{ isMe(match.blackPlayerId) ? 'You' : 'Black Player' }}
                    @if (isMe(match.blackPlayerId)) {
                      <span class="me-badge">ME</span>
                    }
                  </span>
                </div>
              </div>

              @if (match.tournamentId) {
                <div class="match-tournament">
                  <span class="tournament-label">Tournament:</span>
                  <a [routerLink]="['/tournaments', match.tournamentId]" class="tournament-link">
                    View Tournament →
                  </a>
                </div>
              }
            </div>
          }
        </div>
      }
    </div>
  `,
  styles: `
    .my-matches-container {
      max-width: 1200px;
      margin: 0 auto;
      padding: 2rem;
    }

    .my-matches-header {
      margin-bottom: 2rem;
      padding-bottom: 1.5rem;
      border-bottom: 2px solid rgba(184, 134, 11, 0.2);
    }

    .my-matches-header h1 {
      color: var(--chess-tournaments-heading);
      font-size: 2.5rem;
      font-weight: 700;
      margin: 0 0 0.5rem 0;
    }

    .subtitle {
      color: var(--chess-tournaments-muted);
      font-size: 1.1rem;
      margin: 0;
    }

    .loading-container,
    .error-container,
    .empty-state {
      text-align: center;
      padding: 3rem;
      background: var(--chess-tournaments-card-bg);
      border: 1px solid rgba(184, 134, 11, 0.3);
      border-radius: 15px;
    }

    .error-message {
      color: rgba(220, 38, 38, 0.9);
      font-size: 1.1rem;
      font-weight: 600;
    }

    .empty-icon {
      font-size: 4rem;
      margin-bottom: 1rem;
      opacity: 0.5;
    }

    .empty-state h2 {
      color: var(--chess-tournaments-heading);
      font-size: 1.75rem;
      margin: 0 0 1rem 0;
    }

    .empty-state p {
      color: var(--chess-tournaments-muted);
      font-size: 1.1rem;
      margin: 0 0 2rem 0;
    }

    .btn {
      padding: 0.75rem 2rem;
      font-size: 1rem;
      font-weight: 700;
      text-transform: uppercase;
      letter-spacing: 1px;
      border-radius: 12px;
      cursor: pointer;
      transition: all 0.3s ease;
      display: inline-block;
      text-decoration: none;
      border: 2px solid transparent;
    }

    .btn-primary {
      background: linear-gradient(
        135deg,
        var(--chess-tournaments-primary),
        var(--chess-tournaments-primary-hover)
      );
      color: var(--chess-tournaments-on-primary);
      border-color: rgba(184, 134, 11, 0.3);
      box-shadow: 0 2px 8px rgba(184, 134, 11, 0.2);
    }

    .btn-primary:hover {
      transform: translateY(-2px);
      box-shadow: 0 4px 12px rgba(184, 134, 11, 0.3);
    }

    .matches-list {
      display: flex;
      flex-direction: column;
      gap: 1rem;
      margin-bottom: 3rem;
    }

    .match-card {
      background: var(--chess-tournaments-card-bg);
      border: 2px solid rgba(184, 134, 11, 0.3);
      border-radius: 15px;
      padding: 1.5rem;
      transition: all 0.3s ease;
    }

    .match-card:hover {
      transform: translateX(4px);
      box-shadow: 0 4px 12px rgba(184, 134, 11, 0.15);
    }

    .match-card.my-win {
      border-color: rgba(34, 197, 94, 0.5);
      background: linear-gradient(
        135deg,
        rgba(34, 197, 94, 0.05),
        var(--chess-tournaments-card-bg)
      );
    }

    .match-card.my-loss {
      border-color: rgba(239, 68, 68, 0.5);
      background: linear-gradient(
        135deg,
        rgba(239, 68, 68, 0.05),
        var(--chess-tournaments-card-bg)
      );
    }

    .match-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-bottom: 1rem;
      padding-bottom: 1rem;
      border-bottom: 1px solid rgba(184, 134, 11, 0.2);
    }

    .match-date {
      color: var(--chess-tournaments-muted);
      font-size: 0.9rem;
    }

    .match-result-badge {
      padding: 0.25rem 0.75rem;
      border-radius: 20px;
      font-size: 0.85rem;
      font-weight: 700;
      text-transform: uppercase;
      letter-spacing: 1px;
    }

    .match-result-badge.win {
      background: rgba(34, 197, 94, 0.2);
      color: rgb(34, 197, 94);
    }

    .match-result-badge.loss {
      background: rgba(239, 68, 68, 0.2);
      color: rgb(239, 68, 68);
    }

    .match-result-badge.draw {
      background: rgba(156, 163, 175, 0.2);
      color: rgb(156, 163, 175);
    }

    .match-result-badge.ongoing {
      background: rgba(59, 130, 246, 0.2);
      color: rgb(59, 130, 246);
    }

    .match-players {
      display: flex;
      align-items: center;
      gap: 1rem;
      margin-bottom: 1rem;
    }

    .player {
      flex: 1;
      display: flex;
      align-items: center;
      gap: 0.75rem;
      padding: 0.75rem;
      background: rgba(20, 20, 20, 0.5);
      border-radius: 10px;
      border: 2px solid transparent;
      transition: all 0.3s ease;
    }

    .player.me {
      border-color: var(--chess-tournaments-primary);
    }

    .player.winner {
      background: rgba(34, 197, 94, 0.1);
    }

    .player-color {
      font-size: 1.5rem;
    }

    .player-name {
      color: var(--chess-tournaments-heading);
      font-weight: 600;
      display: flex;
      align-items: center;
      gap: 0.5rem;
    }

    .me-badge {
      background: var(--chess-tournaments-primary);
      color: var(--chess-tournaments-on-primary);
      padding: 0.125rem 0.5rem;
      border-radius: 10px;
      font-size: 0.7rem;
      font-weight: 700;
    }

    .vs-divider {
      color: var(--chess-tournaments-muted);
      font-weight: 700;
      font-size: 0.9rem;
    }

    .match-tournament {
      display: flex;
      align-items: center;
      gap: 0.5rem;
      padding-top: 1rem;
      border-top: 1px solid rgba(184, 134, 11, 0.1);
    }

    .tournament-label {
      color: var(--chess-tournaments-muted);
      font-size: 0.9rem;
    }

    .tournament-link {
      color: var(--chess-tournaments-primary);
      text-decoration: none;
      font-weight: 600;
      font-size: 0.9rem;
      transition: all 0.3s ease;
    }

    .tournament-link:hover {
      color: var(--chess-tournaments-primary-hover);
    }

    .matches-stats {
      background: var(--chess-tournaments-card-bg);
      border: 1px solid rgba(184, 134, 11, 0.3);
      border-radius: 15px;
      padding: 2rem;
    }

    .matches-stats h2 {
      color: var(--chess-tournaments-heading);
      font-size: 1.75rem;
      margin: 0 0 1.5rem 0;
    }

    .stats-grid {
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(150px, 1fr));
      gap: 1.5rem;
    }

    .stat-card {
      background: rgba(20, 20, 20, 0.5);
      border-radius: 10px;
      padding: 1.5rem;
      text-align: center;
      display: flex;
      flex-direction: column;
      gap: 0.5rem;
    }

    .stat-value {
      color: var(--chess-tournaments-primary);
      font-size: 2rem;
      font-weight: 700;
    }

    .stat-card.wins .stat-value {
      color: rgb(34, 197, 94);
    }

    .stat-card.losses .stat-value {
      color: rgb(239, 68, 68);
    }

    .stat-card.draws .stat-value {
      color: rgb(156, 163, 175);
    }

    .stat-label {
      color: var(--chess-tournaments-muted);
      font-size: 0.9rem;
      text-transform: uppercase;
      letter-spacing: 1px;
    }

    @media (max-width: 768px) {
      .my-matches-header h1 {
        font-size: 2rem;
      }

      .match-players {
        flex-direction: column;
      }

      .stats-grid {
        grid-template-columns: repeat(2, 1fr);
      }
    }
  `,
})
export class MyMatchesComponent implements OnInit {
  private authService = inject(AuthService);
  private matchesApi = inject(MatchesService);

  matches = signal<TournamentMatchDto[]>([]);
  isLoading = signal<boolean>(false);
  error = signal<string | null>(null);

  get userId(): string | null {
    const claims = this.authService.identityClaims;
    return claims ? (claims['sub'] as string) : null;
  }

  ngOnInit(): void {
    this.loadMatches();
  }

  private loadMatches(): void {
    const userId = this.userId;
    if (!userId) {
      this.error.set('User ID not found');
      return;
    }

    this.isLoading.set(true);

    // Search for matches using userId (Match entity stores userId, not Player entity ID)
    this.matchesApi.searchMatches({ playerId: userId }).subscribe({
      next: (matches: TournamentMatchDto[]) => {
        this.matches.set(matches);
        this.isLoading.set(false);
      },
      error: (err: unknown) => {
        this.error.set('Failed to load matches');
        this.isLoading.set(false);
        console.error('Error loading matches:', err);
      },
    });
  }

  isMe(playerId: string | undefined): boolean {
    return playerId === this.userId;
  }

  private isResultType(match: TournamentMatchDto, ...types: (string | number)[]): boolean {
    return types.some(
      (type) =>
        match.result === type ||
        (typeof match.result === 'string' && match.result === this.getEnumName(type as number)),
    );
  }

  private getEnumName(value: number): string {
    const names = ['Ongoing', 'WhiteWin', 'BlackWin', 'Draw', 'Forfeit'];
    return names[value] || '';
  }

  isWhiteWinner(match: TournamentMatchDto): boolean {
    return this.isResultType(match, 1, 'WhiteWin');
  }

  isBlackWinner(match: TournamentMatchDto): boolean {
    return this.isResultType(match, 2, 'BlackWin');
  }

  isMyWin(match: TournamentMatchDto): boolean {
    const userId = this.userId;
    return (
      (match.whitePlayerId === userId && this.isResultType(match, 1, 'WhiteWin')) ||
      (match.blackPlayerId === userId && this.isResultType(match, 2, 'BlackWin'))
    );
  }

  isMyLoss(match: TournamentMatchDto): boolean {
    const userId = this.userId;
    return (
      (match.whitePlayerId === userId && this.isResultType(match, 2, 'BlackWin')) ||
      (match.blackPlayerId === userId && this.isResultType(match, 1, 'WhiteWin'))
    );
  }

  getResultClass(match: TournamentMatchDto): string {
    if (this.isResultType(match, 0, 'Ongoing')) return 'ongoing';
    if (this.isResultType(match, 3, 'Draw')) return 'draw';
    return this.isMyWin(match) ? 'win' : 'loss';
  }

  getResultText(match: TournamentMatchDto): string {
    if (this.isResultType(match, 0, 'Ongoing')) return 'Ongoing';
    if (this.isResultType(match, 3, 'Draw')) return 'Draw';
    if (this.isResultType(match, 4, 'Forfeit')) return 'Forfeit';
    return this.isMyWin(match) ? 'Win' : 'Loss';
  }

  getTotalMatches(): number {
    return this.matches().filter((m) => !this.isResultType(m, 0, 'Ongoing')).length;
  }

  getWins(): number {
    return this.matches().filter((m) => this.isMyWin(m)).length;
  }

  getLosses(): number {
    return this.matches().filter((m) => this.isMyLoss(m)).length;
  }

  getDraws(): number {
    return this.matches().filter((m) => this.isResultType(m, 3, 'Draw')).length;
  }
}
