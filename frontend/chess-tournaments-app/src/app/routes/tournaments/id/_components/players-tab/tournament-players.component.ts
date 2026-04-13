import { Component, OnInit, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute } from '@angular/router';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { TableModule } from 'primeng/table';
import { ButtonModule } from 'primeng/button';
import { CardModule } from 'primeng/card';
import { TagModule } from 'primeng/tag';
import { InputTextModule } from 'primeng/inputtext';
import { InputNumberModule } from 'primeng/inputnumber';
import { ToastModule } from 'primeng/toast';
import { TooltipModule } from 'primeng/tooltip';
import { MessageService } from 'primeng/api';
import { TournamentsService } from '@app/infrastructure/api/services/tournaments.service';
import { TournamentPlayerDto } from '@app/infrastructure/api/models/tournament-player-dto';
import { AuthorizationService, Permission } from '@app/auth/authorization.service';

@Component({
  selector: 'app-tournament-players',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    TableModule,
    ButtonModule,
    CardModule,
    TagModule,
    InputTextModule,
    InputNumberModule,
    ToastModule,
    TooltipModule,
  ],
  providers: [MessageService],
  templateUrl: './tournament-players.component.html',
  styleUrls: ['./tournament-players.component.css'],
})
export class TournamentPlayersComponent implements OnInit {
  private route = inject(ActivatedRoute);
  private tournamentsApi = inject(TournamentsService);
  private authorizationService = inject(AuthorizationService);
  private fb = inject(FormBuilder);
  private messageService = inject(MessageService);

  tournamentId = signal<string>('');
  players = signal<TournamentPlayerDto[]>([]);
  loading = signal(false);
  error = signal<string | null>(null);
  showRegisterForm = signal(false);
  registerForm!: FormGroup;
  registering = signal(false);

  Permission = Permission;

  ngOnInit(): void {
    this.route.parent?.params.subscribe((params) => {
      const id = params['id'];
      if (id) {
        this.tournamentId.set(id);
        this.loadPlayers();
        this.initRegisterForm();
      }
    });
  }

  initRegisterForm(): void {
    this.registerForm = this.fb.group({
      playerId: ['', Validators.required],
      playerName: ['', [Validators.required, Validators.maxLength(200)]],
      rating: [null, [Validators.min(0), Validators.max(3000)]],
    });
  }

  loadPlayers(): void {
    this.loading.set(true);
    this.error.set(null);

    this.tournamentsApi.apiTournamentsIdPlayersGet({ id: this.tournamentId() }).subscribe({
      next: (players) => {
        this.players.set(players);
        this.loading.set(false);
      },
      error: (error) => {
        this.error.set('Failed to load players.');
        this.loading.set(false);
        console.error('Error loading players:', error);
      },
    });
  }

  toggleRegisterForm(): void {
    this.showRegisterForm.update((v) => !v);
    if (!this.showRegisterForm()) {
      this.registerForm.reset();
    }
  }

  canRegisterPlayer(): boolean {
    return this.authorizationService.hasPermission(Permission.ManageTournament);
  }

  onRegisterPlayer(): void {
    if (this.registerForm.invalid) {
      Object.keys(this.registerForm.controls).forEach((key) => {
        this.registerForm.get(key)?.markAsTouched();
      });
      return;
    }

    this.messageService.add({
      severity: 'info',
      summary: 'Info',
      detail: 'Player registration through tournament requests.',
    });
    this.registerForm.reset();
    this.showRegisterForm.set(false);
  }

  getPlayerRank(index: number): number {
    return index + 1;
  }

  isFieldInvalid(fieldName: string): boolean {
    const field = this.registerForm.get(fieldName);
    return field ? field.invalid && field.touched : false;
  }
}
