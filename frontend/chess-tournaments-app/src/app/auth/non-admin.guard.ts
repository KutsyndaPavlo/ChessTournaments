import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { map } from 'rxjs/operators';
import { AuthService } from './auth.service';
import { AuthorizationService } from './authorization.service';

export const nonAdminGuard: CanActivateFn = (route, state) => {
  const authService = inject(AuthService);
  const authorizationService = inject(AuthorizationService);
  const router = inject(Router);

  return authService.isDoneLoading$.pipe(
    map((isDone) => {
      if (!isDone) return false;

      if (!authService.hasValidToken()) {
        authService.login(state.url);
        return false;
      }

      // Check if user is NOT an admin
      if (authorizationService.isAdmin) {
        // Redirect admins to tournaments or home page
        router.navigate(['/tournaments']);
        return false;
      }

      return true;
    }),
  );
};
