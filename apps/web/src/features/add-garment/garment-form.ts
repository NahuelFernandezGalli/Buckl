import {
  MAX_SIZE_LENGTH,
  type Category,
  type Classification,
  type Color,
} from '../../domain/classification'
import type { Garment } from '../../domain/garment'
import { MAX_NOTES_LENGTH } from '../../domain/garment'
import { MAX_AMOUNT } from '../../domain/money'
import type { PurchaseInfo } from '../../domain/purchase-info'

/** What the inputs hold: strings, as the DOM gives them. */
export interface GarmentFormValues {
  category: Category | ''
  color: Color | ''
  size: string
  amount: string
  currency: string
  purchaseDate: string
  notes: string
}

export const EMPTY_GARMENT_FORM: GarmentFormValues = {
  category: '',
  color: '',
  size: '',
  amount: '',
  currency: 'ARS',
  purchaseDate: '',
  notes: '',
}

export type GarmentFormErrors = Partial<
  Record<'category' | 'color' | 'size' | 'amount' | 'purchaseDate' | 'notes', string>
>

/** The validated, domain-shaped part of a submission. The photo is handled by the form itself. */
export interface GarmentFormOutput {
  classification: Classification
  purchaseInfo: PurchaseInfo | null
  notes: string | null
}

export type GarmentFormResult =
  { ok: true; output: GarmentFormOutput } | { ok: false; errors: GarmentFormErrors }

/** Prefills the form with a garment's current values, for editing. */
export function fromGarment(garment: Garment): GarmentFormValues {
  const { category, color, size } = garment.classification
  return {
    category,
    color,
    size: size ?? '',
    amount: garment.purchaseInfo ? String(garment.purchaseInfo.price.amount) : '',
    currency: garment.purchaseInfo?.price.currency ?? EMPTY_GARMENT_FORM.currency,
    purchaseDate: garment.purchaseInfo?.date ?? '',
    notes: garment.notes ?? '',
  }
}

const PRICE_PATTERN = /^\d+(\.\d{1,2})?$/

/** Mirrors the domain rules of Classification, Size, Money, PurchaseInfo and Garment.Notes. */
export function validateGarmentForm(values: GarmentFormValues, today: string): GarmentFormResult {
  const errors: GarmentFormErrors = {}
  const size = values.size.trim()
  const amount = values.amount.trim()
  const notes = values.notes.trim()

  if (!values.category) errors.category = 'Choose a category'
  if (!values.color) errors.color = 'Choose a color'
  if (size.length > MAX_SIZE_LENGTH) {
    errors.size = `Size can have at most ${MAX_SIZE_LENGTH} characters`
  }

  const hasPurchase = amount !== '' || values.purchaseDate !== ''
  if (hasPurchase) {
    if (amount === '') {
      errors.amount = 'Enter the price'
    } else if (amount.startsWith('-')) {
      errors.amount = 'The price cannot be negative'
    } else if (!PRICE_PATTERN.test(amount)) {
      errors.amount = 'Enter a price with at most two decimals'
    } else if (Number(amount) > MAX_AMOUNT) {
      errors.amount = 'The price is too large'
    }

    if (values.purchaseDate === '') {
      errors.purchaseDate = 'Enter the purchase date'
    } else if (values.purchaseDate > today) {
      errors.purchaseDate = 'The purchase date cannot be in the future'
    }
  }

  if (notes.length > MAX_NOTES_LENGTH) {
    errors.notes = `Notes can have at most ${MAX_NOTES_LENGTH} characters`
  }

  if (Object.keys(errors).length > 0 || !values.category || !values.color) {
    return { ok: false, errors }
  }

  return {
    ok: true,
    output: {
      classification: { category: values.category, color: values.color, size: size || null },
      purchaseInfo: hasPurchase
        ? {
            price: { amount: Number(amount), currency: values.currency },
            date: values.purchaseDate,
          }
        : null,
      notes: notes || null,
    },
  }
}
