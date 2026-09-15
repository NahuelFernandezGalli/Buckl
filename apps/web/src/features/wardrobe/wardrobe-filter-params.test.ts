import { describe, expect, it } from 'vitest'
import { DEFAULT_WARDROBE_FILTER } from '../../domain/wardrobe-filter'
import { clearCriteria, parseWardrobeFilter } from './wardrobe-filter-params'

const parse = (query: string) => parseWardrobeFilter(new URLSearchParams(query))

describe('parseWardrobeFilter', () => {
  it('is the default filter when the address has no parameters', () => {
    expect(parse('')).toEqual(DEFAULT_WARDROBE_FILTER)
  })

  it('reads category, color, size, search text and status', () => {
    expect(parse('category=top&color=blue&size=M&q=oxford&status=archived')).toEqual({
      category: 'top',
      color: 'blue',
      size: 'M',
      searchText: 'oxford',
      status: 'archived',
    })
  })

  it('ignores unknown categories, colors and statuses', () => {
    expect(parse('category=hat&color=teal&status=deleted')).toEqual(DEFAULT_WARDROBE_FILTER)
  })

  it('trims text and treats blank as absent', () => {
    expect(parse('q=%20oxford%20&size=%20')).toEqual({
      ...DEFAULT_WARDROBE_FILTER,
      searchText: 'oxford',
    })
  })

  it('caps the size label at 20 characters', () => {
    expect(parse(`size=${'x'.repeat(30)}`).size).toHaveLength(20)
  })
})

describe('clearCriteria', () => {
  it('drops category, color, size and search text but keeps the status', () => {
    const params = new URLSearchParams('category=top&color=blue&size=M&q=oxford&status=archived')
    expect(clearCriteria(params).toString()).toBe('status=archived')
  })

  it('returns a copy and leaves the given parameters untouched', () => {
    const params = new URLSearchParams('category=top')
    clearCriteria(params)
    expect(params.toString()).toBe('category=top')
  })
})
