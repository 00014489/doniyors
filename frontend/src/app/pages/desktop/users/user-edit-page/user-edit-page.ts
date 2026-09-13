import { Component, computed, inject, input, signal } from '@angular/core';
import { rxResource } from '@angular/core/rxjs-interop';
import { Router, RouterLink } from '@angular/router';
import { TranslatePipe, TranslateService } from '@ngx-translate/core';

import { BackNavigation } from '../../../../shared/services/back-navigation';
import { apiErrorMessage } from '../../../../shared/utils/api-error';
import { UserForm } from '../user-form/user-form';
import { UserPayload } from '../models/user-admin-dto';
import { UsersService } from '../services/users.service';

@Component({
  selector: 'app-user-edit-page',
  imports: [UserForm, RouterLink, TranslatePipe],
  templateUrl: './user-edit-page.html',
})
export class UserEditPage {
  private readonly usersService = inject(UsersService);
  private readonly backNavigation = inject(BackNavigation);
  private readonly router = inject(Router);
  private readonly translate = inject(TranslateService);

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
  protected readonly loadError = computed(() =>
    this.userResource.error() ? 'admin.users.notFound' : null,
  );

  protected readonly isSaving = signal(false);
  protected readonly saveError = signal<string | null>(null);

  protected save(payload: UserPayload): void {
    if (this.isSaving()) {
      return;
    }

    this.isSaving.set(true);
    this.saveError.set(null);

    this.usersService.update(this.userId(), payload).subscribe({
      next: (updated) => {
        this.isSaving.set(false);

        void this.router.navigate(['/users', updated.id]);
      },
      error: (error: unknown) => {
        this.isSaving.set(false);
        this.saveError.set(apiErrorMessage(this.translate, error, 'admin.common.saveFailed'));
      },
    });
  }

  protected cancel(): void {
    this.backNavigation.back(['/users', this.userId()]);
  }

  protected goToList(): void {
    void this.router.navigate(['/users']);
  }
}
