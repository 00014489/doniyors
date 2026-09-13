import { Component, inject, signal } from '@angular/core';

import { TelegramService } from '../../core/services/telegram-service';
import { MobileNav, NavItem } from '../../shared/components/mobile-nav/mobile-nav';
import { Home } from '../../pages/home/home';
import { QrCode } from '../../pages/qr-code/qr-code';
import { Profile } from '../../pages/profile/profile';
import { AdventureDetail } from '../../pages/adventure-detail/adventure-detail';

/**
 * The member shell: three tabs, plus a detail view pushed over them.
 *
 * The detail is state rather than a route because the member side of the app
 * has no router — the one in `app.routes.ts` belongs to the admin panel. A
 * pushed view with Telegram's own back arrow is also what a Mini App user
 * expects, so this matches the platform rather than working around it.
 */
@Component({
  selector: 'app-mobile-component',
  imports: [MobileNav, Home, QrCode, Profile, AdventureDetail],
  templateUrl: './mobile-component.html',
  styleUrl: './mobile-component.scss',
})
export class MobileComponent {
  private readonly tg = inject(TelegramService);

  protected readonly activeTab = signal<NavItem>('home');

  /** Non-null while an adventure's detail page is open. */
  protected readonly openedAdventureId = signal<number | null>(null);

  protected openAdventure(id: number): void {
    this.tg.tapFeedback();

    this.openedAdventureId.set(id);
  }

  protected closeAdventure(): void {
    this.openedAdventureId.set(null);
  }
}
