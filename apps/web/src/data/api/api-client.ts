export interface ApiClientOptions {
  /** Where the API lives, such as `http://localhost:5080`; a path prefix is kept. */
  baseUrl: string
  /** Usually `Session.getAccessToken`; asked once per request. */
  getAccessToken: () => Promise<string>
  fetch?: typeof fetch
}

/**
 * The only way the web app talks to the Buckl API: every request carries the user's access token,
 * and the resolved URL is checked to stay under the API root before the token is ever asked for.
 * Phase 6 builds the typed repositories on top of it.
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
      // A leading "/" is stripped and re-resolved against `root` rather than the raw string,
      // because WHATWG URL parsing can turn a path that merely starts with a single "/" into an
      // absolute, off-origin URL (a backslash is a path separator for special schemes, and a
      // scheme after the stripped slash makes the rest an absolute URL): re-checking the parsed
      // result's origin and prefix is what actually keeps the token on the API's own origin.
      const url = new URL(path.slice(1), root)
      if (url.origin !== root.origin || !url.href.startsWith(root.href)) {
        throw new Error(`API paths must stay under ${root.href}: ${path}`)
      }
      const token = await getAccessToken()
      const headers = new Headers(init.headers)
      headers.set('Authorization', `Bearer ${token}`)
      // Called as a plain function: browsers refuse fetch invoked on another object.
      return send(url, { ...init, headers })
    },
  }
}
