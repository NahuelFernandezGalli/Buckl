export interface ApiClientOptions {
  /** Where the API lives, such as `http://localhost:5080`; a path prefix is kept. */
  baseUrl: string
  /** Usually `Session.getAccessToken`; asked once per request. */
  getAccessToken: () => Promise<string>
  fetch?: typeof fetch
}

/**
 * The only way the web app talks to the Buckl API: every request carries the user's access token,
 * and only ever to the API's own origin. Phase 6 builds the typed repositories on top of it.
 */
export interface ApiClient {
  /** `path` is relative to the API root and starts with a single `/`: `/garments?status=active`. */
  send(path: string, init?: RequestInit): Promise<Response>
}

export function createApiClient({
  baseUrl,
  getAccessToken,
  fetch: send = globalThis.fetch,
}: ApiClientOptions): ApiClient {
  const root = new URL(baseUrl.endsWith('/') ? baseUrl : `${baseUrl}/`)

  return {
    async send(path, init = {}) {
      if (!path.startsWith('/') || path.startsWith('//')) {
        throw new Error(`API paths start with a single "/": ${path}`)
      }
      const token = await getAccessToken()
      const headers = new Headers(init.headers)
      headers.set('Authorization', `Bearer ${token}`)
      // Called as a plain function: browsers refuse fetch invoked on another object.
      return send(new URL(path.slice(1), root), { ...init, headers })
    },
  }
}
