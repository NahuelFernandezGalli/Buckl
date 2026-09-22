import { describe, expect, it } from 'vitest'
import { GarmentMother } from '../test/garment-mother'
import { garmentTitle } from './garment-title'

describe('garmentTitle', () => {
  it('combines the color and the category in plain words', () => {
    const garment = GarmentMother.active({
      classification: { category: 'top', color: 'blue', size: 'M' },
    })
    expect(garmentTitle(garment)).toBe('Blue top')
  })

  it('handles multi-word labels', () => {
    const garment = GarmentMother.active({
      classification: { category: 'outerwear', color: 'multicolor', size: null },
    })
    expect(garmentTitle(garment)).toBe('Multicolor outerwear')
  })
})
