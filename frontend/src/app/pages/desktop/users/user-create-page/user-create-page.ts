import { Component, inject, signal } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { TranslatePipe, TranslateService } from '@ngx-translate/core';

import { BackNavigation } from '../../../../shared/services/back-navigation';
import { apiErrorMessage } from '../../../../shared/utils/api-error';
import { UserForm } from '../user-form/user-form';
import { UserPayload } from '../models/user-admin-dto';
import { UsersService } from '../services/users.service';

@Component({
  selector: 'app-user-create-page',
  imports: [UserForm, RouterLink, TranslatePipe],
  templateUrl: './user-create-page.html',
})
export class UserCreatePage {
  private readonly usersService = inject(UsersService);
  private readonly backNavigation = inject(BackNavigation);
  private readonly router = inject(Router);
  private readonly translate = inject(TranslateService);

  protected readonly isSaving = signal(false);
  protected readonly saveError = signal<string | null>(null);

  protected save(payload: UserPayload): void {
    if (this.isSaving()) {
      return;
    }

    this.isSaving.set(true);
    this.saveError.set(null);

    this.usersService.create(payload).subscribe({
      next: () => {
        this.isSaving.set(false);

        // Back to the list, which reloads and therefore shows the new user.
        void this.router.navigate(['/users']);
      },
      error: (error: unknown) => {
        this.isSaving.set(false);
        this.saveError.set(apiErrorMessage(this.translate, error, 'admin.users.createFailed'));
      },
    });
  }

  protected cancel(): void {
    this.backNavigation.back(['/users']);
  }
}
