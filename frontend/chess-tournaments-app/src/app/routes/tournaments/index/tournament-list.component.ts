import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { TableModule, TableLazyLoadEvent } from 'primeng/table';
import { ButtonModule } from 'primeng/button';
import { TagModule } from 'primeng/tag';
import { SelectModule } from 'primeng/select';
import { TooltipModule } from 'primeng/tooltip';
import { TournamentsService } from '@app/infrastructure/api/services/tournaments.service';
import { TournamentDto } from '@app/infrastructure/api/models/tournament-dto';
import { TournamentStatus } from '@app/infrastructure/api/models/tournament-status';
import { TournamentFormat } from '@app/infrastructure/api/models/tournament-format';
import { HasPermissionDirective } from '@app/auth/has-permission.directive';
import { Permission } from '@app/auth/authorization.service';

interface StatusOption {
  label: string;
  value: string;
}

@Component({
  selector: 'app-tournament-list',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    TableModule,
    ButtonModule,
    TagModule,
    SelectModule,
    TooltipModule,
    HasPermissionDirective,
  ],
  templateUrl: './tournament-list.component.html',
  styleUrls: ['./tournament-list.component.css'],
})
export class TournamentListComponent implements OnInit {
  private tournamentsApi = inject(TournamentsService);
  private router = inject(Router);

  tournaments = signal<TournamentDto[]>([]);
  allTournaments = signal<TournamentDto[]>([]);
  loading = signal(false);
  error = signal<string | null>(null);
  selectedStatus = signal<string | null>(null);

  // Pagination state
  totalCount = signal(0);
  first = signal(0);
  rows = signal(10);

  TournamentStatus = TournamentStatus;
  Permission = Permission;

  statusOptions: StatusOption[] = [
    { label: 'All Statuses', value: '' },
    { label: 'Registration', value: TournamentStatus.Registration },
    { label: 'In Progress', value: TournamentStatus.InProgress },
    { label: 'Completed', value: TournamentStatus.Completed },
    { label: 'Cancelled', value: TournamentStatus.Cancelled },
  ];

  rowsPerPageOptions = [5, 10, 20, 50];

  ngOnInit(): void {
    this.loadTournaments();
  }

  loadTournaments(event?: TableLazyLoadEvent): void {
    this.loading.set(true);
    this.error.set(null);

    if (event) {
      this.first.set(event.first ?? 0);
      this.rows.set(event.rows ?? 10);
    }

    this.tournamentsApi.apiTournamentsGet().subscribe({
      next: (tournaments: TournamentDto[]) => {
        this.allTournaments.set(tournaments);
        this.totalCount.set(tournaments.length);
        this.applyPagination();
        this.loading.set(false);
      },
      error: (error: { status?: number; message?: string; statusText?: string }) => {
        this.handleError(error);
      },
    });
  }

  private applyPagination(): void {
    const all = this.allTournaments();
    const start = this.first();
    const end = start + this.rows();
    this.tournaments.set(all.slice(start, end));
  }

  onLazyLoad(event: TableLazyLoadEvent): void {
    this.first.set(event.first ?? 0);
    this.rows.set(event.rows ?? 10);
    this.applyPagination();
  }

  private handleError(error: { status?: number; message?: string; statusText?: string }): void {
    console.error('Error loading tournaments:', error);
    if (error.status === 401) {
      this.error.set('Authentication required. Please log in.');
    } else if (error.status === 0) {
      this.error.set('Cannot connect to server. Please check if the API is running.');
    } else {
      this.error.set(`Failed to load tournaments: ${error.message || error.statusText}`);
    }
    this.loading.set(false);
  }

  onStatusFilterChange(): void {
    this.first.set(0);
    this.loadTournaments();
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

  viewTournament(tournament: TournamentDto): void {
    this.router.navigate(['/tournaments', tournament.id]);
  }

  createTournament(): void {
    this.router.navigate(['/tournaments/create']);
  }
}
