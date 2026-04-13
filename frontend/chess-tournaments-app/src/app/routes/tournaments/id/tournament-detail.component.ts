import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { CardModule } from 'primeng/card';
import { ButtonModule } from 'primeng/button';
import { TagModule } from 'primeng/tag';
import { TabsModule } from 'primeng/tabs';
import { ProgressBarModule } from 'primeng/progressbar';
import { ToastModule } from 'primeng/toast';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { MessageService, ConfirmationService } from 'primeng/api';
import { TournamentsService } from '@app/infrastructure/api/services/tournaments.service';
import { TournamentRequestsService } from '@app/infrastructure/api/services/tournament-requests.service';
import { TournamentDto } from '@app/infrastructure/api/models/tournament-dto';
import { TournamentStatus } from '@app/infrastructure/api/models/tournament-status';
import { TournamentFormat } from '@app/infrastructure/api/models/tournament-format';
import { TimeControl } from '@app/infrastructure/api/models/time-control';
import { CreateTournamentRequestCommand } from '@app/infrastructure/api/models/create-tournament-request-command';
import { AuthorizationService, Permission } from '@app/auth/authorization.service';
import { AuthService } from '@app/auth/auth.service';

@Component({
  selector: 'app-tournament-detail',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    CardModule,
    ButtonModule,
    TagModule,
    TabsModule,
    ProgressBarModule,
    ToastModule,
    ConfirmDialogModule,
  ],
  providers: [MessageService, ConfirmationService],
  templateUrl: './tournament-detail.component.html',
  styleUrls: ['./tournament-detail.component.css'],
})
export class TournamentDetailComponent implements OnInit {
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private tournamentsApi = inject(TournamentsService);
  private tournamentRequestsApi = inject(TournamentRequestsService);
  private authorizationService = inject(AuthorizationService);
  private authService = inject(AuthService);
  private messageService = inject(MessageService);
  private confirmationService = inject(ConfirmationService);

  tournament = signal<TournamentDto | null>(null);
  loading = signal(false);
  error = signal<string | null>(null);
  activeTab = signal<string>('0');
  requestLoading = signal(false);

  TournamentStatus = TournamentStatus;
  Permission = Permission;

  ngOnInit(): void {
    this.route.params.subscribe((params) => {
      const id = params['id'];
      if (id) {
        this.loadTournament(id);
      }
    });
  }

  loadTournament(id: string): void {
    this.loading.set(true);
    this.error.set(null);

    this.tournamentsApi.apiTournamentsIdGet({ id }).subscribe({
      next: (tournament) => {
        this.tournament.set(tournament);
        this.loading.set(false);
      },
      error: (error) => {
        console.error('Error loading tournament:', error);
        if (error.status === 401) {
          this.error.set('Authentication required. Please log in.');
        } else if (error.status === 0) {
          this.error.set('Cannot connect to server. Please check if the API is running.');
        } else {
          this.error.set(`Failed to load tournament: ${error.message || error.statusText}`);
        }
        this.loading.set(false);
      },
    });
  }

  getStatusSeverity(status: string): 'success' | 'info' | 'warn' | 'danger' | 'secondary' {
    switch (status) {
      case TournamentStatus.Registration:
        return 'info';
      case TournamentStatus.InProgress:
        return 'warn';
      case TournamentStatus.Completed:
        return 'success';
      case TournamentStatus.Cancelled:
        return 'danger';
      default:
        return 'secondary';
    }
  }

  getFormatDisplay(format: string): string {
    switch (format) {
      case TournamentFormat.Swiss:
        return 'Swiss';
      case TournamentFormat.RoundRobin:
        return 'Round Robin';
      case TournamentFormat.Knockout:
        return 'Knockout';
      default:
        return format;
    }
  }

  getTimeControlDisplay(timeControl: string): string {
    switch (timeControl) {
      case TimeControl.Bullet:
        return 'Bullet';
      case TimeControl.Blitz:
        return 'Blitz';
      case TimeControl.Rapid:
        return 'Rapid';
      case TimeControl.Classical:
        return 'Classical';
      default:
        return timeControl;
    }
  }

  getPlayerProgress(): number {
    const t = this.tournament();
    if (!t || !t.settings?.maxPlayers) return 0;
    const playerCount =
      typeof t.playerCount === 'number' ? t.playerCount : Number(t.playerCount) || 0;
    const maxPlayers =
      typeof t.settings.maxPlayers === 'number'
        ? t.settings.maxPlayers
        : Number(t.settings.maxPlayers) || 1;
    return Math.round((playerCount / maxPlayers) * 100);
  }

  getRoundProgress(): number {
    const t = this.tournament();
    if (!t || !t.settings?.numberOfRounds) return 0;
    const roundCount = typeof t.roundCount === 'number' ? t.roundCount : Number(t.roundCount) || 0;
    const numberOfRounds =
      typeof t.settings.numberOfRounds === 'number'
        ? t.settings.numberOfRounds
        : Number(t.settings.numberOfRounds) || 1;
    return Math.round((roundCount / numberOfRounds) * 100);
  }

  canOpenRegistration(): boolean {
    return (
      this.tournament()?.status === TournamentStatus.Draft &&
      this.authorizationService.hasPermission(Permission.ManageTournament)
    );
  }

  canCloseRegistration(): boolean {
    return (
      this.tournament()?.status === TournamentStatus.Registration &&
      this.authorizationService.hasPermission(Permission.ManageTournament)
    );
  }

  canCompleteTournament(): boolean {
    return (
      this.tournament()?.status === TournamentStatus.InProgress &&
      this.authorizationService.hasPermission(Permission.ManageTournament)
    );
  }

  canCancelTournament(): boolean {
    return (
      this.tournament()?.status !== TournamentStatus.Completed &&
      this.tournament()?.status !== TournamentStatus.Cancelled &&
      this.authorizationService.hasPermission(Permission.ManageTournament)
    );
  }

  canRequestToJoin(): boolean {
    return (
      this.tournament()?.status === TournamentStatus.Registration &&
      !this.authorizationService.hasPermission(Permission.ManageTournament)
    );
  }

  openRegistration(): void {
    const tournament = this.tournament();
    if (!tournament || !tournament.id || !this.canOpenRegistration()) return;

    this.tournamentsApi.apiTournamentsIdOpenRegistrationPost({ id: tournament.id }).subscribe({
      next: () => {
        this.messageService.add({
          severity: 'success',
          summary: 'Success',
          detail: 'Registration has been opened.',
        });
        this.loadTournament(tournament.id!);
      },
      error: (error) => {
        this.messageService.add({
          severity: 'error',
          summary: 'Error',
          detail: 'Failed to open registration.',
        });
        console.error('Error:', error);
      },
    });
  }

  closeRegistration(): void {
    const tournament = this.tournament();
    if (!tournament || !tournament.id || !this.canCloseRegistration()) return;

    const tournamentId = tournament.id;
    this.confirmationService.confirm({
      message:
        'Are you sure you want to close registration and start the tournament? The first round will be automatically created with pairings.',
      header: 'Start Tournament',
      icon: 'pi pi-play',
      acceptButtonProps: { severity: 'primary' },
      accept: () => {
        this.tournamentsApi.apiTournamentsIdCloseRegistrationPost({ id: tournamentId }).subscribe({
          next: () => {
            this.messageService.add({
              severity: 'success',
              summary: 'Success',
              detail: 'Tournament has started! First round created.',
            });
            this.loadTournament(tournamentId);
            this.activeTab.set('2'); // Switch to rounds tab
          },
          error: (error) => {
            this.messageService.add({
              severity: 'error',
              summary: 'Error',
              detail: 'Failed to start tournament.',
            });
            console.error('Error:', error);
          },
        });
      },
    });
  }

  completeTournament(): void {
    const tournament = this.tournament();
    if (!tournament || !tournament.id || !this.canCompleteTournament()) return;

    const tournamentId = tournament.id;
    this.confirmationService.confirm({
      message: 'Are you sure you want to complete this tournament?',
      header: 'Complete Tournament',
      icon: 'pi pi-check-circle',
      acceptButtonProps: { severity: 'success' },
      accept: () => {
        this.tournamentsApi.apiTournamentsIdCompletePost({ id: tournamentId }).subscribe({
          next: () => {
            this.messageService.add({
              severity: 'success',
              summary: 'Success',
              detail: 'Tournament has been completed.',
            });
            this.loadTournament(tournamentId);
          },
          error: (error) => {
            this.messageService.add({
              severity: 'error',
              summary: 'Error',
              detail: 'Failed to complete tournament.',
            });
            console.error('Error:', error);
          },
        });
      },
    });
  }

  cancelTournament(): void {
    const tournament = this.tournament();
    if (!tournament || !tournament.id || !this.canCancelTournament()) return;

    const tournamentId = tournament.id;
    this.confirmationService.confirm({
      message: 'Are you sure you want to cancel this tournament? This action cannot be undone.',
      header: 'Cancel Tournament',
      icon: 'pi pi-exclamation-triangle',
      acceptButtonProps: { severity: 'danger' },
      accept: () => {
        this.tournamentsApi.apiTournamentsIdCancelPost({ id: tournamentId }).subscribe({
          next: () => {
            this.messageService.add({
              severity: 'warn',
              summary: 'Cancelled',
              detail: 'Tournament has been cancelled.',
            });
            this.loadTournament(tournamentId);
          },
          error: (error) => {
            this.messageService.add({
              severity: 'error',
              summary: 'Error',
              detail: 'Failed to cancel tournament.',
            });
            console.error('Error:', error);
          },
        });
      },
    });
  }

  goBack(): void {
    this.router.navigate(['/tournaments']);
  }

  onTabChange(value: string | number | undefined): void {
    this.activeTab.set(String(value ?? '0'));
  }

  navigateToPlayers(): void {
    const tournament = this.tournament();
    if (tournament) {
      this.router.navigate(['/tournaments', tournament.id, 'players']);
    }
  }

  navigateToRounds(): void {
    const tournament = this.tournament();
    if (tournament) {
      this.router.navigate(['/tournaments', tournament.id, 'rounds']);
    }
  }

  requestToJoinTournament(): void {
    const tournament = this.tournament();
    if (!tournament || !tournament.id) return;

    const userId = this.authService.getUserId();
    if (!userId) {
      this.messageService.add({
        severity: 'error',
        summary: 'Error',
        detail: 'User not authenticated',
      });
      return;
    }

    const request: CreateTournamentRequestCommand = {
      tournamentId: tournament.id,
      requestedBy: userId,
    };

    this.requestLoading.set(true);

    this.tournamentRequestsApi.apiTournamentRequestsPost({ body: request }).subscribe({
      next: () => {
        this.requestLoading.set(false);
        this.messageService.add({
          severity: 'success',
          summary: 'Request Submitted',
          detail: 'Your request to join this tournament has been submitted!',
        });
        setTimeout(() => {
          this.router.navigate(['/tournament-requests/my-requests']);
        }, 1500);
      },
      error: (error) => {
        this.requestLoading.set(false);
        this.messageService.add({
          severity: 'error',
          summary: 'Error',
          detail: 'Failed to submit request: ' + (error.error?.error || error.message),
        });
      },
    });
  }
}
