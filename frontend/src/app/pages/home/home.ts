import { Component, computed, inject, output, signal } from '@angular/core';
import { rxResource } from '@angular/core/rxjs-interop';
import { TranslatePipe } from '@ngx-translate/core';

import { MemberApi } from '../../core/services/member-api';
import { AdventuresList } from './adventures-list/adventures-list';
import { Leaderboard } from './leaderboard/leaderboard';

type HomeTab = 'users' | 'adventures';

@Component({
  selector: 'app-home',
  imports: [Leaderboard, AdventuresList, TranslatePipe],
  templateUrl: './home.html',
  styleUrl: './home.scss',
})
export class Home {
  private readonly memberApi = inject(MemberApi);

  readonly selectedTab = signal<HomeTab>('users');

  /** Opens an adventure's detail page; the mobile shell owns that view. */
  readonly openAdventure = output<number>();

  /** Null until the member picks one — the API then answers for the current season. */
  private readonly seasonKey = signal<string | null>(null);

  private readonly leaderboardResource = rxResource({
    params: () => ({ season: this.seasonKey() }),
    stream: ({ params }) => this.memberApi.getLeaderboard(params.season),
  });

  private readonly adventuresResource = rxResource({
    stream: () => this.memberApi.getAdventures(),
  });

  protected readonly adventures = computed(
    () => this.adventuresResource.value() ?? [],
  );

  protected readonly seasons = computed(
    () => this.leaderboardResource.value()?.seasons ?? [],
  );

  protected readonly entries = computed(
    () => this.leaderboardResource.value()?.entries ?? [],
  );

  /**
   * The API decides which season is selected on first load, so trust its
   * answer until the member picks one themselves.
   */
  protected readonly selectedSeasonKey = computed(
    () => this.leaderboardResource.value()?.selectedSeasonKey ?? '',
  );

  protected readonly adventuresLoading = this.adventuresResource.isLoading;
  protected readonly leaderboardLoading = this.leaderboardResource.isLoading;

  protected readonly adventuresFailed = computed(
    () => !!this.adventuresResource.error(),
  );

  protected readonly leaderboardFailed = computed(
    () => !!this.leaderboardResource.error(),
  );

  selectTab(tab: HomeTab): void {
    this.selectedTab.set(tab);
  }

  protected selectSeason(key: string): void {
    this.seasonKey.set(key);
  }

  protected reloadAdventures(): void {
    this.adventuresResource.reload();
  }

  protected reloadLeaderboard(): void {
    this.leaderboardResource.reload();
  }
}
