import { createApiClient, type ApiClient } from '../data/api/api-client'

export interface RecordedRequest {
  method: string
  url: string
  headers: Headers
  /** The parsed JSON of a string body; the raw body (a Blob, for an upload) otherwise. */
  body: unknown
}

type Responder = (request: RecordedRequest) => Response | Promise<Response>

/**
 * A `fetch` that answers from a route table and records every request, to test code that talks
 * HTTP without a server. A route is `"<METHOD> <path and query>"` on the API origin and
 * `"<METHOD> <absolute URL>"` anywhere else. A request without a route rejects the way a network
 * failure does, naming the route it looked for.
 */
export function fakeFetch(apiOrigin = 'http://api.test') {
  const routes = new Map<string, Responder>()
  const requests: RecordedRequest[] = []

  const fetch: typeof globalThis.fetch = async (input, init = {}) => {
    const url = new URL(input instanceof Request ? input.url : String(input))
    const request: RecordedRequest = {
      method: (init.method ?? 'GET').toUpperCase(),
      url: url.href,
      headers: new Headers(init.headers),
      body: typeof init.body === 'string' ? JSON.parse(init.body) : (init.body ?? undefined),
    }
    requests.push(request)
    const target = url.origin === apiOrigin ? `${url.pathname}${url.search}` : url.href
    const respond = routes.get(`${request.method} ${target}`)
    if (!respond) throw new TypeError(`fakeFetch has no route for ${request.method} ${target}`)
    return respond(request)
  }

  return {
    fetch,
    requests,
    on(route: string, respond: Responder) {
      routes.set(route, respond)
    },
  }
}

export type FakeFetch = ReturnType<typeof fakeFetch>

export const jsonResponse = (body: unknown, status = 200) =>
  new Response(JSON.stringify(body), {
    status,
    headers: { 'Content-Type': 'application/json' },
  })

export const problemResponse = (status: number, code: string) =>
  new Response(
    JSON.stringify({
      status,
      code,
      title: 'Problem',
      detail: `Problem ${code}`,
    }),
    {
      status,
      headers: { 'Content-Type': 'application/problem+json' },
    },
  )

/** The real API client over the fake, as the app builds it. */
export function testApiClient(
  http: FakeFetch,
  getAccessToken: () => Promise<string> = async () => 'test-access-token',
): ApiClient {
  return createApiClient({
    baseUrl: 'http://api.test',
    getAccessToken,
    fetch: http.fetch,
  })
}
