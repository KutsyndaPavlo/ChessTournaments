import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthorizationService, Role } from './authorization.service';

/**
 * Guard to protect routes that require specific roles
 * Usage in routes:
 * canActivate: [roleGuard([Role.Admin])]
 */
export const roleGuard = (requiredRoles: Role[]): CanActivateFn => {
  return () => {
    const authorizationService = inject(AuthorizationService);
    const router = inject(Router);

    if (authorizationService.hasAnyRole(requiredRoles)) {
      return true;
    }

    // Redirect to home or unauthorized page
    router.navigate(['/']);
    return false;
  };
};

/**
 * Admin-only guard
 * Usage in routes:
 * canActivate: [adminGuard]
 */
export const adminGuard: CanActivateFn = () => {
  const authorizationService = inject(AuthorizationService);
  const router = inject(Router);

  if (authorizationService.isAdmin) {
    return true;
  }

  router.navigate(['/']);
  return false;
};
