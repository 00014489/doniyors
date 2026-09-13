import { Component, computed, inject, input, signal } from '@angular/core';
import { rxResource } from '@angular/core/rxjs-interop';
import { Router, RouterLink } from '@angular/router';
import { TranslatePipe } from '@ngx-translate/core';

import { AdventuresService } from '../services/adventures.service';
import { MoneyPipe } from '../../../../shared/pipes/money';
import { LocalDatePipe } from '../../../../shared/pipes/local-date';
import { ConfirmDialog } from '../../../../shared/components/confirm-dialog/confirm-dialog';
import { BackNavigation } from '../../../../shared/services/back-navigation';

@Component({
  selector: 'app-adventure-detail-page',
  imports: [LocalDatePipe, MoneyPipe, RouterLink, ConfirmDialog, TranslatePipe],
  templateUrl: './adventure-detail-page.html',
  styleUrl: './adventure-detail-page.scss',
})
export class AdventureDetailPage {
  private readonly adventuresService = inject(AdventuresService);
  private readonly backNavigation = inject(BackNavigation);
  private readonly router = inject(Router);

  /** Bound from the `:id` route parameter. */
  readonly id = input.required<string>();

  protected readonly adventureId = computed(() => Number(this.id()));

  protected readonly adventureResource = rxResource({
    params: () => this.adventureId(),
    stream: ({ params }) => this.adventuresService.getById(params),
  });

  protected readonly adventure = this.adventureResource.value;
  protected readonly isLoading = this.adventureResource.isLoading;

  /** A translation key, resolved in the template. */
  protected readonly errorMessage = computed(() =>
    this.adventureResource.error() ? 'admin.adventures.notFound' : null,
  );

  protected readonly confirmOpen = signal(false);
  protected readonly isDeleting = signal(false);

  /** A translation key, resolved in the template. */
  protected readonly deleteError = signal<string | null>(null);

  protected goBack(): void {
    this.backNavigation.back(['/adventures']);
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

  /** Soft delete — the adventure stays in the database with `status: false`. */
  protected confirmDelete(): void {
    const adventure = this.adventure();

    if (!adventure || this.isDeleting()) {
      return;
    }

    this.isDeleting.set(true);
    this.deleteError.set(null);

    this.adventuresService.disable(adventure.id).subscribe({
      next: (disabled) => {
        this.adventureResource.value.set(disabled);

        this.isDeleting.set(false);
        this.confirmOpen.set(false);
      },
      error: () => {
        this.isDeleting.set(false);
        this.confirmOpen.set(false);
        this.deleteError.set('admin.adventures.disableFailed');
      },
    });
  }

  protected imageUrl(imageId: number): string {
    return this.adventuresService.imageUrl(imageId);
  }

  protected goToList(): void {
    void this.router.navigate(['/adventures']);
  }
}
