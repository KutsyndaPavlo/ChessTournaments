import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthorizationService, Permission } from './authorization.service';

/**
 * Guard to protect routes that require specific permissions
 * Usage in routes:
 * canActivate: [permissionGuard([Permission.CreateTournament])]
 */
export const permissionGuard = (requiredPermissions: Permission[]): CanActivateFn => {
  return () => {
    const authorizationService = inject(AuthorizationService);
    const router = inject(Router);

    if (authorizationService.hasAllPermissions(requiredPermissions)) {
      return true;
    }

    // Redirect to home or unauthorized page
    router.navigate(['/']);
    return false;
  };
};
