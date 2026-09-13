import { Pipe, PipeTransform } from '@angular/core';

/** The only currency this product deals in. */
export const CURRENCY_CODE = 'UZS';

/**
 * Formats a money amount as UZS: `450 000 UZS`.
 *
 * Grouping is a plain space and the code is a suffix, which is how sums are
 * written in Uzbekistan — and being locale-independent it renders identically
 * in all three app languages, so a member switching language sees the price
 * they saw a moment ago.
 *
 * Points are deliberately not routed through this: they are a score, not money.
 */
@Pipe({
  name: 'money',
})
export class MoneyPipe implements PipeTransform {
  transform(value: number | string | null | undefined): string {
    const amount = Number(value ?? 0);

    if (!Number.isFinite(amount)) {
      return `0 ${CURRENCY_CODE}`;
    }

    const negative = amount < 0;
    const [whole, fraction] = Math.abs(amount).toFixed(2).split('.');

    // Thousands separated by a space: 450000 -> "450 000".
    const grouped = whole.replace(/\B(?=(\d{3})+(?!\d))/g, ' ');

    // Whole sums are the norm; only show the cents when there are any.
    const decimals = fraction === '00' ? '' : `,${fraction}`;

    return `${negative ? '-' : ''}${grouped}${decimals} ${CURRENCY_CODE}`;
  }
}
