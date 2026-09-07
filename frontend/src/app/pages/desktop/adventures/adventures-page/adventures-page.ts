import { Component, computed, inject, signal } from '@angular/core';
import { AdventuresService } from '../services/adventures.service';
import { TravelAdminDto } from '../models/travel-admin-dto';
import { DatePipe } from '@angular/common';
import { rxResource } from '@angular/core/rxjs-interop';

@Component({
  selector: 'app-adventures-page',
  imports: [DatePipe],
  templateUrl: './adventures-page.html',
  styleUrl: './adventures-page.scss',
})
export class AdventuresPage {
  private readonly adventuresService = inject(AdventuresService);

  protected readonly adventuresResource = rxResource({
    stream: () => this.adventuresService.getAll(),
  });

  protected readonly adventures = computed(
    () => this.adventuresResource.value() ?? [],
  );
  protected readonly isLoading = this.adventuresResource.isLoading;
  protected readonly errorMessage = computed(() =>
    this.adventuresResource.error()
      ? 'Failed to load adventures. Please try again.'
      : null,
  );

  protected reload(): void {
    this.adventuresResource.reload();
  }
}
