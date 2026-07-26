import { computed, inject, Injectable, signal } from '@angular/core';
import { TranslateService } from '@ngx-translate/core';


export type AppLanguage = 'en' | 'ru' | 'uz';

@Injectable({
  providedIn: 'root',
})
export class LanguageService {
  private readonly translate = inject(TranslateService);

  private readonly _language = signal<AppLanguage>('en');

  readonly language = this._language.asReadonly();

  readonly isEnglish = computed(() => this._language() === 'en');
  readonly isRussian = computed(() => this._language() === 'ru');
  readonly isUzbek = computed(() => this._language() === 'uz');

  constructor() {
    this.translate.addLangs(['en', 'ru', 'uz']);
    this.translate.setFallbackLang('en');

    this.setLanguage('en');
  }

  setLanguage(lang: string): void {
    const normalized = this.normalize(lang);

    this._language.set(normalized);

    this.translate.use(normalized);
  }

  private normalize(lang: string): AppLanguage {
    if (!lang) {
      return 'en';
    }

    const lower = lang.toLowerCase();

    if (lower.startsWith('ru')) {
      return 'ru';
    }

    if (lower.startsWith('uz')) {
      return 'uz';
    }

    return 'en';
  }
}
