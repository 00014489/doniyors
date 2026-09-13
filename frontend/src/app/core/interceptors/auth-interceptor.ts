import { HttpErrorResponse, HttpInterceptorFn, HttpRequest } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, from, switchMap, throwError } from 'rxjs';

import { AuthService } from '../services/auth-service';

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const auth = inject(AuthService);

  if (auth.isSignInRequest(req.url)) {
    return next(req);
  }

  return next(withToken(req, auth.token)).pipe(
    catchError((error: unknown) => {
      // The token expires after an hour, but a Mini App can stay open longer.
      // Sign in again silently and retry once; a second 401 is a real refusal
      // and reaches the caller.
      if (
        !(error instanceof HttpErrorResponse)
        || error.status !== 401
        || !auth.authenticated()
      ) {
        return throwError(() => error);
      }

      return from(auth.refresh()).pipe(
        switchMap(token => next(withToken(req, token))),
      );
    }),
  );
};

function withToken<T>(req: HttpRequest<T>, token: string | null): HttpRequest<T> {
  return token
    ? req.clone({ setHeaders: { Authorization: `Bearer ${token}` } })
    : req;
}
