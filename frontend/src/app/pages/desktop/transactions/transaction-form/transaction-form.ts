import {
  Component,
  computed,
  effect,
  inject,
  input,
  output,
  signal,
} from '@angular/core';
import { rxResource, takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { NonNullableFormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { TranslatePipe } from '@ngx-translate/core';

import { AdventuresService } from '../../adventures/services/adventures.service';
import { UsersService } from '../../users/services/users.service';
import { Translatable } from '../../../../shared/utils/translatable';
import { TransactionAdminDto, TransactionPayload } from '../models/transaction-admin-dto';

/** Value of the adventure select when no adventure is linked. */
const NO_ADVENTURE = 0;

/**
 * Shared create/edit form for a transaction. The adventure is optional: leaving
 * it empty records a manual points adjustment. Persistence and navigation stay
 * with the hosting page.
 */
@Component({
  selector: 'app-transaction-form',
  imports: [ReactiveFormsModule, TranslatePipe],
  templateUrl: './transaction-form.html',
})
export class TransactionForm {
  private readonly formBuilder = inject(NonNullableFormBuilder);
  private readonly usersService = inject(UsersService);
  private readonly adventuresService = inject(AdventuresService);

  /** Existing transaction when editing, `null` when creating. */
  readonly transaction = input<TransactionAdminDto | null>(null);

  readonly saving = input(false);
  readonly errorMessage = input<string | null>(null);

  /** Already translated by the host page; empty falls back to "Save changes". */
  readonly submitLabel = input('');

  readonly save = output<TransactionPayload>();
  readonly cancel = output<void>();

  private readonly usersResource = rxResource({
    stream: () => this.usersService.getAll(),
  });

  private readonly adventuresResource = rxResource({
    stream: () => this.adventuresService.getAll(),
  });

  protected readonly users = this.usersResource.value;
  protected readonly adventures = this.adventuresResource.value;

  protected readonly referenceFailed = computed(
    () => !!this.usersResource.error() || !!this.adventuresResource.error(),
  );

  protected readonly submitted = signal(false);

  protected readonly form = this.formBuilder.group({
    userId: [0, [Validators.required, Validators.min(1)]],
    travelId: [NO_ADVENTURE],
    points: [0, [Validators.required]],
    status: [true],
  });

  private readonly pointsValue = signal(0);

  /** Explains the balance effect of the amount currently entered. */
  protected readonly balanceHint = computed<Translatable>(() => {
    const points = this.pointsValue();

    if (points > 0) {
      return { key: 'admin.transactions.form.adds', params: { points } };
    }

    if (points < 0) {
      return { key: 'admin.transactions.form.subtracts', params: { points: Math.abs(points) } };
    }

    return { key: 'admin.transactions.form.unchanged' };
  });

  constructor() {
    effect(() => {
      const transaction = this.transaction();

      if (!transaction) {
        return;
      }

      // Silently: a programmatic reset must not trigger the adventure
      // prefill below, which would overwrite the saved points.
      this.form.reset(
        {
          userId: transaction.userId,
          travelId: transaction.travelId ?? NO_ADVENTURE,
          points: transaction.points,
          status: transaction.status,
        },
        { emitEvent: false },
      );

      this.pointsValue.set(transaction.points);
    });

    effect(() => {
      if (this.saving()) {
        this.form.disable({ emitEvent: false });
      } else {
        this.form.enable({ emitEvent: false });
      }
    });

    this.form.controls.points.valueChanges
      .pipe(takeUntilDestroyed())
      .subscribe((value) => this.pointsValue.set(Number(value) || 0));

    // Picking an adventure pre-fills its points; the amount stays editable.
    this.form.controls.travelId.valueChanges.pipe(takeUntilDestroyed()).subscribe((value) => {
      const travelId = Number(value);

      if (travelId === NO_ADVENTURE) {
        return;
      }

      const adventure = this.adventures()?.find((item) => item.id === travelId);

      if (adventure) {
        this.form.controls.points.setValue(adventure.points);
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
    const travelId = Number(value.travelId);

    this.save.emit({
      userId: Number(value.userId),
      travelId: travelId === NO_ADVENTURE ? null : travelId,
      points: Number(value.points),
      status: value.status,
    });
  }

  /** A control shows its error once the user has interacted or tried to submit. */
  protected showError(control: 'userId' | 'points'): boolean {
    const field = this.form.controls[control];

    return field.invalid && (field.touched || this.submitted());
  }
}
