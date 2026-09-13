import { inject } from '@angular/core';
import { CanMatchFn, Router } from '@angular/router';

import { AuthService } from '../services/auth-service';

/**
 * Keeps the admin panel's routes from matching for an ordinary account.
 *
 * The API is the real fence — every admin endpoint is behind
 * `[Authorize(Policy = "Administrative")]`. This stops a non-administrator who
 * reaches the URL from being shown a panel that can only answer 403.
 */
export const adminGuard: CanMatchFn = () => {
  const auth = inject(AuthService);
  const router = inject(Router);

  // Returning a UrlTree redirects, so the wildcard route cannot bounce the
  // navigation back into a blocked route.
  return auth.isAdmin() ? true : router.parseUrl('/forbidden');
};

/**
 * Sends an administrator who lands on `/forbidden` — say, a reload of a page
 * left there by an earlier session — back to the panel's start page.
 */
export const forbiddenGuard: CanMatchFn = () => {
  const auth = inject(AuthService);
  const router = inject(Router);

  return auth.isAdmin() ? router.parseUrl('/adventures') : true;
};
