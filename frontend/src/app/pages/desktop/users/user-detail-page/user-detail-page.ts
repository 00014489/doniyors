import { Component, computed, inject, input, signal } from '@angular/core';
import { rxResource } from '@angular/core/rxjs-interop';
import { Router, RouterLink } from '@angular/router';
import { TranslatePipe } from '@ngx-translate/core';

import { isAppLanguage, LANGUAGE_NAMES } from '../../../../core/services/language-service';
import { ConfirmDialog } from '../../../../shared/components/confirm-dialog/confirm-dialog';
import { LocalDatePipe } from '../../../../shared/pipes/local-date';
import { BackNavigation } from '../../../../shared/services/back-navigation';
import { UsersService } from '../services/users.service';

@Component({
  selector: 'app-user-detail-page',
  imports: [LocalDatePipe, RouterLink, ConfirmDialog, TranslatePipe],
  templateUrl: './user-detail-page.html',
})
export class UserDetailPage {
  private readonly usersService = inject(UsersService);
  private readonly backNavigation = inject(BackNavigation);
  private readonly router = inject(Router);

  /** Bound from the `:id` route parameter. */
  readonly id = input.required<string>();

  protected readonly userId = computed(() => Number(this.id()));

  protected readonly userResource = rxResource({
    params: () => this.userId(),
    stream: ({ params }) => this.usersService.getById(params),
  });

  protected readonly user = this.userResource.value;
  protected readonly isLoading = this.userResource.isLoading;

  /** A translation key, resolved in the template. */
  protected readonly errorMessage = computed(() =>
    this.userResource.error() ? 'admin.users.notFound' : null,
  );

  protected readonly confirmOpen = signal(false);
  protected readonly isDeleting = signal(false);

  /** A translation key, resolved in the template. */
  protected readonly deleteError = signal<string | null>(null);

  /** The language's own name, or `null` for a user who has not chosen one. */
  protected languageName(code: string): string | null {
    return isAppLanguage(code) ? LANGUAGE_NAMES[code] : null;
  }

  protected goBack(): void {
    this.backNavigation.back(['/users']);
  }

  protected askDelete(): void {
    this.deleteError.set(null);
    this.confirmOpen.set(true);
  }

  protected cancelDelete(): void {
    if (this.isDeleting()) {
      return;
    }

    this.confirmOpen.set(false);
  }

  /** Soft delete — the user stays in the database with `status: false`. */
  protected confirmDelete(): void {
    const user = this.user();

    if (!user || this.isDeleting()) {
      return;
    }

    this.isDeleting.set(true);
    this.deleteError.set(null);

    this.usersService.disable(user.id).subscribe({
      next: (disabled) => {
        this.userResource.value.set(disabled);

        this.isDeleting.set(false);
        this.confirmOpen.set(false);
      },
      error: () => {
        this.isDeleting.set(false);
        this.confirmOpen.set(false);
        this.deleteError.set('admin.users.disableFailed');
      },
    });
  }

  protected goToList(): void {
    void this.router.navigate(['/users']);
  }
}
