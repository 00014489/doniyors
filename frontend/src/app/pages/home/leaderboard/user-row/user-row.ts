import { Component, computed, input } from '@angular/core';

import { Avatar } from '../../../../shared/components/avatar/avatar';
import { LeaderboardEntry } from '../../../../core/models/member';

@Component({
  selector: 'app-user-row',
  imports: [Avatar],
  template: `
    <div class="row" [class.row--me]="entry().isCurrentUser">

      <span class="rank" [class.rank--medal]="medal()">
        @if (medal(); as medal) {
          <span aria-hidden="true">{{ medal }}</span>
          <span class="visually-hidden">{{ entry().rank }}</span>
        } @else {
          {{ entry().rank }}
        }
      </span>

      <app-avatar
        [name]="entry().displayName"
        [photoUrl]="entry().photoUrl"
        [size]="44" />

      <span class="name">{{ entry().displayName }}</span>

      <span class="points">{{ entry().points }}</span>

    </div>
  `,
  styleUrl: './user-row.scss',
})
export class UserRow {
  readonly entry = input.required<LeaderboardEntry>();

  /** The top three get a medal instead of a plain number. */
  protected readonly medal = computed(() => {
    switch (this.entry().rank) {
      case 1:
        return '🥇';
      case 2:
        return '🥈';
      case 3:
        return '🥉';
      default:
        return null;
    }
  });
}
