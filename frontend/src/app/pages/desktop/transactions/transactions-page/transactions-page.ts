import { Component, computed, inject, signal } from '@angular/core';
import { rxResource } from '@angular/core/rxjs-interop';
import { RouterLink } from '@angular/router';
import { TranslatePipe } from '@ngx-translate/core';

import { ConfirmDialog } from '../../../../shared/components/confirm-dialog/confirm-dialog';
import { LocalDatePipe } from '../../../../shared/pipes/local-date';
import { Translatable } from '../../../../shared/utils/translatable';
import { TransactionAdminDto } from '../models/transaction-admin-dto';
import { TransactionsService } from '../services/transactions.service';

@Component({
  selector: 'app-transactions-page',
  imports: [LocalDatePipe, RouterLink, ConfirmDialog, TranslatePipe],
  templateUrl: './transactions-page.html',
  styleUrl: './transactions-page.scss',
})
export class TransactionsPage {
  private readonly transactionsService = inject(TransactionsService);

  protected readonly transactionsResource = rxResource({
    stream: () => this.transactionsService.getAll(),
  });

  protected readonly transactions = computed(() => this.transactionsResource.value() ?? []);
  protected readonly isLoading = this.transactionsResource.isLoading;

  /** A translation key, resolved in the template. */
  protected readonly errorMessage = computed(() =>
    this.transactionsResource.error() ? 'admin.transactions.loadFailed' : null,
  );

  /** Transaction awaiting delete confirmation; `null` closes the dialog. */
  protected readonly pendingDelete = signal<TransactionAdminDto | null>(null);
  protected readonly isDeleting = signal(false);
  protected readonly deleteError = signal<Translatable | null>(null);

  protected readonly confirmOpen = computed(() => this.pendingDelete() !== null);

  protected askDelete(transaction: TransactionAdminDto): void {
    this.deleteError.set(null);
    this.pendingDelete.set(transaction);
  }

  protected cancelDelete(): void {
    if (this.isDeleting()) {
      return;
    }

    this.pendingDelete.set(null);
  }

  /** Soft delete — the API keeps the record and sets its status to `false`. */
  protected confirmDelete(): void {
    const transaction = this.pendingDelete();

    if (!transaction || this.isDeleting()) {
      return;
    }

    this.isDeleting.set(true);
    this.deleteError.set(null);

    this.transactionsService.disable(transaction.id).subscribe({
      next: (disabled) => {
        this.transactionsResource.value.update((current) =>
          current?.map((item) => (item.id === disabled.id ? disabled : item)),
        );

        this.isDeleting.set(false);
        this.pendingDelete.set(null);
      },
      error: () => {
        this.isDeleting.set(false);
        this.deleteError.set({
          key: 'admin.transactions.voidFailedNumbered',
          params: { id: transaction.id },
        });
        this.pendingDelete.set(null);
      },
    });
  }

  protected reload(): void {
    this.transactionsResource.reload();
  }
}
