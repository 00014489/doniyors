import { inject, Injectable, NgZone, signal } from '@angular/core';
import { init, miniApp, retrieveLaunchParams, themeParams, User, viewport } from '@tma.js/sdk';

@Injectable({
  providedIn: 'root',
})
export class TelegramService {
  private ngZone = inject(NgZone);

  readonly user = signal<User | null>(null);
  readonly platform = signal<string>('unknown');

  initTelegram(): void {
    try {
      init();
    } catch (err) {
      console.error('❌ init() failed', err);
      return;
    }

    try { miniApp.mount(); } catch (err) { console.error('❌ miniApp.mount() failed', err); }
    try { viewport.mount(); } catch (err) { console.error('❌ viewport.mount() failed', err); }
    try { themeParams.mount(); } catch (err) { console.error('❌ themeParams.mount() failed', err); }

    try {
      const lp = retrieveLaunchParams();
      const platformValue = lp['platform'];
      const initData = lp['initData'] as { user?: User } | undefined;

      // Force this write to run inside Angular's zone
      this.ngZone.run(() => {
        this.platform.set(typeof platformValue === 'string' ? platformValue : 'unknown');
        this.user.set(initData?.user ?? null);
      });

      console.log('✅ retrieveLaunchParams() succeeded', lp, 'platform now:', this.platform());

    } catch (err) {
      console.error('❌ retrieveLaunchParams() failed', err);
    }
  }

  getPlatform(): string {
    return this.platform();
  }
}
