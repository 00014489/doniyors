import { HttpClient } from '@angular/common/http';
import { computed, inject, Injectable, signal } from '@angular/core';
import { TelegramService } from './telegram-service';
import { firstValueFrom } from 'rxjs';
import { LanguageService } from './language-service';
import { environment } from '../../environments/environment';

interface LoginResponse {
  token: string;
  languageCode: string;
}

interface JwtPayload {
  UserId: string;
  TypeUser: string;
  exp: number;
}

@Injectable({
  providedIn: 'root',
})
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly telegram = inject(TelegramService);
  private readonly languageService = inject(LanguageService);

  private readonly api = `${environment.apiUrl}/auth/telegram`;

  readonly authenticated = signal(false);
  readonly userId = signal<number | null>(null);
  readonly typeUser = signal<number | null>(null);

  readonly isAdmin = computed(() => this.typeUser() === 1);

  async initialize(): Promise<void> {
    const storedToken = sessionStorage.getItem('jwt');

    if (storedToken && this.isTokenValid(storedToken)) {
      this.readClaims(storedToken);

      this.languageService.setLanguage(sessionStorage.getItem('languageCode') ?? 'en');

      this.authenticated.set(true);

      return;
    }

    if (storedToken) {
      // Stale/expired token — clear it before re-authenticating.
      this.clearSession();
    }

    const initData = this.telegram.initData();

    if (!initData) {
      throw new Error('Telegram initData is missing.');
    }

    const response = await firstValueFrom(
      this.http.post<LoginResponse>(this.api, { initData }),
    );

    sessionStorage.setItem('jwt', response.token);
    sessionStorage.setItem('languageCode', response.languageCode);

    this.readClaims(response.token);

    this.languageService.setLanguage(response.languageCode);

    this.authenticated.set(true);
  }

  logout(): void {
    this.clearSession();

    this.authenticated.set(false);

    this.languageService.setLanguage('en');
  }

  get token(): string | null {
    return sessionStorage.getItem('jwt');
  }

  get languageCode(): string {
    return this.languageService.language();
  }

  private clearSession(): void {
    sessionStorage.removeItem('jwt');
    sessionStorage.removeItem('languageCode');

    this.userId.set(null);
    this.typeUser.set(null);
  }

  /** Pure check — no side effects. Caller decides what to do with an invalid token. */
  private isTokenValid(token: string): boolean {
    try {
      const payload = this.decodeToken(token);
      const now = Math.floor(Date.now() / 1000);

      return payload.exp > now;
    } catch {
      return false;
    }
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
