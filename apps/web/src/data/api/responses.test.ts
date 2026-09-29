import { describe, expect, it } from 'vitest'
import { SessionExpiredError } from '../../session/session'
import { fakeFetch, jsonResponse, problemResponse, testApiClient } from '../../test/fake-fetch'
import { ApiError, NetworkError } from './api-errors'
import { requestJson } from './responses'

describe('requestJson', () => {
  it('resolves with the JSON body of a successful answer', async () => {
    const http = fakeFetch()
    http.on('GET /garments?status=active', () => jsonResponse([{ id: 'g-1' }]))

    await expect(requestJson(testApiClient(http), '/garments?status=active')).resolves.toEqual([
      { id: 'g-1' },
    ])
  })

  it('sends a body as JSON, and no content type when there is no body', async () => {
    const http = fakeFetch()
    http.on('POST /garments', () => jsonResponse({ id: 'g-1' }, 201))
    http.on('POST /garments/g-1/archive', () => jsonResponse({ id: 'g-1' }))
    const api = testApiClient(http)

    await requestJson(api, '/garments', { method: 'POST', body: { notes: 'Linen' } })
    await requestJson(api, '/garments/g-1/archive', { method: 'POST' })

    expect(http.requests[0].body).toEqual({ notes: 'Linen' })
    expect(http.requests[0].headers.get('Content-Type')).toBe('application/json')
    expect(http.requests[1].body).toBeUndefined()
    expect(http.requests[1].headers.has('Content-Type')).toBe(false)
  })

  it('turns problem details into an ApiError that carries their code', async () => {
    const http = fakeFetch()
    http.on('PATCH /garments/g-1', () => problemResponse(409, 'garment.archived_read_only'))

    const result = requestJson(testApiClient(http), '/garments/g-1', {
      method: 'PATCH',
      body: {},
    })

    await expect(result).rejects.toBeInstanceOf(ApiError)
    await expect(result).rejects.toMatchObject({
      status: 409,
      code: 'garment.archived_read_only',
    })
  })

  it('gives an error answer without problem details a code from its status', async () => {
    const http = fakeFetch()
    http.on(
      'GET /garments?status=active',
      () => new Response('<html>Bad gateway</html>', { status: 502 }),
    )
    http.on('GET /products/p-1', () => new Response('Too large', { status: 413 }))
    const api = testApiClient(http)

    await expect(requestJson(api, '/garments?status=active')).rejects.toMatchObject({
      status: 502,
      code: 'server.error',
    })
    await expect(requestJson(api, '/products/p-1')).rejects.toMatchObject({
      status: 413,
      code: 'request.rejected',
    })
  })

  it('reports a token the API rejects as an expired session', async () => {
    const http = fakeFetch()
    http.on('GET /garments?status=active', () => problemResponse(401, 'request.unauthenticated'))

    await expect(
      requestJson(testApiClient(http), '/garments?status=active'),
    ).rejects.toBeInstanceOf(SessionExpiredError)
  })

  it('reports an answer that never arrived as a network error', async () => {
    const http = fakeFetch()

    const result = requestJson(testApiClient(http), '/garments?status=active')

    await expect(result).rejects.toBeInstanceOf(NetworkError)
    await expect(result).rejects.toHaveProperty('cause', expect.any(TypeError))
  })

  it('lets an expired session through without calling the API', async () => {
    const http = fakeFetch()
    const api = testApiClient(http, async () => {
      throw new SessionExpiredError()
    })

    await expect(requestJson(api, '/garments?status=active')).rejects.toBeInstanceOf(
      SessionExpiredError,
    )
    expect(http.requests).toEqual([])
  })
})
