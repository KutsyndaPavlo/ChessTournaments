import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { TableModule } from 'primeng/table';
import { ButtonModule } from 'primeng/button';
import { TagModule } from 'primeng/tag';
import { ToastModule } from 'primeng/toast';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { ProgressBarModule } from 'primeng/progressbar';
import { TooltipModule } from 'primeng/tooltip';
import { MessageService, ConfirmationService } from 'primeng/api';
import { TournamentRequestsService } from '@app/infrastructure/api/services/tournament-requests.service';
import { TournamentRequestDto } from '@app/infrastructure/api/models/tournament-request-dto';
import { RequestStatus } from '@app/infrastructure/api/models/request-status';
import { TournamentsService } from '@app/infrastructure/api/services/tournaments.service';
import { TournamentDto } from '@app/infrastructure/api/models/tournament-dto';
import { forkJoin, of } from 'rxjs';
import { catchError } from 'rxjs/operators';

interface TournamentRequestWithDetails extends TournamentRequestDto {
  tournament?: TournamentDto;
}

@Component({
  selector: 'app-tournament-request-list',
  standalone: true,
  imports: [
    CommonModule,
    RouterLink,
    TableModule,
    ButtonModule,
    TagModule,
    ToastModule,
    ConfirmDialogModule,
    ProgressBarModule,
    TooltipModule,
  ],
  providers: [MessageService, ConfirmationService],
  templateUrl: './tournament-request-list.component.html',
  styleUrl: './tournament-request-list.component.css',
})
export class TournamentRequestListComponent implements OnInit {
  private tournamentRequestsApi = inject(TournamentRequestsService);
  private tournamentsApi = inject(TournamentsService);
  private messageService = inject(MessageService);
  private confirmationService = inject(ConfirmationService);

  requests = signal<TournamentRequestWithDetails[]>([]);
  loading = signal(false);
  error = signal<string | null>(null);

  RequestStatus = RequestStatus;

  ngOnInit(): void {
    this.loadRequests();
  }

  loadRequests(): void {
    this.loading.set(true);
    this.error.set(null);

    this.tournamentRequestsApi.apiTournamentRequestsMyRequestsGet().subscribe({
      next: (requests) => {
        if (requests.length === 0) {
          this.requests.set([]);
          this.loading.set(false);
          return;
        }

        const tournamentFetches = requests.map((request) =>
          this.tournamentsApi
            .apiTournamentsIdGet({ id: request.tournamentId! })
            .pipe(catchError(() => of(null))),
        );

        forkJoin(tournamentFetches).subscribe({
          next: (tournaments) => {
            this.requests.set(
              requests.map((request, index) => ({
                ...request,
                tournament: tournaments[index] || undefined,
              })),
            );
            this.loading.set(false);
          },
          error: (error) => {
            console.error('Error loading tournament details:', error);
            this.requests.set(requests);
            this.loading.set(false);
          },
        });
      },
      error: (error) => {
        this.error.set(
          error.error?.error || error.message || 'Failed to load tournament requests.',
        );
        this.loading.set(false);
        console.error('Error loading requests:', error);
      },
    });
  }

  cancelRequest(requestId: string): void {
    this.confirmationService.confirm({
      message: 'Are you sure you want to cancel this tournament request?',
      header: 'Cancel Request',
      icon: 'pi pi-exclamation-triangle',
      acceptButtonProps: { severity: 'danger' },
      accept: () => {
        this.tournamentRequestsApi
          .apiTournamentRequestsRequestIdCancelPost({ requestId })
          .subscribe({
            next: () => {
              this.messageService.add({
                severity: 'success',
                summary: 'Success',
                detail: 'Tournament request cancelled successfully.',
              });
              this.loadRequests();
            },
            error: (error) => {
              this.messageService.add({
                severity: 'error',
                summary: 'Error',
                detail:
                  error.error?.error || error.message || 'Failed to cancel tournament request.',
              });
              console.error('Error cancelling request:', error);
            },
          });
      },
    });
  }

  getStatusSeverity(
    status: RequestStatus,
  ): 'success' | 'info' | 'warn' | 'danger' | 'secondary' | 'contrast' {
    switch (status) {
      case RequestStatus.Approved:
        return 'success';
      case RequestStatus.Pending:
        return 'warn';
      case RequestStatus.Rejected:
        return 'danger';
      case RequestStatus.Cancelled:
        return 'secondary';
      default:
        return 'info';
    }
  }

  getStatusDisplay(status: RequestStatus): string {
    switch (status) {
      case RequestStatus.Pending:
        return 'Pending';
      case RequestStatus.Approved:
        return 'Approved';
      case RequestStatus.Rejected:
        return 'Rejected';
      case RequestStatus.Cancelled:
        return 'Cancelled';
      default:
        return status;
    }
  }

  canCancel(request: TournamentRequestDto): boolean {
    return request.status === RequestStatus.Pending;
  }
}
