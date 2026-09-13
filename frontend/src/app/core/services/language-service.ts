import { registerLocaleData } from '@angular/common';
import localeRu from '@angular/common/locales/ru';
import localeUz from '@angular/common/locales/uz';
import { computed, inject, Service, signal } from '@angular/core';
import { TranslateService } from '@ngx-translate/core';

// Month and weekday names for dates; English ships with Angular.
registerLocaleData(localeRu);
registerLocaleData(localeUz);

/**
 * The languages the app is translated into — the same codes the API stores in
 * `Users.LanguageCode` and the bot reads.
 */
export type AppLanguage = 'uz' | 'ru' | 'en';

export const APP_LANGUAGES: readonly AppLanguage[] = ['uz', 'ru', 'en'];

/** Shown while a member has no language stored yet. */
export const DEFAULT_LANGUAGE: AppLanguage = 'en';

/** Each language in its own words, so anyone can find theirs whatever is on screen. */
export const LANGUAGE_NAMES: Readonly<Record<AppLanguage, string>> = {
  uz: "O'zbekcha",
  ru: 'Русский',
  en: 'English',
};

const LOCALE_IDS: Readonly<Record<AppLanguage, string>> = {
  uz: 'uz',
  ru: 'ru',
  en: 'en-US',
};

export function isAppLanguage(value: unknown): value is AppLanguage {
  return typeof value === 'string' && (APP_LANGUAGES as readonly string[]).includes(value);
}

/**
 * The language on screen. Its source of truth is the member's row in the
 * database: AuthService applies the stored value on sign-in and whenever the
 * app returns to the foreground, and the profile page saves a new choice
 * before it is shown. Nothing here keeps a copy of its own.
 */
@Service()
export class LanguageService {
  private readonly translate = inject(TranslateService);

  private readonly _language = signal<AppLanguage>(DEFAULT_LANGUAGE);

  readonly language = this._language.asReadonly();

  /** Locale id for formatting dates in the current language. */
  readonly locale = computed(() => LOCALE_IDS[this._language()]);

  constructor() {
    this.translate.addLangs([...APP_LANGUAGES]);
    this.translate.setFallbackLang(DEFAULT_LANGUAGE);

    this.apply(DEFAULT_LANGUAGE);
  }

  /**
   * Shows the language stored for the member. Anything else — including the
   * empty string of a member who has not chosen yet — shows the default.
   */
  setLanguage(code: string | null | undefined): void {
    this.apply(isAppLanguage(code) ? code : DEFAULT_LANGUAGE);
  }

  /**
   * Only for the moments before the stored language is known (the splash, a
   * failed sign-in): the Telegram client's locale, such as "ru" or "en-US", is
   * the best guess available then.
   */
  useClientHint(tag: string | null | undefined): void {
    const primary = (tag ?? '').split(/[-_]/)[0].toLowerCase();

    this.apply(isAppLanguage(primary) ? primary : DEFAULT_LANGUAGE);
  }

  private apply(language: AppLanguage): void {
    this._language.set(language);

    this.translate.use(language);

    document.documentElement.lang = language;
  }
}
