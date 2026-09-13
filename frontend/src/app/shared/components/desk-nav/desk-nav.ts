import { Component, signal } from '@angular/core';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { TranslatePipe } from '@ngx-translate/core';

interface NavItem {
  /** Translation key. */
  readonly label: string;
  readonly icon: string;
  readonly route: string;
}

@Component({
  selector: 'app-desk-nav',
  imports: [RouterLink, RouterLinkActive, TranslatePipe],
  templateUrl: './desk-nav.html',
  styleUrl: './desk-nav.scss',
})
export class DeskNav {
  readonly collapsed = signal(false);

  readonly items = signal<readonly NavItem[]>([
    {
      label: 'admin.nav.adventures',
      icon: 'hiking',
      route: '/adventures',
    },
    {
      label: 'admin.nav.users',
      icon: 'group',
      route: '/users',
    },
    {
      label: 'admin.nav.transactions',
      icon: 'receipt_long',
      route: '/transactions',
    },
  ]);

  toggle(): void {
    this.collapsed.update((value) => !value);
  }
}
