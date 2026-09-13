import {
  Component,
  effect,
  inject,
  input,
  output,
  signal,
} from '@angular/core';
import { rxResource } from '@angular/core/rxjs-interop';
import { NonNullableFormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { TranslatePipe } from '@ngx-translate/core';

import {
  APP_LANGUAGES,
  isAppLanguage,
  LANGUAGE_NAMES,
} from '../../../../core/services/language-service';
import { UserAdminDto, UserPayload } from '../models/user-admin-dto';
import { UsersService } from '../services/users.service';

/**
 * Shared create/edit form for a user. It owns validation and the account-type
 * lookup only — persistence and navigation stay with the hosting page.
 */
@Component({
  selector: 'app-user-form',
  imports: [ReactiveFormsModule, TranslatePipe],
  templateUrl: './user-form.html',
})
export class UserForm {
  private readonly formBuilder = inject(NonNullableFormBuilder);
  private readonly usersService = inject(UsersService);

  /** Existing user when editing, `null` when creating. */
  readonly user = input<UserAdminDto | null>(null);

  readonly saving = input(false);
  readonly errorMessage = input<string | null>(null);

  /** Already translated by the host page; empty falls back to "Save changes". */
  readonly submitLabel = input('');

  readonly save = output<UserPayload>();
  readonly cancel = output<void>();

  /** Each in its own words. "Not set" is offered separately, as an empty value. */
  protected readonly languages = APP_LANGUAGES.map((code) => ({
    code,
    label: LANGUAGE_NAMES[code],
  }));

  private readonly rolesResource = rxResource({
    stream: () => this.usersService.getRoles(),
  });

  protected readonly roles = this.rolesResource.value;
  protected readonly rolesFailed = this.rolesResource.error;

  protected readonly submitted = signal(false);

  protected readonly form = this.formBuilder.group({
    userName: ['', [Validators.required, Validators.maxLength(100)]],
    tgUserId: [0, [Validators.required, Validators.min(1)]],
    typeUserId: [0, [Validators.required, Validators.min(1)]],
    // No `points` control: the balance is derived from the user's
    // transactions and the API rejects a client-supplied value.
    // Empty means "not chosen yet": the bot then asks the member itself.
    languageCode: [''],
    status: [true],
  });

  constructor() {
    effect(() => {
      const user = this.user();

      if (!user) {
        return;
      }

      this.form.reset({
        userName: user.userName,
        tgUserId: user.tgUserId,
        typeUserId: user.typeUserId,
        // Shown as stored — never defaulted — so saving an unrelated change
        // cannot quietly set a language the member did not pick.
        languageCode: isAppLanguage(user.languageCode) ? user.languageCode : '',
        status: user.status,
      });
    });

    // Default a new user to the plain "User" role once the list has loaded.
    effect(() => {
      const roles = this.roles();

      if (this.user() || !roles?.length || this.form.controls.typeUserId.value) {
        return;
      }

      const fallback = roles.find((role) => role.name === 'User') ?? roles[roles.length - 1];

      this.form.controls.typeUserId.setValue(fallback.id);
    });

    effect(() => {
      if (this.saving()) {
        this.form.disable({ emitEvent: false });
      } else {
        this.form.enable({ emitEvent: false });
      }
    });
  }

  protected submit(): void {
    this.submitted.set(true);

    if (this.form.invalid) {
      this.form.markAllAsTouched();

      return;
    }

    const value = this.form.getRawValue();

    this.save.emit({
      userName: value.userName.trim(),
      tgUserId: Number(value.tgUserId),
      typeUserId: Number(value.typeUserId),
      languageCode: value.languageCode,
      status: value.status,
    });
  }

  /** A control shows its error once the user has interacted or tried to submit. */
  protected showError(control: 'userName' | 'tgUserId' | 'typeUserId'): boolean {
    const field = this.form.controls[control];

    return field.invalid && (field.touched || this.submitted());
  }
}
