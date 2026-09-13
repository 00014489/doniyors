import { Component, computed, inject, input, signal } from '@angular/core';
import { rxResource } from '@angular/core/rxjs-interop';
import { Router, RouterLink } from '@angular/router';
import { TranslatePipe, TranslateService } from '@ngx-translate/core';

import { BackNavigation } from '../../../../shared/services/back-navigation';
import { apiErrorMessage } from '../../../../shared/utils/api-error';
import { TransactionForm } from '../transaction-form/transaction-form';
import { TransactionPayload } from '../models/transaction-admin-dto';
import { TransactionsService } from '../services/transactions.service';

@Component({
  selector: 'app-transaction-edit-page',
  imports: [TransactionForm, RouterLink, TranslatePipe],
  templateUrl: './transaction-edit-page.html',
})
export class TransactionEditPage {
  private readonly transactionsService = inject(TransactionsService);
  private readonly backNavigation = inject(BackNavigation);
  private readonly router = inject(Router);
  private readonly translate = inject(TranslateService);

  /** Bound from the `:id` route parameter. */
  readonly id = input.required<string>();

  protected readonly transactionId = computed(() => Number(this.id()));

  protected readonly transactionResource = rxResource({
    params: () => this.transactionId(),
    stream: ({ params }) => this.transactionsService.getById(params),
  });

  protected readonly transaction = this.transactionResource.value;
  protected readonly isLoading = this.transactionResource.isLoading;

  /** A translation key, resolved in the template. */
  protected readonly loadError = computed(() =>
    this.transactionResource.error() ? 'admin.transactions.notFound' : null,
  );

  protected readonly isSaving = signal(false);
  protected readonly saveError = signal<string | null>(null);

  protected save(payload: TransactionPayload): void {
    if (this.isSaving()) {
      return;
    }

    this.isSaving.set(true);
    this.saveError.set(null);

    this.transactionsService.update(this.transactionId(), payload).subscribe({
      next: (updated) => {
        this.isSaving.set(false);

        void this.router.navigate(['/transactions', updated.id]);
      },
      error: (error: unknown) => {
        this.isSaving.set(false);
        this.saveError.set(apiErrorMessage(this.translate, error, 'admin.common.saveFailed'));
      },
    });
  }

  protected cancel(): void {
    this.backNavigation.back(['/transactions', this.transactionId()]);
  }

  protected goToList(): void {
    void this.router.navigate(['/transactions']);
  }
}
