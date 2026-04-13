import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { TableModule } from 'primeng/table';
import { ButtonModule } from 'primeng/button';
import { CardModule } from 'primeng/card';
import { InputTextModule } from 'primeng/inputtext';
import { TagModule } from 'primeng/tag';
import { DialogModule } from 'primeng/dialog';
import { ToastModule } from 'primeng/toast';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { TooltipModule } from 'primeng/tooltip';
import { ChipModule } from 'primeng/chip';
import { MessageService, ConfirmationService } from 'primeng/api';
import { MatchesService } from '@app/infrastructure/api/services/matches.service';
import { TournamentMatchDto } from '@app/infrastructure/api/models/tournament-match-dto';
import { GameResult } from '@app/infrastructure/api/models/game-result';

@Component({
  selector: 'app-match-search',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    RouterModule,
    TableModule,
    ButtonModule,
    CardModule,
    InputTextModule,
    TagModule,
    DialogModule,
    ToastModule,
    ConfirmDialogModule,
    TooltipModule,
    ChipModule,
  ],
  providers: [MessageService, ConfirmationService],
  templateUrl: './match-search.component.html',
  styleUrl: './match-search.component.scss',
})
export class MatchSearchComponent {
  private matchesApi = inject(MatchesService);
  private messageService = inject(MessageService);
  private confirmationService = inject(ConfirmationService);

  matches = signal<TournamentMatchDto[]>([]);
  loading = signal(false);
  searchPlayerId = signal('');
  searchTags = signal('');
  searchTournamentId = signal('');
  showTagModal = signal(false);
  selectedMatch = signal<TournamentMatchDto | null>(null);
  newTagName = signal('');
  addingTag = signal(false);

  searchMatches(): void {
    this.loading.set(true);

    const tags = this.searchTags()
      .split(',')
      .map((t) => t.trim())
      .filter((t) => t.length > 0);

    this.matchesApi
      .searchMatches({
        playerId: this.searchPlayerId() || undefined,
        tags: tags.length > 0 ? tags.join(',') : undefined,
        tournamentId: this.searchTournamentId() || undefined,
      })
      .subscribe({
        next: (matches: TournamentMatchDto[]) => {
          this.matches.set(matches);
          this.loading.set(false);
        },
        error: (error: unknown) => {
          console.error('Error searching matches:', error);
          this.loading.set(false);
          this.messageService.add({
            severity: 'error',
            summary: 'Error',
            detail: 'Failed to search matches',
          });
        },
      });
  }

  clearSearch(): void {
    this.searchPlayerId.set('');
    this.searchTags.set('');
    this.searchTournamentId.set('');
    this.matches.set([]);
  }

  getResultDisplay(result: GameResult | string): string {
    if (typeof result === 'string') {
      switch (result) {
        case 'WhiteWin':
          return '1-0';
        case 'BlackWin':
          return '0-1';
        case 'Draw':
          return '½-½';
        case 'Forfeit':
          return 'Forfeit';
        case 'Ongoing':
        default:
          return 'Ongoing';
      }
    }

    switch (result) {
      case GameResult.WhiteWin:
        return '1-0';
      case GameResult.BlackWin:
        return '0-1';
      case GameResult.Draw:
        return '½-½';
      case GameResult.Forfeit:
        return 'Forfeit';
      case GameResult.Ongoing:
      default:
        return 'Ongoing';
    }
  }

  getResultSeverity(
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

  openTagModal(match: TournamentMatchDto): void {
    this.selectedMatch.set(match);
    this.newTagName.set('');
    this.showTagModal.set(true);
  }

  closeTagModal(): void {
    this.showTagModal.set(false);
    this.selectedMatch.set(null);
    this.newTagName.set('');
  }

  addTag(): void {
    const match = this.selectedMatch();
    const tagName = this.newTagName().trim();
    if (!match || !tagName) {
      return;
    }

    this.addingTag.set(true);

    this.matchesApi.addMatchTag({ matchId: match.id!, body: { tagName } }).subscribe({
      next: () => {
        this.addingTag.set(false);
        this.newTagName.set('');
        this.closeTagModal();
        this.searchMatches();
        this.messageService.add({
          severity: 'success',
          summary: 'Success',
          detail: 'Tag added successfully',
        });
      },
      error: (error: { error?: { message?: string } }) => {
        console.error('Error adding tag:', error);
        this.addingTag.set(false);
        this.messageService.add({
          severity: 'error',
          summary: 'Error',
          detail: 'Failed to add tag: ' + (error.error?.message || 'Unknown error'),
        });
      },
    });
  }

  removeTag(match: TournamentMatchDto, tagName: string): void {
    this.confirmationService.confirm({
      message: `Are you sure you want to remove the tag "${tagName}"?`,
      header: 'Remove Tag',
      icon: 'pi pi-exclamation-triangle',
      acceptButtonProps: { severity: 'danger' },
      accept: () => {
        this.matchesApi.removeMatchTag({ matchId: match.id!, tagName }).subscribe({
          next: () => {
            this.searchMatches();
            this.messageService.add({
              severity: 'success',
              summary: 'Success',
              detail: 'Tag removed successfully',
            });
          },
          error: (error: { error?: { message?: string } }) => {
            console.error('Error removing tag:', error);
            this.messageService.add({
              severity: 'error',
              summary: 'Error',
              detail: 'Failed to remove tag: ' + (error.error?.message || 'Unknown error'),
            });
          },
        });
      },
    });
  }
}
