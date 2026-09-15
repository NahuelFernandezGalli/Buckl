import { describe, expect, it } from 'vitest'
import { formatMoney, formatPurchaseDate } from './format'

// Intl separates a currency code from the amount with a non-breaking space (\u00a0).
const plain = (text: string) => text.replace(/\u00a0/g, ' ')

describe('formatMoney', () => {
  it('shows the currency code when the locale has no symbol for it', () => {
    expect(plain(formatMoney({ amount: 45000, currency: 'ARS' }, 'en-US'))).toBe('ARS 45,000.00')
  })

  it('uses the symbol when the locale has one', () => {
    expect(formatMoney({ amount: 39.9, currency: 'USD' }, 'en-US')).toBe('$39.90')
    expect(formatMoney({ amount: 120, currency: 'EUR' }, 'en-US')).toBe('€120.00')
  })
})

describe('formatPurchaseDate', () => {
  it('formats a calendar date without shifting the day', () => {
    expect(formatPurchaseDate('2026-03-15', 'en-US')).toBe('Mar 15, 2026')
    expect(formatPurchaseDate('2025-12-31', 'en-US')).toBe('Dec 31, 2025')
    expect(formatPurchaseDate('2026-01-01', 'en-US')).toBe('Jan 1, 2026')
  })
})
