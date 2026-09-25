import { describe, expect, it } from 'vitest'
import { safeReturnTo } from './return-to'

describe('safeReturnTo', () => {
  it.each(['/wardrobe', '/wardrobe/g-1/edit', '/wardrobe?category=top&q=blue', '/garments/new'])(
    'keeps the app path %s',
    (path) => {
      expect(safeReturnTo(path)).toBe(path)
    },
  )

  it.each([
    ['nothing', undefined],
    ['a number', 42],
    ['an empty string', ''],
    ['a relative path', 'wardrobe'],
    ['another site', 'https://evil.example/wardrobe'],
    ['a protocol-relative URL', '//evil.example'],
    ['a backslash trick', '/\\evil.example'],
    ['the welcome screen', '/welcome'],
    ['the sign-in callback', '/callback?code=abc&state=xyz'],
  ])('falls back to the wardrobe for %s', (_label, value) => {
    expect(safeReturnTo(value)).toBe('/wardrobe')
  })
})
