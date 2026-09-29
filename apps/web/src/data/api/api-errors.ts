/**
 * An error answer of the Buckl API. Every one carries a stable `code`
 * (docs/architecture/api.md), which is what the app reacts to; the message is for developers and
 * is never shown as is.
 */
export class ApiError extends Error {
  readonly status: number
  readonly code: string

  constructor(status: number, code: string, detail?: string) {
    super(detail ?? `The API answered ${status} (${code}).`)
    this.name = 'ApiError'
    this.status = status
    this.code = code
  }
}

/** The request got no answer at all: offline, the API is down, or the browser blocked it. */
export class NetworkError extends Error {
  constructor(cause: unknown) {
    super('Could not reach the Buckl API.', { cause })
    this.name = 'NetworkError'
  }
}
