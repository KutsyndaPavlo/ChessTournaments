import { inject } from '@angular/core';
import { ResolveFn, Router } from '@angular/router';
import { catchError, EMPTY } from 'rxjs';
import { TournamentsService } from '@app/infrastructure/api/services/tournaments.service';
import { TournamentDto } from '@app/infrastructure/api/models/tournament-dto';

/**
 * Resolver that fetches tournament details before route activation.
 * Redirects to error page if tournament is not found.
 *
 * @example
 * // In route configuration:
 * {
 *   path: ':id',
 *   component: TournamentDetailComponent,
 *   resolve: { tournament: tournamentResolver },
 * }
 *
 * // In component:
 * private readonly route = inject(ActivatedRoute);
 * tournament = toSignal(this.route.data.pipe(map(data => data['tournament'])));
 */
export const tournamentResolver: ResolveFn<TournamentDto> = (route) => {
  const tournamentsApi = inject(TournamentsService);
  const router = inject(Router);
  const id = route.paramMap.get('id')!;

  return tournamentsApi.apiTournamentsIdGet({ id }).pipe(
    catchError(() => {
      router.navigate(['/tournaments']);
      return EMPTY;
    }),
  );
};
