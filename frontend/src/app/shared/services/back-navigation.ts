import { Location } from '@angular/common';
import { inject, Service } from '@angular/core';
import { Router } from '@angular/router';

/**
 * Returns the administrator to where they came from, falling back to a known
 * route when the page was opened directly through its URL (no in-app history).
 */
@Service()
export class BackNavigation {
  private readonly router = inject(Router);
  private readonly location = inject(Location);

  back(fallback: readonly (string | number)[]): void {
    // `lastSuccessfulNavigation` is a signal in Angular v21.
    const hasInAppHistory =
      this.router.lastSuccessfulNavigation()?.previousNavigation != null;

    if (hasInAppHistory) {
      this.location.back();

      return;
    }

    void this.router.navigate([...fallback]);
  }
}
