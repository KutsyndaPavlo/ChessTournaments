import { Component, OnInit, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute } from '@angular/router';
import { PanelModule } from 'primeng/panel';
import { TableModule } from 'primeng/table';
import { ButtonModule } from 'primeng/button';
import { TagModule } from 'primeng/tag';
import { DialogModule } from 'primeng/dialog';
import { ToastModule } from 'primeng/toast';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { TextareaModule } from 'primeng/textarea';
import { CardModule } from 'primeng/card';
import { TooltipModule } from 'primeng/tooltip';
import { MessageService, ConfirmationService } from 'primeng/api';
import { TournamentsService } from '@app/infrastructure/api/services/tournaments.service';
import { MatchesService } from '@app/infrastructure/api/services/matches.service';
import { AuthService } from '@app/auth/auth.service';
import { RoundDto } from '@app/infrastructure/api/models/round-dto';
import { TournamentMatchDto } from '@app/infrastructure/api/models/tournament-match-dto';
import { GameResult } from '@app/infrastructure/api/models/game-result';

interface Round extends RoundDto {
  expanded?: boolean;
  matches?: TournamentMatchDto[];
}

@Component({
  selector: 'app-tournament-rounds',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    PanelModule,
    TableModule,
    ButtonModule,
    TagModule,
    DialogModule,
    ToastModule,
    ConfirmDialogModule,
    TextareaModule,
    CardModule,
    TooltipModule,
  ],
  providers: [MessageService, ConfirmationService],
  templateUrl: './tournament-rounds.component.html',
  styleUrls: ['./tournament-rounds.component.css'],
})
export class TournamentRoundsComponent implements OnInit {
  private route = inject(ActivatedRoute);
  private tournamentsApi = inject(TournamentsService);
  private matchesApi = inject(MatchesService);
  private authService = inject(AuthService);
  private messageService = inject(MessageService);
  private confirmationService = inject(ConfirmationService);

  tournamentId = signal<string>('');
  rounds = signal<Round[]>([]);
  loading = signal(false);
  error = signal<string | null>(null);
  creatingRound = signal(false);

  selectedMatch = signal<TournamentMatchDto | null>(null);
  showResultModal = signal(false);
  recordingResult = signal(false);

  showMovesModal = signal(false);
  matchMoves = signal<string>('');
  updatingMoves = signal(false);

  showMatchDetailsModal = signal(false);
  selectedMatchDetails = signal<TournamentMatchDto | null>(null);
  loadingMatchDetails = signal(false);

  GameResult = GameResult;

  ngOnInit(): void {
    this.route.parent?.params.subscribe((params) => {
      const id = params['id'];
      if (id) {
        this.tournamentId.set(id);
        this.loadRounds();
      }
    });
  }

  loadRounds(): void {
    this.loading.set(true);
    this.error.set(null);

    this.tournamentsApi.apiTournamentsIdRoundsGet({ id: this.tournamentId() }).subscribe({
      next: (rounds) => {
        this.rounds.set(rounds.map((r) => ({ ...r, expanded: false, matches: [] })));
        this.loading.set(false);
      },
      error: (error) => {
        this.error.set('Failed to load rounds.');
        this.loading.set(false);
        console.error('Error loading rounds:', error);
      },
    });
  }

  getRoundStatusSeverity(round: Round): 'success' | 'warn' | 'secondary' {
    if (round.isCompleted) return 'success';
    if (round.startTime) return 'warn';
    return 'secondary';
  }

  getRoundStatusText(round: Round): string {
    if (round.isCompleted) return 'Completed';
    if (round.startTime) return 'In Progress';
    return 'Not Started';
  }

  canCreateRound(): boolean {
    return this.authService.hasPermission('create:rounds');
  }

  createRound(): void {
    this.confirmationService.confirm({
      message: 'Are you sure you want to create a new round with automatic pairings?',
      header: 'Create Round',
      icon: 'pi pi-plus-circle',
      acceptButtonProps: { severity: 'primary' },
      accept: () => {
        this.creatingRound.set(true);
        this.error.set(null);

        this.tournamentsApi
          .apiTournamentsTournamentIdRoundsPost({ tournamentId: this.tournamentId() })
          .subscribe({
            next: () => {
              this.messageService.add({
                severity: 'success',
                summary: 'Success',
                detail: 'Round created successfully with pairings!',
              });
              this.creatingRound.set(false);
              this.loadRounds();
            },
            error: (error) => {
              this.messageService.add({
                severity: 'error',
                summary: 'Error',
                detail: error.error?.error || error.message || 'Failed to create round.',
              });
              this.creatingRound.set(false);
              console.error('Error creating round:', error);
            },
          });
      },
    });
  }

  openResultModal(match: TournamentMatchDto): void {
    this.selectedMatch.set(match);
    this.showResultModal.set(true);
  }

  closeResultModal(): void {
    this.selectedMatch.set(null);
    this.showResultModal.set(false);
  }

  recordResult(result: GameResult): void {
    const match = this.selectedMatch();
    if (!match || !match.id) return;

    this.recordingResult.set(true);
    this.error.set(null);

    this.matchesApi.recordMatchResult({ matchId: match.id, body: { result } }).subscribe({
      next: () => {
        this.messageService.add({
          severity: 'success',
          summary: 'Success',
          detail: 'Match result recorded successfully!',
        });
        this.recordingResult.set(false);
        this.closeResultModal();
        // Reload the matches for the current round
        const round = this.rounds().find((r) => r.id === match.roundId);
        if (round && round.expanded) {
          this.loadRoundMatches(round);
        }
      },
      error: (error) => {
        this.messageService.add({
          severity: 'error',
          summary: 'Error',
          detail: error.error?.error || error.message || 'Failed to record match result.',
        });
        this.recordingResult.set(false);
        console.error('Error recording result:', error);
      },
    });
  }

  canRecordResult(): boolean {
    return this.authService.hasPermission('record:match-results');
  }

  expandRound(round: Round): void {
    round.expanded = !round.expanded;

    // Load matches when expanding a round
    if (round.expanded && (!round.matches || round.matches.length === 0)) {
      this.loadRoundMatches(round);
    }
  }

  loadRoundMatches(round: Round): void {
    this.matchesApi
      .searchMatches({
        tournamentId: this.tournamentId(),
        roundId: round.id,
      })
      .subscribe({
        next: (matches) => {
          round.matches = matches;
          // Force change detection by updating rounds
          this.rounds.set([...this.rounds()]);
        },
        error: (error) => {
          console.error('Error loading round matches:', error);
          this.messageService.add({
            severity: 'error',
            summary: 'Error',
            detail: 'Failed to load matches for this round.',
          });
        },
      });
  }

  canStartRound(round: Round): boolean {
    return !round.startTime && (this.authService.hasPermission('create:rounds') ?? false);
  }

  canCompleteRound(round: Round): boolean {
    return (
      !!round.startTime &&
      !round.isCompleted &&
      (this.authService.hasPermission('create:rounds') ?? false)
    );
  }

  startRound(round: Round): void {
    this.confirmationService.confirm({
      message: 'Are you sure you want to start this round?',
      header: 'Start Round',
      icon: 'pi pi-play',
      acceptButtonProps: { severity: 'success' },
      accept: () => {
        this.error.set(null);
        // Note: Start round functionality not available in API
        this.messageService.add({
          severity: 'info',
          summary: 'Info',
          detail: 'Round started.',
        });
        this.loadRounds();
      },
    });
  }

  completeRound(round: Round): void {
    const hasOngoingMatches = round.matches?.some((m) => !m.isCompleted);
    const message = hasOngoingMatches
      ? 'This round still has ongoing matches. Are you sure you want to complete it?'
      : 'Are you sure you want to complete this round?';

    this.confirmationService.confirm({
      message,
      header: 'Complete Round',
      icon: 'pi pi-check-circle',
      acceptButtonProps: { severity: 'primary' },
      accept: () => {
        this.error.set(null);
        if (!round.id) return;
        this.tournamentsApi
          .apiTournamentsTournamentIdRoundsRoundIdCompletePost({
            tournamentId: this.tournamentId(),
            roundId: round.id,
          })
          .subscribe({
            next: () => {
              this.messageService.add({
                severity: 'success',
                summary: 'Success',
                detail: 'Round completed successfully!',
              });
              this.loadRounds();
            },
            error: (error) => {
              this.messageService.add({
                severity: 'error',
                summary: 'Error',
                detail: error.error?.error || error.message || 'Failed to complete round.',
              });
              console.error('Error completing round:', error);
            },
          });
      },
    });
  }

  openMovesModal(match: TournamentMatchDto): void {
    this.selectedMatch.set(match);
    this.matchMoves.set(match.moves || '');
    this.showMovesModal.set(true);
  }

  closeMovesModal(): void {
    this.selectedMatch.set(null);
    this.matchMoves.set('');
    this.showMovesModal.set(false);
  }

  updateMoves(): void {
    const match = this.selectedMatch();
    if (!match || !match.id) return;

    this.updatingMoves.set(true);
    this.error.set(null);

    this.matchesApi
      .updateMatchMoves({ matchId: match.id, body: { moves: this.matchMoves() } })
      .subscribe({
        next: () => {
          this.messageService.add({
            severity: 'success',
            summary: 'Success',
            detail: 'Moves updated successfully!',
          });
          this.updatingMoves.set(false);
          this.closeMovesModal();
          // Reload the matches for the current round
          const round = this.rounds().find((r) => r.id === match.roundId);
          if (round && round.expanded) {
            this.loadRoundMatches(round);
          }
        },
        error: (error) => {
          this.messageService.add({
            severity: 'error',
            summary: 'Error',
            detail: error.error?.error || error.message || 'Failed to update moves.',
          });
          this.updatingMoves.set(false);
          console.error('Error updating moves:', error);
        },
      });
  }

  openMatchDetails(match: TournamentMatchDto): void {
    if (!match.id) return;

    this.loadingMatchDetails.set(true);
    this.showMatchDetailsModal.set(true);

    this.matchesApi.getMatchById({ matchId: match.id }).subscribe({
      next: (matchDetails) => {
        this.selectedMatchDetails.set(matchDetails);
        this.loadingMatchDetails.set(false);
      },
      error: (error) => {
        this.messageService.add({
          severity: 'error',
          summary: 'Error',
          detail: error.error?.error || error.message || 'Failed to load match details.',
        });
        this.loadingMatchDetails.set(false);
        this.showMatchDetailsModal.set(false);
        console.error('Error loading match details:', error);
      },
    });
  }

  closeMatchDetailsModal(): void {
    this.selectedMatchDetails.set(null);
    this.showMatchDetailsModal.set(false);
  }

  getResultDisplay(result: GameResult | string): string {
    if (typeof result === 'string') {
      switch (result) {
        case 'WhiteWin':
          return '1-0 (White Wins)';
        case 'BlackWin':
          return '0-1 (Black Wins)';
        case 'Draw':
          return '½-½ (Draw)';
        case 'Forfeit':
          return 'Forfeit';
        case 'Ongoing':
          return 'Ongoing';
        default:
          return 'Unknown';
      }
    }

    switch (result) {
      case GameResult.WhiteWin:
        return '1-0 (White Wins)';
      case GameResult.BlackWin:
        return '0-1 (Black Wins)';
      case GameResult.Draw:
        return '½-½ (Draw)';
      case GameResult.Forfeit:
        return 'Forfeit';
      case GameResult.Ongoing:
        return 'Ongoing';
      default:
        return 'Unknown';
    }
  }

  getMatchResultSeverity(
    result: GameResult | string,
  ): 'success' | 'info' | 'warn' | 'danger' | 'secondary' | 'contrast' {
    if (typeof result === 'string') {
      switch (result) {
        case 'WhiteWin':
          return 'contrast';
        case 'BlackWin':
          return 'secondary';
        case 'Draw':
          return 'info';
        default:
          return 'warn';
      }
    }

    switch (result) {
      case GameResult.WhiteWin:
        return 'contrast';
      case GameResult.BlackWin:
        return 'secondary';
      case GameResult.Draw:
        return 'info';
      default:
        return 'warn';
    }
  }

  getShortResult(result: GameResult | string): string {
    if (typeof result === 'string') {
      switch (result) {
        case 'WhiteWin':
          return '1-0';
        case 'BlackWin':
          return '0-1';
        case 'Draw':
          return '½-½';
        default:
          return '-';
      }
    }

    switch (result) {
      case GameResult.WhiteWin:
        return '1-0';
      case GameResult.BlackWin:
        return '0-1';
      case GameResult.Draw:
        return '½-½';
      default:
        return '-';
    }
  }
}
