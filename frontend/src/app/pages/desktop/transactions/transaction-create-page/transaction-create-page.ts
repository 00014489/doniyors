import { Component, inject, signal } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { TranslatePipe, TranslateService } from '@ngx-translate/core';

import { BackNavigation } from '../../../../shared/services/back-navigation';
import { apiErrorMessage } from '../../../../shared/utils/api-error';
import { TransactionForm } from '../transaction-form/transaction-form';
import { TransactionPayload } from '../models/transaction-admin-dto';
import { TransactionsService } from '../services/transactions.service';

@Component({
  selector: 'app-transaction-create-page',
  imports: [TransactionForm, RouterLink, TranslatePipe],
  templateUrl: './transaction-create-page.html',
})
export class TransactionCreatePage {
  private readonly transactionsService = inject(TransactionsService);
  private readonly backNavigation = inject(BackNavigation);
  private readonly router = inject(Router);
  private readonly translate = inject(TranslateService);

  protected readonly isSaving = signal(false);
  protected readonly saveError = signal<string | null>(null);

  protected save(payload: TransactionPayload): void {
    if (this.isSaving()) {
      return;
    }

    this.isSaving.set(true);
    this.saveError.set(null);

    this.transactionsService.create(payload).subscribe({
      next: () => {
        this.isSaving.set(false);

        // Back to the list, which reloads and therefore shows the new record.
        void this.router.navigate(['/transactions']);
      },
      error: (error: unknown) => {
        this.isSaving.set(false);
        this.saveError.set(
          apiErrorMessage(this.translate, error, 'admin.transactions.createFailed'),
        );
      },
    });
  }

  protected cancel(): void {
    this.backNavigation.back(['/transactions']);
  }
}
