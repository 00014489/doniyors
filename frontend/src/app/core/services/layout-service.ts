import { computed, inject, Injectable } from '@angular/core';
import { TelegramService } from './telegram-service';

@Injectable({
  providedIn: 'root',
})
export class LayoutService {
  private telegram = inject(TelegramService);

  private readonly mobileNative = ['android', 'android_x', 'ios'];
  private readonly desktopNative = ['tdesktop', 'macos', 'linux'];
  private readonly webPlatforms = ['web', 'weba', 'webk'];

  readonly isMobile = computed(() => {
    const platform = this.telegram.platform();

    if (this.mobileNative.includes(platform)) return true;
    if (this.webPlatforms.includes(platform)) return this.isMobileViewport();

    return false;
  });

  readonly isDesktop = computed(() => !this.isMobile());

  readonly isReady = computed(() => this.telegram.platform() !== 'unknown');

  private isMobileViewport(): boolean {
    return window.innerWidth < 768; // or your preferred breakpoint
  }
}
