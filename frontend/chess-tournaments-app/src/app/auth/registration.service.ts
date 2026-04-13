import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { environment } from '../../environments/environment';

export interface RegisterRequest {
  username: string;
  email: string;
  password: string;
  confirmPassword: string;
  firstName?: string;
  lastName?: string;
}

export interface RegisterResponse {
  status: 'Succeeded' | 'UserAlreadyExists' | 'ValidationError' | 'Failed';
  message?: string;
  errors?: string[];
}

@Injectable({
  providedIn: 'root',
})
export class RegistrationService {
  private http = inject(HttpClient);
  private registrationUrl = `${environment.oidc.issuer}api/register`;

  register(request: RegisterRequest): Observable<RegisterResponse> {
    return this.http.post<RegisterResponse>(this.registrationUrl, request).pipe(
      catchError((error: HttpErrorResponse) => {
        console.error('Registration error:', error);

        // Handle error response
        if (error.error && typeof error.error === 'object') {
          // If the server returned a RegisterResponse with error details
          if (error.error.status) {
            return throwError(() => error.error as RegisterResponse);
          }
          // If the server returned a generic error object
          if (error.error.message) {
            return throwError(() => ({
              status: 'Failed' as const,
              message: error.error.message,
            }));
          }
        }

        // Generic error handling
        return throwError(() => ({
          status: 'Failed' as const,
          message: error.message || 'An unexpected error occurred during registration',
        }));
      }),
    );
  }
}
