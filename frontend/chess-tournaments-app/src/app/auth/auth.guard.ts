import { inject } from '@angular/core';
import { Router, CanActivateFn } from '@angular/router';
import { AuthService } from './auth.service';
import { map } from 'rxjs/operators';

export const authGuard: CanActivateFn = (route, state) => {
  const authService = inject(AuthService);
  const router = inject(Router);

  return authService.isDoneLoading$.pipe(
    map((isDone) => {
      if (!isDone) {
        return false;
      }

      if (authService.hasValidToken()) {
        return true;
      }

      // Redirect to login
      authService.login(state.url);
      return false;
    }),
  );
};
