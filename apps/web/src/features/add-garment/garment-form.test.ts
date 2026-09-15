import { describe, expect, it } from 'vitest'
import { GarmentMother } from '../../test/garment-mother'
import {
  EMPTY_GARMENT_FORM,
  fromGarment,
  validateGarmentForm,
  type GarmentFormValues,
} from './garment-form'

const TODAY = '2026-09-15'

const valid: GarmentFormValues = {
  ...EMPTY_GARMENT_FORM,
  category: 'top',
  color: 'blue',
  size: ' M ',
  amount: '45000',
  currency: 'ARS',
  purchaseDate: '2026-03-15',
  notes: ' Bought in Madrid ',
}

const errorsOf = (values: GarmentFormValues) => {
  const result = validateGarmentForm(values, TODAY)
  return result.ok ? {} : result.errors
}

describe('validateGarmentForm', () => {
  it('turns valid values into a classification, purchase info and trimmed notes', () => {
    const result = validateGarmentForm(valid, TODAY)
    expect(result).toEqual({
      ok: true,
      output: {
        classification: { category: 'top', color: 'blue', size: 'M' },
        purchaseInfo: { price: { amount: 45000, currency: 'ARS' }, date: '2026-03-15' },
        notes: 'Bought in Madrid',
      },
    })
  })

  it('requires a category and a color', () => {
    expect(errorsOf(EMPTY_GARMENT_FORM)).toEqual({
      category: 'Choose a category',
      color: 'Choose a color',
    })
  })

  it('accepts a garment with no purchase information, size or notes', () => {
    const result = validateGarmentForm(
      { ...EMPTY_GARMENT_FORM, category: 'top', color: 'blue' },
      TODAY,
    )
    expect(result).toEqual({
      ok: true,
      output: {
        classification: { category: 'top', color: 'blue', size: null },
        purchaseInfo: null,
        notes: null,
      },
    })
  })

  it('caps the size label at 20 characters', () => {
    expect(errorsOf({ ...valid, size: 'x'.repeat(21) }).size).toBe(
      'Size can have at most 20 characters',
    )
  })

  it('needs both price and date once one of them is filled', () => {
    expect(errorsOf({ ...valid, purchaseDate: '' }).purchaseDate).toBe('Enter the purchase date')
    expect(errorsOf({ ...valid, amount: '' }).amount).toBe('Enter the price')
  })

  it('rejects negative, badly formatted and oversized prices', () => {
    expect(errorsOf({ ...valid, amount: '-1' }).amount).toBe('The price cannot be negative')
    expect(errorsOf({ ...valid, amount: '10.999' }).amount).toBe(
      'Enter a price with at most two decimals',
    )
    expect(errorsOf({ ...valid, amount: 'abc' }).amount).toBe(
      'Enter a price with at most two decimals',
    )
    expect(errorsOf({ ...valid, amount: '10000000000' }).amount).toBe('The price is too large')
  })

  it('accepts a comma as the decimal separator', () => {
    const result = validateGarmentForm({ ...valid, amount: '39,90' }, TODAY)
    expect(result.ok && result.output.purchaseInfo?.price.amount).toBe(39.9)
  })

  it('does not accept thousands separators', () => {
    expect(errorsOf({ ...valid, amount: '1.234,5' }).amount).toBe(
      'Enter a price with at most two decimals',
    )
  })

  it('asks for a well formatted price when only a minus sign is entered', () => {
    expect(errorsOf({ ...valid, amount: '-' }).amount).toBe(
      'Enter a price with at most two decimals',
    )
  })

  it('rejects a purchase date after today', () => {
    expect(errorsOf({ ...valid, purchaseDate: '2026-09-16' }).purchaseDate).toBe(
      'The purchase date cannot be in the future',
    )
    expect(errorsOf({ ...valid, purchaseDate: TODAY })).toEqual({})
  })

  it('caps notes at 500 characters', () => {
    expect(errorsOf({ ...valid, notes: 'x'.repeat(501) }).notes).toBe(
      'Notes can have at most 500 characters',
    )
  })
})

describe('fromGarment', () => {
  it('fills the form with the garment values', () => {
    const garment = GarmentMother.active({
      classification: { category: 'top', color: 'blue', size: 'M' },
      purchaseInfo: { price: { amount: 45000, currency: 'ARS' }, date: '2026-03-15' },
      notes: 'Bought in Madrid',
    })
    expect(fromGarment(garment)).toEqual({
      category: 'top',
      color: 'blue',
      size: 'M',
      amount: '45000',
      currency: 'ARS',
      purchaseDate: '2026-03-15',
      notes: 'Bought in Madrid',
    })
  })

  it('leaves optional fields empty and keeps the default currency', () => {
    const garment = GarmentMother.active({
      classification: { category: 'top', color: 'blue', size: null },
      purchaseInfo: null,
      notes: null,
    })
    expect(fromGarment(garment)).toEqual({ ...EMPTY_GARMENT_FORM, category: 'top', color: 'blue' })
  })
})
