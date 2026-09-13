import { HttpClient } from '@angular/common/http';
import { computed, inject, Service, signal } from '@angular/core';
import { firstValueFrom } from 'rxjs';

import { environment } from '../../environments/environment';
import { AppLanguage, LanguageService } from './language-service';
import { TelegramService } from './telegram-service';

interface LoginResponse {
  token: string;
  languageCode: string;
}

interface LanguageResponse {
  languageCode: string;
}

interface JwtPayload {
  UserId: string;
  TypeUser: string;
  exp: number;
}

const TOKEN_KEY = 'jwt';

@Service()
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly telegram = inject(TelegramService);
  private readonly languageService = inject(LanguageService);

  private readonly loginUrl = `${environment.apiUrl}/auth/telegram`;
  private readonly languageUrl = `${environment.apiUrl}/profile/language`;

  readonly authenticated = signal(false);
  readonly userId = signal<number | null>(null);
  readonly typeUser = signal<number | null>(null);

  /** TypeUser 1 = SuperAdmin, 2 = Admin — both may use the admin panel. */
  readonly isAdmin = computed(() => {
    const type = this.typeUser();

    return type === 1 || type === 2;
  });

  /** The sign-in in flight, shared by every caller that needs a token at once. */
  private signingIn: Promise<string> | null = null;

  private watchingForeground = false;

  /**
   * Signs in on start-up. This always asks the API, even with a token left in
   * session storage: the answer carries the member's stored language, and a
   * copy kept from earlier may be stale — the bot's language menu or the admin
   * panel can have changed it since.
   */
  async initialize(): Promise<void> {
    // Until the API answers, the Telegram client's locale is the only hint.
    this.languageService.useClientHint(this.telegram.user()?.language_code);

    await this.refresh();

    this.watchForeground();
  }

  /**
   * Signs in again with Telegram's initData and resolves with the new token.
   * The token lives an hour but initData stays valid for a day, so the
   * interceptor uses this to recover a Mini App left open past the hour.
   */
  refresh(): Promise<string> {
    this.signingIn ??= this.signIn().finally(() => {
      this.signingIn = null;
    });

    return this.signingIn;
  }

  /** The sign-in call itself must never trigger a sign-in. */
  isSignInRequest(url: string): boolean {
    return url === this.loginUrl;
  }

  /** Re-reads the stored language, for when the app comes back into view. */
  async syncLanguage(): Promise<void> {
    if (!this.authenticated()) {
      return;
    }

    try {
      const response = await firstValueFrom(
        this.http.get<LanguageResponse>(this.languageUrl),
      );

      this.languageService.setLanguage(response.languageCode);
    } catch {
      // Keep what is on screen; the next return to the app tries again.
    }
  }

  /**
   * The member's own choice. Saved before it is shown, so the screen never
   * displays a language the database does not hold. The API relabels the bot's
   * menu button too, and the bot reads the same column.
   */
  async changeLanguage(language: AppLanguage): Promise<void> {
    const response = await firstValueFrom(
      this.http.put<LanguageResponse>(this.languageUrl, { languageCode: language }),
    );

    this.languageService.setLanguage(response.languageCode);
  }

  logout(): void {
    this.clearSession();

    this.authenticated.set(false);
  }

  get token(): string | null {
    return sessionStorage.getItem(TOKEN_KEY);
  }

  private async signIn(): Promise<string> {
    const initData = this.telegram.initData();

    if (!initData) {
      throw new Error('Telegram initData is missing.');
    }

    const response = await firstValueFrom(
      this.http.post<LoginResponse>(this.loginUrl, { initData }),
    );

    sessionStorage.setItem(TOKEN_KEY, response.token);

    this.readClaims(response.token);

    this.languageService.setLanguage(response.languageCode);

    this.authenticated.set(true);

    return response.token;
  }

  /**
   * A member can change their language in the bot while the Mini App waits in
   * the background, so the stored value is read again whenever it returns.
   */
  private watchForeground(): void {
    if (this.watchingForeground) {
      return;
    }

    this.watchingForeground = true;

    document.addEventListener('visibilitychange', () => {
      if (document.visibilityState === 'visible') {
        void this.syncLanguage();
      }
    });

    // Telegram's own signal: a minimised Mini App does not always fire
    // visibilitychange when it is restored.
    this.telegram.onActivated(() => void this.syncLanguage());
  }

  private clearSession(): void {
    sessionStorage.removeItem(TOKEN_KEY);

    this.userId.set(null);
    this.typeUser.set(null);
  }

  private readClaims(token: string): void {
    const payload = this.decodeToken(token);

    this.userId.set(Number(payload.UserId));
    this.typeUser.set(Number(payload.TypeUser));
  }

  private decodeToken(token: string): JwtPayload {
    const base64Url = token.split('.')[1];

    if (!base64Url) {
      throw new Error('Malformed token.');
    }

    // JWT payloads are base64url — convert to base64 and restore padding before atob().
    const base64 = base64Url.replace(/-/g, '+').replace(/_/g, '/');
    const padded = base64.padEnd(base64.length + ((4 - (base64.length % 4)) % 4), '=');

    return JSON.parse(atob(padded)) as JwtPayload;
  }
}
