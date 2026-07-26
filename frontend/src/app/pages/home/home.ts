import { Component, computed, inject, signal } from '@angular/core';
import { Leaderboard } from "./leaderboard/leaderboard";
import { AdventuresList } from "./adventures-list/adventures-list";
import { HomeService } from './home-service';

type HomeTab = 'users' | 'adventures';

@Component({
  selector: 'app-home',
  imports: [Leaderboard, AdventuresList],
  templateUrl: './home.html',
  styleUrl: './home.scss',
})
export class Home {
  private readonly homeService = inject(HomeService);

  readonly selectedTab = signal<HomeTab>('users');

  readonly users = computed(() => this.homeService.users());

  readonly adventures = computed(() => this.homeService.adventures());

  selectTab(tab: HomeTab): void {
    this.selectedTab.set(tab);
  }
}
