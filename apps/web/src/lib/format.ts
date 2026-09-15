import type { Money } from '../domain/money'

export function formatMoney(money: Money, locale: string = navigator.language): string {
  return new Intl.NumberFormat(locale, { style: 'currency', currency: money.currency }).format(
    money.amount,
  )
}

/**
 * Formats a YYYY-MM-DD purchase date. The parts are used as a local calendar date on purpose:
 * parsing the string with `new Date('2026-03-15')` would read it as UTC midnight and show the
 * previous day west of Greenwich.
 */
export function formatPurchaseDate(date: string, locale: string = navigator.language): string {
  const [year, month, day] = date.split('-').map(Number)
  return new Intl.DateTimeFormat(locale, { dateStyle: 'medium' }).format(
    new Date(year, month - 1, day),
  )
}
