import { Component, computed, inject, input, output } from '@angular/core';
import { TranslatePipe, TranslateService } from '@ngx-translate/core';
import { toSignal } from '@angular/core/rxjs-interop';

import { LeaderboardEntry, Season } from '../../../core/models/member';
import { UserRow } from './user-row/user-row';

/** A season with its name already resolved into the member's language. */
interface SeasonOption {
  readonly key: string;
  readonly label: string;
}

@Component({
  selector: 'app-leaderboard',
  imports: [UserRow, TranslatePipe],
  templateUrl: './leaderboard.html',
  styleUrl: './leaderboard.scss',
})
export class Leaderboard {
  readonly entries = input.required<readonly LeaderboardEntry[]>();
  readonly seasons = input.required<readonly Season[]>();
  readonly selectedSeasonKey = input.required<string>();

  /** Dimmed while the newly chosen season is being fetched. */
  readonly loading = input(false);

  readonly seasonChange = output<string>();

  private readonly translate = inject(TranslateService);

  /**
   * Re-reads the labels when the member changes language. The API sends the
   * season's name and year separately precisely so the picker can be written
   * in their own language rather than always in English.
   */
  private readonly language = toSignal(this.translate.onLangChange, {
    initialValue: null,
  });

  protected readonly options = computed<readonly SeasonOption[]>(() => {
    // Read so the labels recompute after a language change.
    this.language();

    return this.seasons().map(season => ({
      key: season.key,
      label: this.labelFor(season),
    }));
  });

  protected onSeasonChange(event: Event): void {
    this.seasonChange.emit((event.target as HTMLSelectElement).value);
  }

  private labelFor(season: Season): string {
    const name = this.translate.instant(`season.${season.name}`);

    // A missing translation comes back as the key itself; fall back to the
    // English label the API already provides rather than showing "season.x".
    if (name === `season.${season.name}`) {
      return season.label;
    }

    // Winter spans two years — "Winter 2026/27".
    const years =
      season.name === 'winter'
        ? `${season.year}/${`${(season.year + 1) % 100}`.padStart(2, '0')}`
        : `${season.year}`;

    return `${name} ${years}`;
  }
}
