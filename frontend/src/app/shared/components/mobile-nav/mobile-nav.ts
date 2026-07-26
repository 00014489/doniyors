import { Component, input, output } from '@angular/core';
import { TranslatePipe } from '@ngx-translate/core';

export type NavItem = 'home' | 'qr' | 'profile';

@Component({
  selector: 'app-mobile-nav',
  imports: [TranslatePipe],
  templateUrl: './mobile-nav.html',
  styleUrl: './mobile-nav.scss',
})
export class MobileNav {
  readonly active = input.required<NavItem>();
  readonly activeChange = output<NavItem>();

  select(item: NavItem): void {
    this.activeChange.emit(item);
  }
}
