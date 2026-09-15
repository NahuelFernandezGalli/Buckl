import { describe, expect, it } from 'vitest'
import { readFileAsDataUrl } from './files'

describe('readFileAsDataUrl', () => {
  it('encodes the file content with its media type', async () => {
    const file = new File(['hello'], 'hello.txt', { type: 'text/plain' })
    await expect(readFileAsDataUrl(file)).resolves.toBe('data:text/plain;base64,aGVsbG8=')
  })
})
