import { Component, computed, inject, input, signal } from '@angular/core';
import { rxResource } from '@angular/core/rxjs-interop';
import { Router, RouterLink } from '@angular/router';
import { TranslatePipe, TranslateService } from '@ngx-translate/core';

import { AdventureForm } from '../adventure-form/adventure-form';
import { AdventureSubmit } from '../models/travel-admin-dto';
import { AdventuresService } from '../services/adventures.service';
import { BackNavigation } from '../../../../shared/services/back-navigation';
import { apiErrorMessage } from '../../../../shared/utils/api-error';

@Component({
  selector: 'app-adventure-edit-page',
  imports: [AdventureForm, RouterLink, TranslatePipe],
  templateUrl: './adventure-edit-page.html',
  styleUrl: './adventure-edit-page.scss',
})
export class AdventureEditPage {
  private readonly adventuresService = inject(AdventuresService);
  private readonly backNavigation = inject(BackNavigation);
  private readonly router = inject(Router);
  private readonly translate = inject(TranslateService);

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
  protected readonly loadError = computed(() =>
    this.adventureResource.error() ? 'admin.adventures.notFound' : null,
  );

  protected readonly isSaving = signal(false);
  protected readonly saveError = signal<string | null>(null);

  protected save(submit: AdventureSubmit): void {
    if (this.isSaving()) {
      return;
    }

    this.isSaving.set(true);
    this.saveError.set(null);

    this.adventuresService.update(this.adventureId(), submit).subscribe({
      next: (updated) => {
        this.isSaving.set(false);

        void this.router.navigate(['/adventures', updated.id]);
      },
      error: (error: unknown) => {
        this.isSaving.set(false);
        this.saveError.set(apiErrorMessage(this.translate, error, 'admin.common.saveFailed'));
      },
    });
  }

  protected cancel(): void {
    this.backNavigation.back(['/adventures', this.adventureId()]);
  }

  protected goToList(): void {
    void this.router.navigate(['/adventures']);
  }
}
