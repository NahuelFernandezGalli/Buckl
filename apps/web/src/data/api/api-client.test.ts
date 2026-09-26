import { describe, expect, it, vi } from 'vitest'
import { SessionExpiredError } from '../../session/session'
import { createApiClient } from './api-client'

function setup(baseUrl = 'http://localhost:5080') {
  const fetch = vi.fn<typeof globalThis.fetch>().mockResolvedValue(new Response('[]'))
  const getAccessToken = vi.fn<() => Promise<string>>().mockResolvedValue('token-1')
  const client = createApiClient({ baseUrl, getAccessToken, fetch })
  const sent = () => {
    const [url, init] = fetch.mock.calls[0]
    return { url: String(url), init: init!, headers: new Headers(init!.headers) }
  }
  return { client, fetch, getAccessToken, sent }
}

describe('createApiClient', () => {
  it('sends the access token as a bearer token to the API', async () => {
    const { client, sent } = setup()

    await client.send('/garments?status=archived')

    expect(sent().url).toBe('http://localhost:5080/garments?status=archived')
    expect(sent().headers.get('Authorization')).toBe('Bearer token-1')
  })

  it('asks for a token on every request, so a renewed token is used', async () => {
    const { client, getAccessToken, fetch } = setup()
    getAccessToken.mockResolvedValueOnce('token-1').mockResolvedValueOnce('token-2')

    await client.send('/garments')
    await client.send('/garments')

    expect(new Headers(fetch.mock.calls[1][1]!.headers).get('Authorization')).toBe('Bearer token-2')
  })

  it('keeps the request method, body and headers', async () => {
    const { client, sent } = setup()

    await client.send('/garments', {
      method: 'POST',
      body: '{}',
      headers: { 'Content-Type': 'application/json' },
    })

    expect(sent().init.method).toBe('POST')
    expect(sent().init.body).toBe('{}')
    expect(sent().headers.get('Content-Type')).toBe('application/json')
  })

  it('keeps a path prefix of the base URL', async () => {
    const { client, sent } = setup('https://api.buckl.app/v1')

    await client.send('/garments')

    expect(sent().url).toBe('https://api.buckl.app/v1/garments')
  })

  it.each(['https://evil.example/garments', '//evil.example/garments', 'garments'])(
    'never sends the token to %s',
    async (path) => {
      const { client, fetch, getAccessToken } = setup()

      await expect(client.send(path)).rejects.toThrow('API paths start with a single "/"')
      expect(getAccessToken).not.toHaveBeenCalled()
      expect(fetch).not.toHaveBeenCalled()
    },
  )

  it('does not call the API when there is no token to send', async () => {
    const { client, fetch, getAccessToken } = setup()
    getAccessToken.mockRejectedValue(new SessionExpiredError())

    await expect(client.send('/garments')).rejects.toBeInstanceOf(SessionExpiredError)
    expect(fetch).not.toHaveBeenCalled()
  })
})
