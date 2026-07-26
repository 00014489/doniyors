import { Component, computed, DestroyRef, effect, inject, signal } from '@angular/core';
import { TelegramService } from './core/services/telegram-service';
import { MobileComponent } from "./layouts/mobile-component/mobile-component";
import { DesktopComponent } from "./layouts/desktop-component/desktop-component";
import { AuthService } from './core/services/auth-service';
import { NgOptimizedImage } from '@angular/common';

const SPLASH_MIN_DURATION_MS = 3000; // how long the logo stays fully visible
const SPLASH_FADE_MS = 500;          // fade-out transition length

@Component({
  selector: 'app-root',
  imports: [MobileComponent, DesktopComponent, NgOptimizedImage],
  templateUrl: './app.html',
  styleUrl: './app.scss'
})
export class App {
  protected readonly tg = inject(TelegramService);
  protected readonly auth = inject(AuthService);

  private readonly destroyRef = inject(DestroyRef);

  protected readonly showSplash = signal(true);
  protected readonly fadingOut = signal(false);
  protected readonly authError = signal<string | null>(null);

  protected readonly splashImage = computed(() =>
    this.tg.deviceType() === 'desktop'
      ? '/desktop.png'
      : '/phone.JPG',
  );

  constructor() {
    if (!this.tg.isSupportedPlatform()) {
      this.showSplash.set(false);
      return;
    }

    void this.initialize();
  }

  private async initialize(): Promise<void> {
    const splashTimer = new Promise<void>((resolve) => {
      const timer = setTimeout(resolve, SPLASH_MIN_DURATION_MS);

      this.destroyRef.onDestroy(() => clearTimeout(timer));
    });

    try {
      await Promise.all([
        this.auth.initialize(),
        splashTimer,
      ]);

      this.fadingOut.set(true);

      const removeTimer = setTimeout(() => {
        this.showSplash.set(false);
      }, SPLASH_FADE_MS);

      this.destroyRef.onDestroy(() => clearTimeout(removeTimer));
    } catch (error) {
      console.error('Authentication failed', error);

      this.authError.set(
        "We couldn't sign you in. Please reopen the app from Telegram.",
      );

      this.showSplash.set(false);
    }
  }
}
