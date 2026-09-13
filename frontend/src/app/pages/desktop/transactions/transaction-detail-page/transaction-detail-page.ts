import { Component, computed, inject, input, signal } from '@angular/core';
import { rxResource } from '@angular/core/rxjs-interop';
import { Router, RouterLink } from '@angular/router';
import { TranslatePipe } from '@ngx-translate/core';

import { ConfirmDialog } from '../../../../shared/components/confirm-dialog/confirm-dialog';
import { LocalDatePipe } from '../../../../shared/pipes/local-date';
import { BackNavigation } from '../../../../shared/services/back-navigation';
import { TransactionsService } from '../services/transactions.service';

@Component({
  selector: 'app-transaction-detail-page',
  imports: [LocalDatePipe, RouterLink, ConfirmDialog, TranslatePipe],
  templateUrl: './transaction-detail-page.html',
  styleUrl: './transaction-detail-page.scss',
})
export class TransactionDetailPage {
  private readonly transactionsService = inject(TransactionsService);
  private readonly backNavigation = inject(BackNavigation);
  private readonly router = inject(Router);

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
  protected readonly errorMessage = computed(() =>
    this.transactionResource.error() ? 'admin.transactions.notFound' : null,
  );

  protected readonly confirmOpen = signal(false);
  protected readonly isDeleting = signal(false);

  /** A translation key, resolved in the template. */
  protected readonly deleteError = signal<string | null>(null);

  protected goBack(): void {
    this.backNavigation.back(['/transactions']);
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

  /** Soft delete — the record stays in the database with `status: false`. */
  protected confirmDelete(): void {
    const transaction = this.transaction();

    if (!transaction || this.isDeleting()) {
      return;
    }

    this.isDeleting.set(true);
    this.deleteError.set(null);

    this.transactionsService.disable(transaction.id).subscribe({
      next: (disabled) => {
        this.transactionResource.value.set(disabled);

        this.isDeleting.set(false);
        this.confirmOpen.set(false);
      },
      error: () => {
        this.isDeleting.set(false);
        this.confirmOpen.set(false);
        this.deleteError.set('admin.transactions.voidFailed');
      },
    });
  }

  protected goToList(): void {
    void this.router.navigate(['/transactions']);
  }
}
