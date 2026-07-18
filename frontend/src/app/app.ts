import { Component, inject, OnInit, signal } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { TelegramService } from './core/services/telegram-service';
import { LayoutService } from './core/services/layout-service';
import { DesktopComponent } from "./layouts/desktop-component/desktop-component";
import { MobileComponent } from './layouts/mobile-component/mobile-component';

@Component({
  selector: 'app-root',
  imports: [DesktopComponent, MobileComponent],
  templateUrl: './app.html',
  styleUrl: './app.scss'
})
export class App implements OnInit {
  telegram = inject(TelegramService);
  layout = inject(LayoutService);

  ngOnInit(): void {
    this.telegram.initTelegram();
  }
}
