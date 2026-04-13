import { Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { AuthService } from '@app/auth/auth.service';
import { AuthorizationService } from '@app/auth/authorization.service';
import { PlayersService } from '@app/infrastructure/api/services/players.service';
import { PlayerDto } from '@app/infrastructure/api/models/player-dto';

@Component({
  selector: 'app-home',
  imports: [CommonModule, RouterModule],
  template: `
    <div class="home-container">
      @if (isAuthenticated$ | async) {
        @if (!authorizationService.isAdmin) {
          <!-- Dashboard for non-admin users -->
          <div class="dashboard-container">
            <div class="dashboard-header">
              <div class="header-content">
                <h1>Dashboard</h1>
                <p class="welcome-text">Welcome, {{ player()?.fullName || userName }}!</p>
              </div>
            </div>

            @if (isLoading()) {
              <div class="loading-container">
                <p>Loading player data...</p>
              </div>
            }

            @if (error()) {
              <div class="error-container">
                <p class="error-message">{{ error() }}</p>
              </div>
            }

            @if (!isLoading() && player()) {
              <div class="dashboard-grid">
                <div class="dashboard-card">
                  <div class="card-icon">♔</div>
                  <h3>Tournaments</h3>
                  <p class="card-value">{{ player()!.tournamentsParticipated }}</p>
                  <p class="card-subtitle">Participated</p>
                </div>

                <div class="dashboard-card">
                  <div class="card-icon">♖</div>
                  <h3>Matches Played</h3>
                  <p class="card-value">{{ player()!.totalGamesPlayed }}</p>
                  <p class="card-subtitle">Total matches</p>
                </div>

                <div class="dashboard-card">
                  <div class="card-icon">♕</div>
                  <h3>Win Rate</h3>
                  <p class="card-value">{{ player()!.winRate | number: '1.1-1' }}%</p>
                  <p class="card-subtitle">
                    {{ player()!.wins }} wins / {{ player()!.losses }} losses
                  </p>
                </div>

                <div class="dashboard-card">
                  <div class="card-icon">♗</div>
                  <h3>Rating</h3>
                  <p class="card-value">{{ player()!.rating }}</p>
                  <p class="card-subtitle">Peak: {{ player()!.peakRating }}</p>
                </div>
              </div>

              <div class="user-info-section">
                <h2>Player Information</h2>
                <div class="info-card">
                  <div class="info-row">
                    <span class="info-label">Name:</span>
                    <span class="info-value">{{ player()!.fullName }}</span>
                  </div>
                  <div class="info-row">
                    <span class="info-label">Email:</span>
                    <span class="info-value">{{ userEmail || 'N/A' }}</span>
                  </div>
                  @if (player()!.country) {
                    <div class="info-row">
                      <span class="info-label">Country:</span>
                      <span class="info-value">{{ player()!.country }}</span>
                    </div>
                  }
                  <div class="info-row">
                    <span class="info-label">Rating:</span>
                    <span class="info-value">{{ player()!.rating }}</span>
                  </div>
                  <div class="info-row">
                    <span class="info-label">Tournaments Won:</span>
                    <span class="info-value">{{ player()!.tournamentsWon }}</span>
                  </div>
                  <div class="info-row">
                    <span class="info-label">Record:</span>
                    <span class="info-value">
                      {{ player()!.wins }}W - {{ player()!.draws }}D - {{ player()!.losses }}L
                    </span>
                  </div>
                </div>
              </div>
            }
          </div>
        } @else {
          <!-- Landing page for admins -->
          <div class="hero-section">
            <div class="chess-pieces-bg">♔ ♕ ♖ ♗ ♘ ♙</div>
            <h1 class="main-title">Chess Tournaments</h1>
            <div class="action-section">
              <div class="welcome-message">
                <p>Welcome back, {{ userName }}!</p>
                <p class="admin-badge">Administrator</p>
              </div>
            </div>
          </div>

          <div class="features-section">
            <div class="feature-card clickable" routerLink="/tournaments">
              <div class="feature-icon">♔</div>
              <h3>Organize Tournaments</h3>
              <p>Create and manage chess tournaments with ease</p>
            </div>
            <div class="feature-card">
              <div class="feature-icon">♖</div>
              <h3>Track Rankings</h3>
              <p>Monitor player rankings and tournament standings</p>
            </div>
            <div class="feature-card">
              <div class="feature-icon">♗</div>
              <h3>Match History</h3>
              <p>View detailed match history and statistics</p>
            </div>
          </div>
        }
      } @else {
        <!-- Landing page for unauthenticated users -->
        <div class="hero-section">
          <div class="chess-pieces-bg">♔ ♕ ♖ ♗ ♘ ♙</div>
          <h1 class="main-title">Chess Tournaments</h1>

          <div class="action-section">
            <button class="btn btn-primary" (click)="login()">
              <span>Sign In</span>
              <span class="btn-arrow">→</span>
            </button>
            <button class="btn btn-secondary" routerLink="/register">Create Account</button>
            <p class="info-text">Sign in to access your tournaments and matches</p>
          </div>
        </div>
      }
    </div>
  `,
  styles: `
    .home-container {
      min-height: calc(100vh - 120px);
      padding: 2rem;
    }

    /* Dashboard Styles */
    .dashboard-container {
      padding: 2rem;
      max-width: 1400px;
      margin: 0 auto;
    }

    .dashboard-header {
      margin-bottom: 2rem;
      padding-bottom: 1.5rem;
      border-bottom: 2px solid rgba(184, 134, 11, 0.2);
    }

    .header-content h1 {
      color: var(--chess-tournaments-heading);
      font-size: 2.5rem;
      font-weight: 700;
      margin: 0 0 0.5rem 0;
    }

    .welcome-text {
      color: var(--chess-tournaments-muted);
      font-size: 1.1rem;
      margin: 0;
    }

    .dashboard-grid {
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(250px, 1fr));
      gap: 1.5rem;
      margin-bottom: 3rem;
    }

    .dashboard-card {
      background: var(--chess-tournaments-card-bg);
      border: 1px solid rgba(184, 134, 11, 0.3);
      border-radius: 15px;
      padding: 2rem;
      transition: all 0.3s ease;
      box-shadow: 0 2px 8px rgba(0, 0, 0, 0.06);
    }

    .dashboard-card:hover {
      transform: translateY(-4px);
      box-shadow: 0 4px 16px rgba(184, 134, 11, 0.15);
    }

    .card-icon {
      font-size: 2.5rem;
      margin-bottom: 1rem;
      background: linear-gradient(
        135deg,
        var(--chess-tournaments-primary),
        var(--chess-tournaments-primary-hover)
      );
      -webkit-background-clip: text;
      -webkit-text-fill-color: transparent;
      background-clip: text;
    }

    .dashboard-card h3 {
      color: var(--chess-tournaments-heading);
      font-size: 1.1rem;
      margin: 0 0 1rem 0;
      text-transform: uppercase;
      letter-spacing: 1px;
    }

    .card-value {
      color: var(--chess-tournaments-primary);
      font-size: 2.5rem;
      font-weight: 700;
      margin: 0 0 0.5rem 0;
    }

    .card-subtitle {
      color: var(--chess-tournaments-muted);
      font-size: 0.9rem;
      margin: 0;
    }

    .user-info-section {
      background: var(--chess-tournaments-card-bg);
      border: 1px solid rgba(184, 134, 11, 0.3);
      border-radius: 15px;
      padding: 2rem;
    }

    .user-info-section h2 {
      color: var(--chess-tournaments-heading);
      font-size: 1.75rem;
      margin: 0 0 1.5rem 0;
    }

    .info-card {
      background: rgba(20, 20, 20, 0.5);
      border-radius: 10px;
      padding: 1.5rem;
    }

    .info-row {
      display: flex;
      justify-content: space-between;
      padding: 0.75rem 0;
      border-bottom: 1px solid rgba(184, 134, 11, 0.1);
    }

    .info-row:last-child {
      border-bottom: none;
    }

    .info-label {
      color: var(--chess-tournaments-muted);
      font-weight: 600;
    }

    .info-value {
      color: var(--chess-tournaments-heading);
      font-family: monospace;
    }

    .loading-container,
    .error-container {
      text-align: center;
      padding: 3rem;
      background: var(--chess-tournaments-card-bg);
      border: 1px solid rgba(184, 134, 11, 0.3);
      border-radius: 15px;
      margin-bottom: 2rem;
    }

    .loading-container p {
      color: var(--chess-tournaments-text);
      font-size: 1.1rem;
    }

    .error-message {
      color: rgba(220, 38, 38, 0.9);
      font-size: 1.1rem;
      font-weight: 600;
    }

    /* Landing Page Styles */
    .hero-section {
      text-align: center;
      padding: 4rem 2rem;
      position: relative;
      margin-bottom: 4rem;
    }

    .chess-pieces-bg {
      font-size: 6rem;
      color: var(--chess-tournaments-border);
      opacity: 0.1;
      position: absolute;
      top: 0;
      left: 50%;
      transform: translateX(-50%);
      pointer-events: none;
      z-index: 0;
    }

    .main-title {
      font-size: 4rem;
      font-weight: 700;
      color: var(--chess-tournaments-heading);
      margin: 0 0 1rem 0;
      text-shadow: 0 4px 12px rgba(0, 0, 0, 0.5);
      position: relative;
      z-index: 1;
    }

    .action-section {
      display: flex;
      flex-direction: column;
      align-items: center;
      gap: 1.5rem;
      position: relative;
      z-index: 1;
    }

    .welcome-message {
      color: var(--chess-tournaments-heading);
      font-size: 1.25rem;
      font-weight: 600;
      text-align: center;
    }

    .welcome-message p {
      margin: 0.5rem 0;
    }

    .admin-badge {
      display: inline-block;
      background: linear-gradient(
        135deg,
        var(--chess-tournaments-primary),
        var(--chess-tournaments-primary-hover)
      );
      color: var(--chess-tournaments-on-primary);
      padding: 0.5rem 1rem;
      border-radius: 20px;
      font-size: 0.9rem;
      font-weight: 700;
      text-transform: uppercase;
      letter-spacing: 1px;
    }

    .btn {
      padding: 1rem 2.5rem;
      font-size: 1.1rem;
      font-weight: 700;
      text-transform: uppercase;
      letter-spacing: 1px;
      border-radius: 12px;
      cursor: pointer;
      transition: all 0.3s ease;
      display: flex;
      align-items: center;
      gap: 0.75rem;
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

    .btn-secondary {
      background: rgba(255, 255, 255, 0.8);
      color: var(--chess-tournaments-text);
      border-color: var(--chess-tournaments-border);
    }

    .btn-secondary:hover {
      background: rgba(255, 255, 255, 1);
      border-color: var(--chess-tournaments-primary);
      transform: translateY(-2px);
    }

    .btn-arrow {
      font-size: 1.25rem;
      transition: transform 0.3s ease;
    }

    .btn-primary:hover .btn-arrow {
      transform: translateX(4px);
    }

    .info-text {
      color: var(--chess-tournaments-muted);
      font-size: 0.95rem;
    }

    .features-section {
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(300px, 1fr));
      gap: 2rem;
      max-width: 1200px;
      margin: 0 auto;
    }

    .feature-card {
      background: var(--chess-tournaments-card-bg);
      border: 1px solid rgba(184, 134, 11, 0.3);
      border-radius: 15px;
      padding: 2rem;
      text-align: center;
      transition: all 0.3s ease;
      box-shadow: 0 2px 8px rgba(0, 0, 0, 0.06);
    }

    .feature-card:hover {
      transform: translateY(-4px);
      box-shadow: 0 4px 16px rgba(184, 134, 11, 0.15);
    }

    .feature-card.clickable {
      cursor: pointer;
    }

    .feature-card.clickable:hover {
      border-color: var(--chess-tournaments-primary);
      box-shadow: 0 6px 20px rgba(184, 134, 11, 0.25);
    }

    .feature-icon {
      font-size: 3rem;
      margin-bottom: 1rem;
      background: linear-gradient(
        135deg,
        var(--chess-tournaments-primary),
        var(--chess-tournaments-primary-hover)
      );
      -webkit-background-clip: text;
      -webkit-text-fill-color: transparent;
      background-clip: text;
    }

    .feature-card h3 {
      color: var(--chess-tournaments-heading);
      font-size: 1.5rem;
      margin: 0 0 0.75rem 0;
    }

    .feature-card p {
      color: var(--chess-tournaments-muted);
      margin: 0;
    }

    @media (max-width: 768px) {
      .main-title {
        font-size: 2.5rem;
      }

      .chess-pieces-bg {
        font-size: 4rem;
      }

      .features-section {
        grid-template-columns: 1fr;
      }

      .dashboard-grid {
        grid-template-columns: 1fr;
      }

      .info-row {
        flex-direction: column;
        gap: 0.5rem;
      }
    }
  `,
})
export class HomeComponent implements OnInit {
  private authService = inject(AuthService);
  authorizationService = inject(AuthorizationService);
  private playersApi = inject(PlayersService);
  private router = inject(Router);

  isAuthenticated$ = this.authService.isAuthenticated$;
  player = signal<PlayerDto | null>(null);
  isLoading = signal<boolean>(false);
  error = signal<string | null>(null);

  get userName(): string | null {
    return this.authService.getUserName();
  }

  get userEmail(): string | null {
    return this.authService.getUserEmail();
  }

  get userId(): string | null {
    const claims = this.authService.identityClaims;
    return claims ? (claims['sub'] as string) : null;
  }

  ngOnInit(): void {
    // Load player data only if authenticated and not admin
    this.authService.isAuthenticated$.subscribe((isAuth) => {
      if (isAuth && !this.authorizationService.isAdmin) {
        this.loadPlayerData();
      }
    });
  }

  private loadPlayerData(): void {
    const userId = this.userId;
    if (!userId) {
      this.error.set('User ID not found');
      return;
    }

    this.isLoading.set(true);
    this.playersApi.apiPlayersUserUserIdGet({ userId }).subscribe({
      next: (player: PlayerDto) => {
        this.player.set(player);
        this.isLoading.set(false);
      },
      error: (err: unknown) => {
        this.error.set('Failed to load player data');
        this.isLoading.set(false);
        console.error('Error loading player data:', err);
      },
    });
  }

  login(): void {
    this.authService.login();
  }
}
