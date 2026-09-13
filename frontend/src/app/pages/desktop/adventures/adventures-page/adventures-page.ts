import { Component, computed, inject, signal } from '@angular/core';
import { rxResource } from '@angular/core/rxjs-interop';
import { RouterLink } from '@angular/router';
import { TranslatePipe } from '@ngx-translate/core';

import { AdventuresService } from '../services/adventures.service';
import { TravelAdminDto } from '../models/travel-admin-dto';
import { ConfirmDialog } from '../../../../shared/components/confirm-dialog/confirm-dialog';
import { LocalDatePipe } from '../../../../shared/pipes/local-date';
import { MoneyPipe } from '../../../../shared/pipes/money';
import { Translatable } from '../../../../shared/utils/translatable';

@Component({
  selector: 'app-adventures-page',
  imports: [LocalDatePipe, MoneyPipe, RouterLink, ConfirmDialog, TranslatePipe],
  templateUrl: './adventures-page.html',
  styleUrl: './adventures-page.scss',
})
export class AdventuresPage {
  private readonly adventuresService = inject(AdventuresService);

  protected readonly adventuresResource = rxResource({
    stream: () => this.adventuresService.getAll(),
  });

  protected readonly adventures = computed(() => this.adventuresResource.value() ?? []);
  protected readonly isLoading = this.adventuresResource.isLoading;

  /** A translation key, resolved in the template. */
  protected readonly errorMessage = computed(() =>
    this.adventuresResource.error() ? 'admin.adventures.loadFailed' : null,
  );

  /** Adventure awaiting delete confirmation; `null` closes the dialog. */
  protected readonly pendingDelete = signal<TravelAdminDto | null>(null);
  protected readonly isDeleting = signal(false);
  protected readonly deleteError = signal<Translatable | null>(null);

  protected readonly confirmOpen = computed(() => this.pendingDelete() !== null);

  protected askDelete(adventure: TravelAdminDto): void {
    this.deleteError.set(null);
    this.pendingDelete.set(adventure);
  }

  protected cancelDelete(): void {
    if (this.isDeleting()) {
      return;
    }

    this.pendingDelete.set(null);
  }

  /** Soft delete — the API keeps the row and sets its status to `false`. */
  protected confirmDelete(): void {
    const adventure = this.pendingDelete();

    if (!adventure || this.isDeleting()) {
      return;
    }

    this.isDeleting.set(true);
    this.deleteError.set(null);

    this.adventuresService.disable(adventure.id).subscribe({
      next: (disabled) => {
        this.adventuresResource.value.update((current) =>
          current?.map((item) => (item.id === disabled.id ? disabled : item)),
        );

        this.isDeleting.set(false);
        this.pendingDelete.set(null);
      },
      error: () => {
        this.isDeleting.set(false);
        this.deleteError.set({
          key: 'admin.adventures.disableFailedNamed',
          params: { title: adventure.title },
        });
        this.pendingDelete.set(null);
      },
    });
  }

  protected imageUrl(imageId: number): string {
    return this.adventuresService.imageUrl(imageId);
  }

  protected reload(): void {
    this.adventuresResource.reload();
  }
}
