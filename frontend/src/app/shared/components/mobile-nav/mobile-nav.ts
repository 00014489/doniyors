import { Component, model } from '@angular/core';
import { TranslatePipe } from '@ngx-translate/core';

export type NavItem = 'home' | 'qr' | 'profile';

@Component({
  selector: 'app-mobile-nav',
  imports: [TranslatePipe],
  template: `
    <nav class="mobile-nav" [attr.aria-label]="'nav.primary' | translate">

      <button
        type="button"
        class="nav-item"
        [class.active]="active() === 'home'"
        [attr.aria-current]="active() === 'home' ? 'page' : null"
        (click)="select('home')">
        <span class="material-symbols-rounded" aria-hidden="true">home</span>
        <span>{{ 'nav.main' | translate }}</span>
      </button>

      <button
        type="button"
        class="nav-item"
        [class.active]="active() === 'qr'"
        [attr.aria-current]="active() === 'qr' ? 'page' : null"
        (click)="select('qr')">
        <span class="material-symbols-rounded" aria-hidden="true">qr_code_2</span>
        <span>{{ 'nav.qrCode' | translate }}</span>
      </button>

      <button
        type="button"
        class="nav-item"
        [class.active]="active() === 'profile'"
        [attr.aria-current]="active() === 'profile' ? 'page' : null"
        (click)="select('profile')">
        <span class="material-symbols-rounded" aria-hidden="true">person</span>
        <span>{{ 'nav.profile' | translate }}</span>
      </button>

    </nav>
  `,
  styleUrl: './mobile-nav.scss',
})
export class MobileNav {
  /** Two-way bound: `[(active)]`. */
  readonly active = model.required<NavItem>();

  protected select(item: NavItem): void {
    this.active.set(item);
  }
}
