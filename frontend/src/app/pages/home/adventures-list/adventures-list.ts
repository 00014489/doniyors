import { Component, input, output } from '@angular/core';

import { AdventureListItem } from '../../../core/models/member';
import { AdventureRow } from './adventure-row/adventure-row';

/**
 * A plain list of adventure cards, rendered in the order given.
 *
 * Used twice: the main page passes upcoming adventures, the profile passes the
 * member's history. Neither needs grouping, so the order is entirely the
 * caller's to decide.
 */
@Component({
  selector: 'app-adventures-list',
  imports: [AdventureRow],
  template: `
    <div class="list">

      <!-- Tracked by index: the list is replaced wholesale on reload, and a
           member can hold two credits for the same adventure in their history. -->
      @for (adventure of adventures(); track $index) {
        <app-adventure-row
          [adventure]="adventure"
          (open)="open.emit($event)" />
      }

    </div>
  `,
  styleUrl: './adventures-list.scss',
})
export class AdventuresList {
  readonly adventures = input.required<readonly AdventureListItem[]>();

  readonly open = output<number>();
}
