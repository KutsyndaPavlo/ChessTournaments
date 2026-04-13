import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { CardModule } from 'primeng/card';
import { ButtonModule } from 'primeng/button';
import { TagModule } from 'primeng/tag';
import { DialogModule } from 'primeng/dialog';
import { ToastModule } from 'primeng/toast';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { TextareaModule } from 'primeng/textarea';
import { ProgressBarModule } from 'primeng/progressbar';
import { MessageService, ConfirmationService } from 'primeng/api';
import { TournamentRequestsService } from '@app/infrastructure/api/services/tournament-requests.service';
import { TournamentsService } from '@app/infrastructure/api/services/tournaments.service';
import { TournamentRequestDto } from '@app/infrastructure/api/models/tournament-request-dto';
import { TournamentDto } from '@app/infrastructure/api/models/tournament-dto';
import { RequestStatus } from '@app/infrastructure/api/models/request-status';
import { of, timeout } from 'rxjs';
import { catchError } from 'rxjs/operators';

interface TournamentRequestWithDetails extends TournamentRequestDto {
  tournament?: TournamentDto;
}

@Component({
  selector: 'app-tournament-request-admin',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    CardModule,
    ButtonModule,
    TagModule,
    DialogModule,
    ToastModule,
    ConfirmDialogModule,
    TextareaModule,
    ProgressBarModule,
  ],
  providers: [MessageService, ConfirmationService],
  templateUrl: './tournament-request-admin.component.html',
  styleUrl: './tournament-request-admin.component.css',
})
export class TournamentRequestAdminComponent implements OnInit {
  private tournamentRequestsApi = inject(TournamentRequestsService);
  private tournamentsApi = inject(TournamentsService);
  private messageService = inject(MessageService);
  private confirmationService = inject(ConfirmationService);

  requests = signal<TournamentRequestWithDetails[]>([]);
  loading = signal(false);
  error = signal<string | null>(null);
  processingRequestId = signal<string | null>(null);
  selectedRequest = signal<TournamentRequestWithDetails | null>(null);
  rejectionReason = signal('');
  showRejectModal = signal(false);

  RequestStatus = RequestStatus;

  ngOnInit(): void {
    this.loadRequests();
  }

  loadRequests(): void {
    this.loading.set(true);
    this.error.set(null);

    this.tournamentRequestsApi.apiTournamentRequestsPendingGet().subscribe({
      next: (requests: TournamentRequestDto[]) => {
        this.requests.set((requests || []).map((r) => ({ ...r, tournament: undefined })));
        this.loading.set(false);

        if (requests && requests.length > 0) {
          this.fetchTournamentDetailsInBackground(requests);
        }
      },
      error: (error: { error?: { error?: string }; message?: string }) => {
        console.error('Error loading pending requests:', error);
        this.error.set(
          error.error?.error || error.message || 'Failed to load pending tournament requests.',
        );
        this.loading.set(false);
      },
    });
  }

  private fetchTournamentDetailsInBackground(requests: TournamentRequestDto[]): void {
    requests.forEach((request) => {
      this.tournamentsApi
        .apiTournamentsIdGet({ id: request.tournamentId! })
        .pipe(
          timeout(5000),
          catchError(() => of(null)),
        )
        .subscribe({
          next: (tournament: TournamentDto | null) => {
            if (tournament) {
              const currentRequests = this.requests();
              const requestIndex = currentRequests.findIndex((r) => r.id === request.id);
              if (requestIndex !== -1) {
                const updatedRequests = [...currentRequests];
                updatedRequests[requestIndex] = { ...updatedRequests[requestIndex], tournament };
                this.requests.set(updatedRequests);
              }
            }
          },
        });
    });
  }

  approveRequest(request: TournamentRequestWithDetails): void {
    const tournamentName = request.tournament?.name || `tournament ${request.tournamentId}`;

    this.confirmationService.confirm({
      message: `Are you sure you want to approve this request? User ${request.requestedBy} will be added to ${tournamentName}.`,
      header: 'Approve Request',
      icon: 'pi pi-check-circle',
      acceptButtonProps: { severity: 'success' },
      accept: () => {
        this.processingRequestId.set(request.id ?? null);
        this.error.set(null);

        this.tournamentRequestsApi
          .apiTournamentRequestsRequestIdApprovePost({ requestId: request.id! })
          .subscribe({
            next: () => {
              this.processingRequestId.set(null);
              this.messageService.add({
                severity: 'success',
                summary: 'Success',
                detail: 'Request approved successfully!',
              });
              this.loadRequests();
            },
            error: (error: { error?: { error?: string }; message?: string }) => {
              this.messageService.add({
                severity: 'error',
                summary: 'Error',
                detail:
                  error.error?.error || error.message || 'Failed to approve tournament request.',
              });
              this.processingRequestId.set(null);
              console.error('Error approving request:', error);
            },
          });
      },
    });
  }

  openRejectModal(request: TournamentRequestWithDetails): void {
    this.selectedRequest.set(request);
    this.rejectionReason.set('');
    this.showRejectModal.set(true);
  }

  closeRejectModal(): void {
    this.selectedRequest.set(null);
    this.rejectionReason.set('');
    this.showRejectModal.set(false);
  }

  confirmReject(): void {
    const request = this.selectedRequest();
    const reason = this.rejectionReason();

    if (!request || !reason.trim()) {
      return;
    }

    this.processingRequestId.set(request.id ?? null);
    this.error.set(null);

    this.tournamentRequestsApi
      .apiTournamentRequestsRequestIdRejectPost({
        requestId: request.id!,
        body: { rejectionReason: reason },
      })
      .subscribe({
        next: () => {
          this.processingRequestId.set(null);
          this.messageService.add({
            severity: 'success',
            summary: 'Success',
            detail: 'Request rejected successfully.',
          });
          this.closeRejectModal();
          this.loadRequests();
        },
        error: (error: { error?: { error?: string }; message?: string }) => {
          this.messageService.add({
            severity: 'error',
            summary: 'Error',
            detail: error.error?.error || error.message || 'Failed to reject tournament request.',
          });
          this.processingRequestId.set(null);
          console.error('Error rejecting request:', error);
        },
      });
  }

  isProcessing(requestId: string): boolean {
    return this.processingRequestId() === requestId;
  }
}
