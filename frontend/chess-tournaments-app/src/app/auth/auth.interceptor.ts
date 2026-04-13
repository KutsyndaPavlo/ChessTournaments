import {
  HttpInterceptorFn,
  HttpErrorResponse,
  HttpEvent,
  HttpRequest,
  HttpHandlerFn,
} from '@angular/common/http';
import { inject } from '@angular/core';
import { OAuthService } from 'angular-oauth2-oidc';
import { environment } from '../../environments/environment';
import { catchError, switchMap, throwError, from, Observable } from 'rxjs';
import { AuthService } from './auth.service';

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const oauthService = inject(OAuthService);
  const authService = inject(AuthService);

  // Only add token to requests going to our API
  if (!req.url.startsWith(environment.apiUrl)) {
    return next(req);
  }

  const accessToken = oauthService.getAccessToken();

  if (!accessToken) {
    return next(req);
  }

  // Clone the request and add the Authorization header
  const clonedRequest = req.clone({
    setHeaders: {
      Authorization: `Bearer ${accessToken}`,
    },
  });

  return next(clonedRequest).pipe(
    catchError((error: HttpErrorResponse) => {
      // If we get a 401 Unauthorized, try to refresh the token
      if (error.status === 401 && oauthService.getRefreshToken()) {
        return handleTokenRefresh(oauthService, authService, req, next);
      }
      return throwError(() => error);
    }),
  );
};

function handleTokenRefresh(
  oauthService: OAuthService,
  authService: AuthService,
  req: HttpRequest<unknown>,
  next: HttpHandlerFn,
): Observable<HttpEvent<unknown>> {
  // Try to refresh the token
  return from(
    oauthService
      .refreshToken()
      .then(() => {
        const newToken = oauthService.getAccessToken();
        if (newToken) {
          return newToken;
        } else {
          throw new Error('No token after refresh');
        }
      })
      .catch((error) => {
        // If refresh fails, redirect to login
        authService.logout();
        authService.login();
        throw error;
      }),
  ).pipe(
    switchMap((newToken) => {
      // Retry the original request with the new token
      const clonedRequest = req.clone({
        setHeaders: {
          Authorization: `Bearer ${newToken}`,
        },
      });
      return next(clonedRequest);
    }),
    catchError((error) => throwError(() => error)),
  );
}
