import { HttpErrorResponse } from '@angular/common/http';
import { MonoTypeOperatorFunction, catchError, EMPTY, throwError } from 'rxjs';

/**
 * Catches HTTP 400 (Bad Request) errors and executes a callback.
 * Other errors are re-thrown.
 */
export function catchHttp400<T>(
  callback: (error: HttpErrorResponse) => void,
): MonoTypeOperatorFunction<T> {
  return catchError((error: HttpErrorResponse) => {
    if (error.status === 400) {
      callback(error);
      return EMPTY;
    }
    return throwError(() => error);
  });
}

/**
 * Catches HTTP 401 (Unauthorized) errors and executes a callback.
 * Other errors are re-thrown.
 */
export function catchHttp401<T>(
  callback: (error: HttpErrorResponse) => void,
): MonoTypeOperatorFunction<T> {
  return catchError((error: HttpErrorResponse) => {
    if (error.status === 401) {
      callback(error);
      return EMPTY;
    }
    return throwError(() => error);
  });
}

/**
 * Catches HTTP 403 (Forbidden) errors and executes a callback.
 * Other errors are re-thrown.
 */
export function catchHttp403<T>(
  callback: (error: HttpErrorResponse) => void,
): MonoTypeOperatorFunction<T> {
  return catchError((error: HttpErrorResponse) => {
    if (error.status === 403) {
      callback(error);
      return EMPTY;
    }
    return throwError(() => error);
  });
}

/**
 * Catches HTTP 404 (Not Found) errors and executes a callback.
 * Other errors are re-thrown.
 */
export function catchHttp404<T>(
  callback: (error: HttpErrorResponse) => void,
): MonoTypeOperatorFunction<T> {
  return catchError((error: HttpErrorResponse) => {
    if (error.status === 404) {
      callback(error);
      return EMPTY;
    }
    return throwError(() => error);
  });
}

/**
 * Catches HTTP 409 (Conflict) errors and executes a callback.
 * Other errors are re-thrown.
 */
export function catchHttp409<T>(
  callback: (error: HttpErrorResponse) => void,
): MonoTypeOperatorFunction<T> {
  return catchError((error: HttpErrorResponse) => {
    if (error.status === 409) {
      callback(error);
      return EMPTY;
    }
    return throwError(() => error);
  });
}

/**
 * Catches HTTP 500 (Internal Server Error) errors and executes a callback.
 * Other errors are re-thrown.
 */
export function catchHttp500<T>(
  callback: (error: HttpErrorResponse) => void,
): MonoTypeOperatorFunction<T> {
  return catchError((error: HttpErrorResponse) => {
    if (error.status === 500) {
      callback(error);
      return EMPTY;
    }
    return throwError(() => error);
  });
}

/**
 * Catches any HTTP error and executes a callback.
 * The callback receives the error and can handle it accordingly.
 */
export function catchHttpError<T>(
  callback: (error: HttpErrorResponse) => void,
): MonoTypeOperatorFunction<T> {
  return catchError((error: HttpErrorResponse) => {
    callback(error);
    return EMPTY;
  });
}
