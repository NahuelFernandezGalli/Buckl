import type { Money } from './money'

export interface PurchaseInfo {
  price: Money
  /** Calendar date as YYYY-MM-DD, mirror of DateOnly. Never a timestamp. */
  date: string
}
