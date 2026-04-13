import { Component, OnInit, inject, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { CardModule } from 'primeng/card';
import { ButtonModule } from 'primeng/button';
import { SelectModule } from 'primeng/select';
import { ToastModule } from 'primeng/toast';
import { ProgressBarModule } from 'primeng/progressbar';
import { MessageService } from 'primeng/api';
import { TournamentRequestsService } from '@app/infrastructure/api/services/tournament-requests.service';
import { TournamentsService } from '@app/infrastructure/api/services/tournaments.service';
import { TournamentDto } from '@app/infrastructure/api/models/tournament-dto';
import { CreateTournamentRequestCommand } from '@app/infrastructure/api/models/create-tournament-request-command';
import { AuthService } from '@app/auth/auth.service';

interface TournamentOption {
  label: string;
  value: string;
  tournament: TournamentDto;
}

@Component({
  selector: 'app-tournament-request-create',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    RouterLink,
    CardModule,
    ButtonModule,
    SelectModule,
    ToastModule,
    ProgressBarModule,
  ],
  providers: [MessageService],
  templateUrl: './tournament-request-create.component.html',
  styleUrl: './tournament-request-create.component.css',
})
export class TournamentRequestCreateComponent implements OnInit {
  private fb = inject(FormBuilder);
  private tournamentRequestsApi = inject(TournamentRequestsService);
  private tournamentsApi = inject(TournamentsService);
  private authService = inject(AuthService);
  private router = inject(Router);
  private messageService = inject(MessageService);

  requestForm!: FormGroup;
  tournaments = signal<TournamentDto[]>([]);
  loading = signal(false);
  loadingTournaments = signal(false);
  error = signal<string | null>(null);

  tournamentOptions = computed<TournamentOption[]>(() =>
    this.tournaments().map((t) => ({
      label: `${t.name} - ${new Date(t.startDate!).toLocaleDateString()} - ${t.location}`,
      value: t.id!,
      tournament: t,
    })),
  );

  selectedTournament = computed<TournamentDto | null>(() => {
    const tournamentId = this.requestForm?.get('tournamentId')?.value;
    return this.tournaments().find((t) => t.id === tournamentId) || null;
  });

  ngOnInit(): void {
    this.initForm();
    this.loadTournaments();
  }

  private initForm(): void {
    this.requestForm = this.fb.group({
      tournamentId: ['', Validators.required],
    });
  }

  private loadTournaments(): void {
    this.loadingTournaments.set(true);
    this.tournamentsApi.apiTournamentsGet().subscribe({
      next: (tournaments: TournamentDto[]) => {
        // Only show tournaments in Registration status
        this.tournaments.set(tournaments.filter((t: TournamentDto) => t.status === 'Registration'));
        this.loadingTournaments.set(false);
      },
      error: (error: unknown) => {
        console.error('Error loading tournaments:', error);
        this.error.set('Failed to load tournaments.');
        this.loadingTournaments.set(false);
      },
    });
  }

  onSubmit(): void {
    if (this.requestForm.invalid) {
      this.requestForm.markAllAsTouched();
      return;
    }

    const userId = this.authService.getUserId();
    if (!userId) {
      this.error.set('User not authenticated');
      return;
    }

    const request: CreateTournamentRequestCommand = {
      tournamentId: this.requestForm.value.tournamentId,
      requestedBy: userId,
    };

    this.loading.set(true);
    this.error.set(null);

    this.tournamentRequestsApi.apiTournamentRequestsPost({ body: request }).subscribe({
      next: () => {
        this.messageService.add({
          severity: 'success',
          summary: 'Success',
          detail: 'Tournament request submitted successfully!',
        });
        this.router.navigate(['/tournament-requests/my-requests']);
      },
      error: (error: { error?: { error?: string }; message?: string }) => {
        this.error.set(
          error.error?.error || error.message || 'Failed to submit tournament request.',
        );
        this.loading.set(false);
        console.error('Error creating request:', error);
      },
    });
  }

  getSelectedTournament(): TournamentDto | null {
    const tournamentId = this.requestForm.get('tournamentId')?.value;
    return this.tournaments().find((t) => t.id === tournamentId) || null;
  }
}
