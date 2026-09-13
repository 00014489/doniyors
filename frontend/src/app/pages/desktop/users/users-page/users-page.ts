import { Component, computed, inject, signal } from '@angular/core';
import { rxResource } from '@angular/core/rxjs-interop';
import { RouterLink } from '@angular/router';
import { TranslatePipe } from '@ngx-translate/core';

import { ConfirmDialog } from '../../../../shared/components/confirm-dialog/confirm-dialog';
import { LocalDatePipe } from '../../../../shared/pipes/local-date';
import { Translatable } from '../../../../shared/utils/translatable';
import { UserAdminDto } from '../models/user-admin-dto';
import { UsersService } from '../services/users.service';

@Component({
  selector: 'app-users-page',
  imports: [LocalDatePipe, RouterLink, ConfirmDialog, TranslatePipe],
  templateUrl: './users-page.html',
  styleUrl: './users-page.scss',
})
export class UsersPage {
  private readonly usersService = inject(UsersService);

  protected readonly usersResource = rxResource({
    stream: () => this.usersService.getAll(),
  });

  protected readonly users = computed(() => this.usersResource.value() ?? []);
  protected readonly isLoading = this.usersResource.isLoading;

  /** A translation key, resolved in the template. */
  protected readonly errorMessage = computed(() =>
    this.usersResource.error() ? 'admin.users.loadFailed' : null,
  );

  /** User awaiting delete confirmation; `null` closes the dialog. */
  protected readonly pendingDelete = signal<UserAdminDto | null>(null);
  protected readonly isDeleting = signal(false);
  protected readonly deleteError = signal<Translatable | null>(null);

  protected readonly confirmOpen = computed(() => this.pendingDelete() !== null);

  protected askDelete(user: UserAdminDto): void {
    this.deleteError.set(null);
    this.pendingDelete.set(user);
  }

  protected cancelDelete(): void {
    if (this.isDeleting()) {
      return;
    }

    this.pendingDelete.set(null);
  }

  /** Soft delete — the API keeps the row and sets its status to `false`. */
  protected confirmDelete(): void {
    const user = this.pendingDelete();

    if (!user || this.isDeleting()) {
      return;
    }

    this.isDeleting.set(true);
    this.deleteError.set(null);

    this.usersService.disable(user.id).subscribe({
      next: (disabled) => {
        this.usersResource.value.update((current) =>
          current?.map((item) => (item.id === disabled.id ? disabled : item)),
        );

        this.isDeleting.set(false);
        this.pendingDelete.set(null);
      },
      error: () => {
        this.isDeleting.set(false);
        this.deleteError.set({
          key: 'admin.users.disableFailedNamed',
          params: { name: user.userName },
        });
        this.pendingDelete.set(null);
      },
    });
  }

  protected reload(): void {
    this.usersResource.reload();
  }
}
