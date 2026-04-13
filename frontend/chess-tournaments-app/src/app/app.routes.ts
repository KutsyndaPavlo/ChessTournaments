import { Routes } from '@angular/router';
import { authGuard } from '@app/auth/auth.guard';
import { nonAdminGuard } from '@app/auth/non-admin.guard';
import { canComponentDeactivate } from '@app/infrastructure/routing/can-deactivate.guard';
import { tournamentResolver } from '@app/routes/tournaments/id/tournament.resolver';

export const routes: Routes = [
  {
    path: '',
    loadComponent: () => import('@app/routes/home/home.component').then((m) => m.HomeComponent),
  },
  {
    path: 'register',
    loadComponent: () => import('@app/routes/register/register').then((m) => m.Register),
  },
  {
    path: 'callback',
    loadComponent: () =>
      import('@app/routes/callback/callback.component').then((m) => m.CallbackComponent),
  },
  {
    path: 'dashboard',
    redirectTo: '',
    pathMatch: 'full',
  },
  {
    path: 'tournaments',
    canActivate: [authGuard],
    children: [
      {
        path: '',
        loadComponent: () =>
          import('@app/routes/tournaments/index/tournament-list.component').then(
            (m) => m.TournamentListComponent,
          ),
        data: { breadcrumb: { label: 'Tournaments' } },
      },
      {
        path: 'create',
        loadComponent: () =>
          import('@app/routes/tournaments/create/tournament-create.component').then(
            (m) => m.TournamentCreateComponent,
          ),
        canDeactivate: [canComponentDeactivate],
        data: { breadcrumb: { label: 'Create Tournament' } },
      },
      {
        path: ':id',
        resolve: { tournament: tournamentResolver },
        loadComponent: () =>
          import('@app/routes/tournaments/id/tournament-detail.component').then(
            (m) => m.TournamentDetailComponent,
          ),
        data: { breadcrumb: { label: 'Tournament Details' } },
        children: [
          {
            path: '',
            redirectTo: 'players',
            pathMatch: 'full',
          },
          {
            path: 'players',
            loadComponent: () =>
              import('@app/routes/tournaments/id/_components/players-tab/tournament-players.component').then(
                (m) => m.TournamentPlayersComponent,
              ),
            data: { breadcrumb: { label: 'Players' } },
          },
          {
            path: 'rounds',
            loadComponent: () =>
              import('@app/routes/tournaments/id/_components/rounds-tab/tournament-rounds.component').then(
                (m) => m.TournamentRoundsComponent,
              ),
            data: { breadcrumb: { label: 'Rounds' } },
          },
        ],
      },
    ],
  },
  {
    path: 'tournament-requests',
    canActivate: [authGuard],
    children: [
      {
        path: 'create',
        loadComponent: () =>
          import('@app/routes/tournament-requests/create/tournament-request-create.component').then(
            (m) => m.TournamentRequestCreateComponent,
          ),
        canDeactivate: [canComponentDeactivate],
        data: { breadcrumb: { label: 'Create Request' } },
      },
      {
        path: 'my-requests',
        loadComponent: () =>
          import('@app/routes/tournament-requests/my-requests/tournament-request-list.component').then(
            (m) => m.TournamentRequestListComponent,
          ),
        data: { breadcrumb: { label: 'My Requests' } },
      },
      {
        path: 'admin',
        loadComponent: () =>
          import('@app/routes/tournament-requests/admin/tournament-request-admin.component').then(
            (m) => m.TournamentRequestAdminComponent,
          ),
        data: { breadcrumb: { label: 'Admin' } },
      },
    ],
  },
  {
    path: 'matches',
    canActivate: [authGuard],
    children: [
      {
        path: 'search',
        loadComponent: () =>
          import('@app/routes/matches/search/match-search.component').then(
            (m) => m.MatchSearchComponent,
          ),
        data: { breadcrumb: { label: 'Search Matches' } },
      },
    ],
  },
  {
    path: 'my-matches',
    loadComponent: () =>
      import('@app/routes/players/my-matches/my-matches.component').then(
        (m) => m.MyMatchesComponent,
      ),
    canActivate: [authGuard, nonAdminGuard],
    data: { breadcrumb: { label: 'My Matches' } },
  },
  {
    path: 'my-achievements',
    loadComponent: () =>
      import('@app/routes/players/my-achievements/my-achievements.component').then(
        (m) => m.MyAchievementsComponent,
      ),
    canActivate: [authGuard, nonAdminGuard],
    data: { breadcrumb: { label: 'My Achievements' } },
  },
  {
    path: '**',
    redirectTo: '',
  },
];
