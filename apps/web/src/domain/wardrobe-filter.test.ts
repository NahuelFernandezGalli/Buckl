import { describe, expect, it } from 'vitest'
import { DEFAULT_WARDROBE_FILTER, hasCriteria, normalizeSearchText } from './wardrobe-filter'

describe('WardrobeFilter', () => {
  it('lists active garments with no other criteria by default', () => {
    expect(DEFAULT_WARDROBE_FILTER).toEqual({
      category: null,
      color: null,
      size: null,
      searchText: null,
      status: 'active',
    })
  })

  it('has no criteria when only the status is set', () => {
    expect(hasCriteria(DEFAULT_WARDROBE_FILTER)).toBe(false)
    expect(hasCriteria({ ...DEFAULT_WARDROBE_FILTER, status: 'archived' })).toBe(false)
  })

  it('has criteria when any of category, color, size or search text is set', () => {
    expect(hasCriteria({ ...DEFAULT_WARDROBE_FILTER, category: 'top' })).toBe(true)
    expect(hasCriteria({ ...DEFAULT_WARDROBE_FILTER, color: 'blue' })).toBe(true)
    expect(hasCriteria({ ...DEFAULT_WARDROBE_FILTER, size: 'M' })).toBe(true)
    expect(hasCriteria({ ...DEFAULT_WARDROBE_FILTER, searchText: 'oxford' })).toBe(true)
  })

  it('trims search text and turns blank into null', () => {
    expect(normalizeSearchText('  oxford ')).toBe('oxford')
    expect(normalizeSearchText('   ')).toBeNull()
    expect(normalizeSearchText(undefined)).toBeNull()
  })

  it('caps search text at 100 characters', () => {
    expect(normalizeSearchText('a'.repeat(120))).toHaveLength(100)
  })
})
