import { describe, expect, it } from 'vitest'
import { safeReturnTo } from './return-to'

describe('safeReturnTo', () => {
  it.each([
    '/',
    '/wardrobe',
    '/wardrobe/g-1/edit',
    '/wardrobe?category=top&q=blue',
    '/garments/new',
  ])('keeps the app path %s', (path) => {
    expect(safeReturnTo(path)).toBe(path)
  })

  it.each([
    ['nothing', undefined],
    ['a number', 42],
    ['an empty string', ''],
    ['a relative path', 'wardrobe'],
    ['another site', 'https://evil.example/wardrobe'],
    ['a protocol-relative URL', '//evil.example'],
    ['a backslash trick', '/\\evil.example'],
    ['a tab trick', '/\t/evil.example'],
    ['a newline trick', '/\n/evil.example'],
    ['a carriage-return trick', '/\r/evil.example'],
    ['the welcome screen', '/welcome'],
    ['the welcome screen with a trailing slash', '/welcome/'],
    ['the welcome screen in another case', '/Welcome'],
    ['the welcome screen with a hash', '/welcome#top'],
    ['the sign-in callback', '/callback?code=abc&state=xyz'],
    ['the sign-in callback with a trailing slash', '/callback/'],
    ['the sign-in callback with no query', '/callback'],
  ])('falls back to the wardrobe for %s', (_label, value) => {
    expect(safeReturnTo(value)).toBe('/wardrobe')
  })
})
