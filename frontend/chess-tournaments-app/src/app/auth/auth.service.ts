import { Injectable, inject } from '@angular/core';
import { Router } from '@angular/router';
import { OAuthService, OAuthErrorEvent } from 'angular-oauth2-oidc';
import { BehaviorSubject, Observable, filter } from 'rxjs';
import { authConfig } from './auth.config';

@Injectable({
  providedIn: 'root',
})
export class AuthService {
  private oauthService = inject(OAuthService);
  private router = inject(Router);

  private isAuthenticatedSubject$ = new BehaviorSubject<boolean>(false);
  public isAuthenticated$ = this.isAuthenticatedSubject$.asObservable();

  private isDoneLoadingSubject$ = new BehaviorSubject<boolean>(false);
  public isDoneLoading$ = this.isDoneLoadingSubject$.asObservable();

  constructor() {
    // Configure OAuth service
    this.oauthService.configure(authConfig);
    this.oauthService.setupAutomaticSilentRefresh();

    // Subscribe to token events
    this.oauthService.events.subscribe((event) => {
      if (event instanceof OAuthErrorEvent) {
        console.error('OAuthErrorEvent:', event);
      } else {
        console.log('OAuthEvent:', event);
      }
    });

    // Check if user is authenticated on token received
    this.oauthService.events
      .pipe(filter((e) => ['token_received'].includes(e.type)))
      .subscribe(() => this.isAuthenticatedSubject$.next(this.oauthService.hasValidAccessToken()));

    // Check if user is authenticated on token refresh
    this.oauthService.events
      .pipe(filter((e) => ['token_refreshed', 'token_expires'].includes(e.type)))
      .subscribe(() => this.isAuthenticatedSubject$.next(this.oauthService.hasValidAccessToken()));

    // Proactively refresh token when it's about to expire
    this.startTokenExpirationCheck();
  }

  private startTokenExpirationCheck(): void {
    // Check token expiration every minute
    setInterval(() => {
      if (this.oauthService.hasValidAccessToken()) {
        const tokenExpiresIn = this.oauthService.getAccessTokenExpiration() - Date.now();
        // Refresh if token expires in less than 5 minutes
        if (tokenExpiresIn < 5 * 60 * 1000) {
          console.log('Token expiring soon, refreshing...');
          this.refresh();
        }
      }
    }, 60 * 1000); // Check every minute
  }

  async runInitialLoginSequence(): Promise<void> {
    // Load discovery document
    await this.oauthService.loadDiscoveryDocument();

    // Try to login with code flow
    return this.oauthService
      .tryLoginCodeFlow()
      .then(() => {
        if (this.oauthService.hasValidAccessToken()) {
          this.isAuthenticatedSubject$.next(true);
        }
      })
      .finally(() => {
        this.isDoneLoadingSubject$.next(true);
      });
  }

  login(targetUrl?: string): void {
    this.oauthService.initCodeFlow(targetUrl || this.router.url);
  }

  logout(): void {
    // Use logOut instead of revokeTokenAndLogout to avoid 404 errors
    // when revocation endpoint is not available
    this.oauthService.logOut();
    this.isAuthenticatedSubject$.next(false);
  }

  refresh(): void {
    this.oauthService.silentRefresh();
  }

  hasValidToken(): boolean {
    return this.oauthService.hasValidAccessToken();
  }

  get accessToken(): string {
    return this.oauthService.getAccessToken();
  }

  get refreshToken(): string | null {
    return this.oauthService.getRefreshToken();
  }

  get identityClaims(): Record<string, unknown> | null {
    return this.oauthService.getIdentityClaims();
  }

  get idToken(): string {
    return this.oauthService.getIdToken();
  }

  getUserId(): string | null {
    const claims = this.identityClaims;
    if (!claims) return null;
    return (claims['sub'] as string) || (claims['nameid'] as string) || null;
  }

  getUserName(): string | null {
    const claims = this.identityClaims;
    if (!claims) return null;
    return (claims['name'] as string) || (claims['preferred_username'] as string) || null;
  }

  getUserEmail(): string | null {
    const claims = this.identityClaims;
    if (!claims) return null;
    return (claims['email'] as string) || null;
  }

  getUserRoles(): string[] {
    const claims = this.identityClaims;
    if (!claims) return [];
    const roles = claims['role'];
    if (Array.isArray(roles)) {
      return roles;
    }
    if (typeof roles === 'string') {
      return [roles];
    }
    return [];
  }

  isAuthenticated(): boolean {
    return this.oauthService.hasValidAccessToken();
  }

  hasPermission(permission: string): boolean {
    const roles = this.getUserRoles();

    // Admin has all permissions
    if (roles.includes('admin') || roles.includes('Admin')) {
      return true;
    }

    // Check for specific permissions
    if (permission === 'admin') {
      return roles.includes('admin') || roles.includes('Admin');
    }

    if (permission === 'create:rounds') {
      return (
        roles.includes('admin') ||
        roles.includes('Admin') ||
        roles.includes('organizer') ||
        roles.includes('Organizer')
      );
    }

    if (permission === 'record:match-results') {
      return (
        roles.includes('admin') ||
        roles.includes('Admin') ||
        roles.includes('organizer') ||
        roles.includes('Organizer')
      );
    }

    return false;
  }
}
