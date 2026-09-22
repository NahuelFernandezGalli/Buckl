import { describe, expect, it } from 'vitest'
import { readFileAsDataUrl } from './files'

describe('readFileAsDataUrl', () => {
  it('encodes the file content with its media type', async () => {
    const file = new File(['hello'], 'hello.txt', { type: 'text/plain' })
    await expect(readFileAsDataUrl(file)).resolves.toBe('data:text/plain;base64,aGVsbG8=')
  })

  it('encodes a blob that is not a file', async () => {
    const blob = new Blob(['hello'], { type: 'text/plain' })
    await expect(readFileAsDataUrl(blob)).resolves.toBe('data:text/plain;base64,aGVsbG8=')
  })
})
