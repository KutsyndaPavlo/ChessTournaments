import { Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { PlayersService } from '@app/infrastructure/api/services/players.service';
import { AuthService } from '@app/auth/auth.service';
import { AchievementResponse } from '@app/infrastructure/api/models/achievement-response';

@Component({
  selector: 'app-my-achievements',
  imports: [CommonModule, RouterModule],
  template: `
    <div class="achievements-container">
      <div class="achievements-header">
        <h1>My Achievements</h1>
        <p class="subtitle">Your tournament victories and accomplishments</p>
      </div>

      @if (isLoading()) {
        <div class="loading-container">
          <p>Loading achievements...</p>
        </div>
      }

      @if (error()) {
        <div class="error-container">
          <p class="error-message">{{ error() }}</p>
        </div>
      }

      @if (!isLoading() && achievements().length === 0) {
        <div class="empty-state">
          <div class="empty-icon">🏆</div>
          <h2>No Achievements Yet</h2>
          <p>Participate in tournaments and finish in the top 3 to earn achievements!</p>
          <a routerLink="/tournaments" class="btn btn-primary">Browse Tournaments</a>
        </div>
      }

      @if (!isLoading() && achievements().length > 0) {
        <div class="achievements-grid">
          @for (achievement of achievements(); track achievement.id) {
            <div class="achievement-card" [class]="'position-' + achievement.position">
              <div class="achievement-medal">
                <span class="medal-emoji">{{ achievement.medalEmoji }}</span>
                <span class="medal-text">{{ achievement.positionText }}</span>
              </div>
              <div class="achievement-content">
                <h3 class="tournament-name">{{ achievement.tournamentName }}</h3>
                <div class="achievement-details">
                  <div class="detail-item">
                    <span class="detail-label">Score:</span>
                    <span class="detail-value">{{ achievement.score }}</span>
                  </div>
                  <div class="detail-item">
                    <span class="detail-label">Date:</span>
                    <span class="detail-value">{{
                      achievement.achievedAt | date: 'mediumDate'
                    }}</span>
                  </div>
                </div>
                <a
                  [routerLink]="['/tournaments', achievement.tournamentId]"
                  class="view-tournament-link"
                >
                  View Tournament →
                </a>
              </div>
            </div>
          }
        </div>

        <div class="achievements-summary">
          <h2>Summary</h2>
          <div class="summary-grid">
            <div class="summary-card">
              <span class="summary-icon">🥇</span>
              <span class="summary-count">{{ getCountByPosition(1) }}</span>
              <span class="summary-label">Gold Medals</span>
            </div>
            <div class="summary-card">
              <span class="summary-icon">🥈</span>
              <span class="summary-count">{{ getCountByPosition(2) }}</span>
              <span class="summary-label">Silver Medals</span>
            </div>
            <div class="summary-card">
              <span class="summary-icon">🥉</span>
              <span class="summary-count">{{ getCountByPosition(3) }}</span>
              <span class="summary-label">Bronze Medals</span>
            </div>
          </div>
        </div>
      }
    </div>
  `,
  styles: `
    .achievements-container {
      max-width: 1200px;
      margin: 0 auto;
      padding: 2rem;
    }

    .achievements-header {
      margin-bottom: 2rem;
      padding-bottom: 1.5rem;
      border-bottom: 2px solid rgba(184, 134, 11, 0.2);
    }

    .achievements-header h1 {
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
    .error-container {
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

    .empty-state {
      text-align: center;
      padding: 4rem 2rem;
      background: var(--chess-tournaments-card-bg);
      border: 1px solid rgba(184, 134, 11, 0.3);
      border-radius: 15px;
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
      background: linear-gradient(
        135deg,
        var(--chess-tournaments-primary-hover),
        var(--chess-tournaments-primary-active)
      );
      transform: translateY(-2px);
      box-shadow: 0 4px 12px rgba(184, 134, 11, 0.3);
    }

    .achievements-grid {
      display: grid;
      grid-template-columns: repeat(auto-fill, minmax(350px, 1fr));
      gap: 1.5rem;
      margin-bottom: 3rem;
    }

    .achievement-card {
      background: var(--chess-tournaments-card-bg);
      border: 2px solid rgba(184, 134, 11, 0.3);
      border-radius: 15px;
      padding: 1.5rem;
      transition: all 0.3s ease;
      display: flex;
      flex-direction: column;
      gap: 1rem;
    }

    .achievement-card:hover {
      transform: translateY(-4px);
      box-shadow: 0 6px 20px rgba(184, 134, 11, 0.2);
    }

    .achievement-card.position-1 {
      border-color: rgba(255, 215, 0, 0.5);
      background: linear-gradient(
        135deg,
        rgba(255, 215, 0, 0.05),
        var(--chess-tournaments-card-bg)
      );
    }

    .achievement-card.position-2 {
      border-color: rgba(192, 192, 192, 0.5);
      background: linear-gradient(
        135deg,
        rgba(192, 192, 192, 0.05),
        var(--chess-tournaments-card-bg)
      );
    }

    .achievement-card.position-3 {
      border-color: rgba(205, 127, 50, 0.5);
      background: linear-gradient(
        135deg,
        rgba(205, 127, 50, 0.05),
        var(--chess-tournaments-card-bg)
      );
    }

    .achievement-medal {
      display: flex;
      align-items: center;
      gap: 1rem;
      padding-bottom: 1rem;
      border-bottom: 1px solid rgba(184, 134, 11, 0.2);
    }

    .medal-emoji {
      font-size: 3rem;
    }

    .medal-text {
      color: var(--chess-tournaments-primary);
      font-size: 1.25rem;
      font-weight: 700;
      text-transform: uppercase;
      letter-spacing: 1px;
    }

    .achievement-content {
      flex: 1;
      display: flex;
      flex-direction: column;
      gap: 0.75rem;
    }

    .tournament-name {
      color: var(--chess-tournaments-heading);
      font-size: 1.25rem;
      font-weight: 600;
      margin: 0;
    }

    .achievement-details {
      display: flex;
      flex-direction: column;
      gap: 0.5rem;
    }

    .detail-item {
      display: flex;
      justify-content: space-between;
      padding: 0.5rem 0;
      border-bottom: 1px solid rgba(184, 134, 11, 0.1);
    }

    .detail-label {
      color: var(--chess-tournaments-muted);
      font-weight: 600;
    }

    .detail-value {
      color: var(--chess-tournaments-heading);
      font-family: monospace;
    }

    .view-tournament-link {
      color: var(--chess-tournaments-primary);
      text-decoration: none;
      font-weight: 600;
      transition: all 0.3s ease;
      align-self: flex-start;
    }

    .view-tournament-link:hover {
      color: var(--chess-tournaments-primary-hover);
      transform: translateX(4px);
    }

    .achievements-summary {
      background: var(--chess-tournaments-card-bg);
      border: 1px solid rgba(184, 134, 11, 0.3);
      border-radius: 15px;
      padding: 2rem;
    }

    .achievements-summary h2 {
      color: var(--chess-tournaments-heading);
      font-size: 1.75rem;
      margin: 0 0 1.5rem 0;
    }

    .summary-grid {
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
      gap: 1.5rem;
    }

    .summary-card {
      background: rgba(20, 20, 20, 0.5);
      border-radius: 10px;
      padding: 1.5rem;
      text-align: center;
      display: flex;
      flex-direction: column;
      align-items: center;
      gap: 0.75rem;
    }

    .summary-icon {
      font-size: 2.5rem;
    }

    .summary-count {
      color: var(--chess-tournaments-primary);
      font-size: 2rem;
      font-weight: 700;
    }

    .summary-label {
      color: var(--chess-tournaments-muted);
      font-size: 0.9rem;
      text-transform: uppercase;
      letter-spacing: 1px;
    }

    @media (max-width: 768px) {
      .achievements-grid {
        grid-template-columns: 1fr;
      }

      .achievements-header h1 {
        font-size: 2rem;
      }
    }
  `,
})
export class MyAchievementsComponent implements OnInit {
  private playersApi = inject(PlayersService);
  private authService = inject(AuthService);

  achievements = signal<AchievementResponse[]>([]);
  isLoading = signal<boolean>(false);
  error = signal<string | null>(null);

  get userId(): string | null {
    const claims = this.authService.identityClaims;
    return claims ? (claims['sub'] as string) : null;
  }

  ngOnInit(): void {
    this.loadAchievements();
  }

  private loadAchievements(): void {
    const userId = this.userId;
    if (!userId) {
      this.error.set('User ID not found');
      return;
    }

    this.isLoading.set(true);

    // First get the player to get their player ID
    this.playersApi.apiPlayersUserUserIdGet({ userId }).subscribe({
      next: (player) => {
        // Now get achievements using the player ID
        this.playersApi.getPlayerAchievements({ playerId: player.id! }).subscribe({
          next: (achievements: AchievementResponse[]) => {
            this.achievements.set(achievements);
            this.isLoading.set(false);
          },
          error: (err: unknown) => {
            this.error.set('Failed to load achievements');
            this.isLoading.set(false);
            console.error('Error loading achievements:', err);
          },
        });
      },
      error: (err: unknown) => {
        this.error.set('Failed to load player data');
        this.isLoading.set(false);
        console.error('Error loading player data:', err);
      },
    });
  }

  getCountByPosition(position: number): number {
    return this.achievements().filter((a) => a.position === position).length;
  }
}
