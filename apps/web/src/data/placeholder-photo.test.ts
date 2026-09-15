import { describe, expect, it } from 'vitest'
import { placeholderPhoto } from './placeholder-photo'

describe('placeholderPhoto', () => {
  it('returns an inline SVG data URL', () => {
    expect(placeholderPhoto('blue')).toMatch(/^data:image\/svg\+xml,/)
  })

  it('paints the garment color', () => {
    expect(decodeURIComponent(placeholderPhoto('blue'))).toContain('#2f6fde')
  })

  it('is deterministic for the same color', () => {
    expect(placeholderPhoto('red')).toBe(placeholderPhoto('red'))
  })
})
