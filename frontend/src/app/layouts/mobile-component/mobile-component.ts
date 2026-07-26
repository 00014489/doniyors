import { Component, inject, signal } from '@angular/core';
import { TelegramService } from '../../core/services/telegram-service';
import { TranslatePipe } from '@ngx-translate/core';
import { MobileNav, NavItem } from "../../shared/components/mobile-nav/mobile-nav";
import { Home } from "../../pages/home/home";
import { QrCode } from "../../pages/qr-code/qr-code";

@Component({
  selector: 'app-mobile-component',
  imports: [TranslatePipe, MobileNav, Home, QrCode],
  templateUrl: './mobile-component.html',
  styleUrl: './mobile-component.scss',
})
export class MobileComponent {
  protected readonly tg = inject(TelegramService);
  protected readonly activeTab = signal<NavItem>('home');

  setTab(tab: NavItem): void {
    this.activeTab.set(tab);
  }
}
