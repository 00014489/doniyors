import { Component, inject, signal } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { TranslatePipe, TranslateService } from '@ngx-translate/core';

import { AdventureForm } from '../adventure-form/adventure-form';
import { AdventureSubmit } from '../models/travel-admin-dto';
import { AdventuresService } from '../services/adventures.service';
import { BackNavigation } from '../../../../shared/services/back-navigation';
import { apiErrorMessage } from '../../../../shared/utils/api-error';

@Component({
  selector: 'app-adventure-create-page',
  imports: [AdventureForm, RouterLink, TranslatePipe],
  templateUrl: './adventure-create-page.html',
  styleUrl: './adventure-create-page.scss',
})
export class AdventureCreatePage {
  private readonly adventuresService = inject(AdventuresService);
  private readonly backNavigation = inject(BackNavigation);
  private readonly router = inject(Router);
  private readonly translate = inject(TranslateService);

  protected readonly isSaving = signal(false);
  protected readonly saveError = signal<string | null>(null);

  protected save(submit: AdventureSubmit): void {
    if (this.isSaving()) {
      return;
    }

    this.isSaving.set(true);
    this.saveError.set(null);

    this.adventuresService.create(submit).subscribe({
      next: () => {
        this.isSaving.set(false);

        // Back to the list, which reloads and therefore shows the new adventure.
        void this.router.navigate(['/adventures']);
      },
      error: (error: unknown) => {
        this.isSaving.set(false);
        this.saveError.set(apiErrorMessage(this.translate, error, 'admin.adventures.createFailed'));
      },
    });
  }

  protected cancel(): void {
    this.backNavigation.back(['/adventures']);
  }
}
