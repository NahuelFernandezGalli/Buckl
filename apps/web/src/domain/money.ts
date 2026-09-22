export interface Money {
  amount: number
  currency: string
}

/** Currencies offered in the form. The domain accepts any three-letter code; this is UI convenience. */
export const CURRENCIES = ['ARS', 'USD', 'EUR', 'UYU', 'BRL', 'CLP', 'GBP'] as const

/** Mirror of numeric(12,2) in the database. */
export const MAX_AMOUNT = 9_999_999_999.99
