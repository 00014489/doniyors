import { isPlatformBrowser } from '@angular/common';
import { computed, DestroyRef, effect, inject, Injectable, PLATFORM_ID, signal } from '@angular/core';

export interface TelegramWebAppUser {
  id: number;
  first_name: string;
  last_name?: string;
  username?: string;
  language_code?: string;
  is_premium?: boolean;
  photo_url?: string;
}

export interface TelegramWebAppInitData {
  user?: TelegramWebAppUser;
  auth_date: number;
  hash: string;
  start_param?: string;
}

/** Telegram's official theme palette for the user's current theme. */
export interface TelegramThemeParams {
  bg_color?: string;
  text_color?: string;
  hint_color?: string;
  link_color?: string;
  button_color?: string;
  button_text_color?: string;
  secondary_bg_color?: string;
  header_bg_color?: string;
  accent_text_color?: string;
  section_bg_color?: string;
  section_header_text_color?: string;
  subtitle_text_color?: string;
  destructive_text_color?: string;
}

export type TelegramColorScheme = 'light' | 'dark';

/** Official platform identifiers, per https://docs.telegram-mini-apps.com/platform/about */
export type TelegramPlatform =
  | 'android'
  | 'ios'
  | 'macos'
  | 'tdesktop'
  | 'web'
  | 'weba'
  | 'webk'
  | 'webz'
  | 'unigram'
  | 'unknown';

interface TelegramWebApp {
  initData: string;
  initDataUnsafe: TelegramWebAppInitData;
  platform: TelegramPlatform;
  colorScheme: TelegramColorScheme;
  themeParams: TelegramThemeParams;

  ready(): void;

  expand(): void;

  requestFullscreen?(): void;
  exitFullscreen?(): void;

  onEvent(eventType: string, cb: (...args: unknown[]) => void): void;
  offEvent(eventType: string, cb: (...args: unknown[]) => void): void;
}

declare global {
  interface Window {
    Telegram?: { WebApp: TelegramWebApp };
  }
}


@Injectable({
  providedIn: 'root',
})
export class TelegramService {
  private readonly destroyRef = inject(DestroyRef);
  private readonly isBrowser = isPlatformBrowser(inject(PLATFORM_ID));

  private readonly webApp =
    this.isBrowser ? window.Telegram?.WebApp ?? null : null;

  readonly available = computed(() => this.webApp !== null);

  readonly initData = computed(() => this.webApp?.initData ?? '');

  readonly user = computed(() => this.webApp?.initDataUnsafe.user);

  readonly platform = signal(this.webApp?.platform ?? 'unknown');

  readonly deviceType = computed<'mobile' | 'desktop'>(() => {
    switch (this.platform()) {
      case 'android':
      case 'ios':
        return 'mobile';

      default:
        return 'desktop';
    }
  });

  readonly isSupportedPlatform = computed(() => this.available());

  readonly unsupportedReason = computed(() =>
    this.available()
      ? null
      : 'Please open this application from Telegram.',
  );

  readonly colorScheme = signal(this.webApp?.colorScheme ?? 'light');

  readonly themeParams = signal(this.webApp?.themeParams ?? {});

  readonly isDark = computed(() => this.colorScheme() === 'dark');

  constructor() {
    if (!this.webApp) {
      console.warn('Telegram WebApp SDK not found.');
      return;
    }

    this.webApp.ready();

    console.log('Telegram platform:', this.webApp.platform);
    console.log('Telegram initData:', this.webApp.initData);
    console.log('Telegram user:', this.webApp.initDataUnsafe.user);

    const onThemeChanged = () => {
      this.colorScheme.set(this.webApp!.colorScheme);
      this.themeParams.set(this.webApp!.themeParams);
    };

    this.webApp.onEvent('themeChanged', onThemeChanged);

    this.destroyRef.onDestroy(() =>
      this.webApp?.offEvent('themeChanged', onThemeChanged),
    );

    effect(() => {
      const root = document.documentElement.style;

      for (const [key, value] of Object.entries(this.themeParams())) {
        if (value) {
          root.setProperty(`--tg-${key.replace(/_/g, '-')}`, value);
        }
      }

      root.setProperty(
        '--tg-text-color',
        this.isDark() ? '#ffffff' : '#000000',
      );

      document.documentElement.dataset['tgColorScheme'] =
        this.colorScheme();
    });

    effect(() => {
      if (!this.available()) {
        return;
      }

      if (this.deviceType() === 'desktop') {
        this.enterFullscreen();
      } else {
        this.exitFullscreen();
      }
    });
  }

  private enterFullscreen(): void {
    this.webApp?.requestFullscreen?.();
  }

  private exitFullscreen(): void {
    this.webApp?.exitFullscreen?.();
  }
}
