import { Injectable, inject } from '@angular/core';
import { Observable, map } from 'rxjs';
import { AuthService } from './auth.service';

export enum Role {
  Admin = 'Admin',
}

export enum Permission {
  ViewTournaments = 'view_tournaments',
  CreateTournament = 'create_tournament',
  UpdateTournament = 'update_tournament',
  DeleteTournament = 'delete_tournament',
  RegisterPlayer = 'register_player',
  ManageTournament = 'manage_tournament',
}

@Injectable({
  providedIn: 'root',
})
export class AuthorizationService {
  private authService = inject(AuthService);

  /**
   * Check if the current user has a specific role
   */
  hasRole(role: Role): boolean {
    const userRoles = this.authService.getUserRoles();
    return userRoles.includes(role);
  }

  /**
   * Check if the current user has any of the specified roles
   */
  hasAnyRole(roles: Role[]): boolean {
    const userRoles = this.authService.getUserRoles();
    return roles.some((role) => userRoles.includes(role));
  }

  /**
   * Check if the current user has all of the specified roles
   */
  hasAllRoles(roles: Role[]): boolean {
    const userRoles = this.authService.getUserRoles();
    return roles.every((role) => userRoles.includes(role));
  }

  /**
   * Check if the current user has a specific permission
   */
  hasPermission(permission: Permission): boolean {
    const userRoles = this.authService.getUserRoles();

    // Admin has all permissions
    if (userRoles.includes(Role.Admin)) {
      return true;
    }

    // Map permissions to roles
    switch (permission) {
      case Permission.ViewTournaments:
        return true;

      case Permission.RegisterPlayer:
        return true;

      case Permission.CreateTournament:
      case Permission.UpdateTournament:
      case Permission.DeleteTournament:
      case Permission.ManageTournament:
        return userRoles.includes(Role.Admin);

      default:
        return false;
    }
  }

  /**
   * Check if the current user has any of the specified permissions
   */
  hasAnyPermission(permissions: Permission[]): boolean {
    return permissions.some((permission) => this.hasPermission(permission));
  }

  /**
   * Check if the current user has all of the specified permissions
   */
  hasAllPermissions(permissions: Permission[]): boolean {
    return permissions.every((permission) => this.hasPermission(permission));
  }

  /**
   * Observable to check if user is admin
   */
  get isAdmin$(): Observable<boolean> {
    return this.authService.isAuthenticated$.pipe(map(() => this.hasRole(Role.Admin)));
  }

  /**
   * Check if user is admin
   */
  get isAdmin(): boolean {
    return this.hasRole(Role.Admin);
  }

  /**
   * Check if user can manage tournaments (admin)
   */
  get canManageTournaments(): boolean {
    return this.hasPermission(Permission.ManageTournament);
  }
}
