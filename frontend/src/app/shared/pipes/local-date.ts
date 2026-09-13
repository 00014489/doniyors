import { formatDate } from '@angular/common';
import { inject, Pipe, PipeTransform } from '@angular/core';

import { LanguageService } from '../../core/services/language-service';

/**
 * Angular's `date`, in the member's language: month and weekday names follow
 * the language stored for them, and change with it.
 *
 * Impure because the language can change while the value does not. Each
 * instance remembers its last result, so the work is only redone when the
 * value, the format or the language actually changes.
 */
@Pipe({
  name: 'localDate',
  pure: false,
})
export class LocalDatePipe implements PipeTransform {
  private readonly languageService = inject(LanguageService);

  private lastKey = '';
  private lastResult: string | null = null;

  transform(value: string | number | Date | null | undefined, format: string): string | null {
    if (value === null || value === undefined || value === '') {
      return null;
    }

    const locale = this.languageService.locale();
    const key = `${value instanceof Date ? value.getTime() : value}|${format}|${locale}`;

    if (key !== this.lastKey) {
      this.lastKey = key;
      this.lastResult = formatDate(value, format, locale);
    }

    return this.lastResult;
  }
}
