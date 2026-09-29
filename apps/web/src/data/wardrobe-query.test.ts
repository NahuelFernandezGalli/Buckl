import { describe, expect, it } from 'vitest'
import { DEFAULT_WARDROBE_FILTER } from '../domain/wardrobe-filter'
import { wardrobeQuery } from './wardrobe-query'

describe('wardrobeQuery', () => {
  it('always states the status', () => {
    expect(wardrobeQuery(DEFAULT_WARDROBE_FILTER)).toBe('status=active')
  })

  it('names every criterion like the API does', () => {
    expect(
      wardrobeQuery({
        category: 'top',
        color: 'blue',
        size: '42/44',
        searchText: 'oxford shirt',
        status: 'archived',
      }),
    ).toBe('category=top&color=blue&size=42%2F44&q=oxford+shirt&status=archived')
  })
})
