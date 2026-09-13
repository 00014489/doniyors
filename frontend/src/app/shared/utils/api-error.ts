import { HttpErrorResponse } from '@angular/common/http';
import { TranslateService } from '@ngx-translate/core';

/**
 * The reason a write was refused, in the user's language.
 *
 * The API answers rejected writes with `{ code, message, ...params }`. The code
 * is translated from "errors" in the i18n files, with the body's other fields
 * (a file name, an image limit) as parameters. A code this build does not know
 * falls back to the API's own English sentence, and anything else — a network
 * failure, an unexpected response — to `fallbackKey`.
 */
export function apiErrorMessage(
  translate: TranslateService,
  error: unknown,
  fallbackKey: string,
): string {
  if (error instanceof HttpErrorResponse) {
    const body = error.error as { code?: unknown; message?: unknown } | null;

    if (typeof body?.code === 'string') {
      const key = `errors.${body.code}`;
      const translated = translate.instant(key, body as Record<string, unknown>);

      if (translated !== key) {
        return translated;
      }
    }

    if (typeof body?.message === 'string' && body.message.trim()) {
      return body.message;
    }
  }

  return translate.instant(fallbackKey);
}
